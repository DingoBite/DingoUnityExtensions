using System;
using System.Collections.Generic;
using System.Threading;
using Bind;
using Cysharp.Threading.Tasks;

namespace DingoUnityExtensions.LoadGlobalSystem
{
    public enum AssetLoadState
    {
        None,
        Loading,
        NotFound,
        Loaded,
        Failed
    }

    public readonly struct AssetLoadData<TAsset, TInfo> where TAsset : class
    {
        public readonly string Path;
        public readonly TInfo Info;
        public readonly TAsset Asset;
        public readonly AssetLoadState State;
        public readonly Exception Error;

        public AssetLoadData(string path, TInfo info, TAsset asset, AssetLoadState state, Exception error = null)
        {
            Path = path;
            Info = info;
            Asset = asset;
            State = state;
            Error = error;
        }

        public static AssetLoadData<TAsset, TInfo> None => new(null, default, null, AssetLoadState.None);
        public static AssetLoadData<TAsset, TInfo> NotFound => new(null, default, null, AssetLoadState.NotFound);
    }

    public interface IAssetLoader<TAsset, in TInfo> where TAsset : class
    {
        UniTask<TAsset> LoadAsync(string path, TInfo info, CancellationToken ct);
    }

    public interface IAssetReleaser<in TAsset, in TInfo> where TAsset : class
    {
        void Release(TAsset asset, string path, TInfo info);
    }

    public interface ICacheKeyFactory<out TKey, in TInfo>
    {
        TKey CreateKey(string path, TInfo info);
    }


    public sealed class GlobalAssetCache<TKey, TAsset, TInfo> where TAsset : class
    {
        private sealed class Entry
        {
            public readonly Bind<AssetLoadData<TAsset, TInfo>> Flow = new();
            public readonly HashSet<object> Receivers = new();

            public string Path;
            public TInfo Info;

            public int Generation;
            public CancellationTokenSource Cts;
        }

        private readonly Dictionary<TKey, Entry> _entries;
        private readonly Dictionary<object, TKey> _receiverToKey;

        private readonly ICacheKeyFactory<TKey, TInfo> _keyFactory;
        private readonly IAssetLoader<TAsset, TInfo> _loader;
        private readonly IAssetReleaser<TAsset, TInfo> _releaser;

        private readonly Func<object, bool> _isReceiverAlive;

        public GlobalAssetCache(ICacheKeyFactory<TKey, TInfo> keyFactory, IAssetLoader<TAsset, TInfo> loader, IAssetReleaser<TAsset, TInfo> releaser, IEqualityComparer<TKey> keyComparer = null, Func<object, bool> isReceiverAlive = null)
        {
            _keyFactory = keyFactory ?? throw new ArgumentNullException(nameof(keyFactory));
            _loader = loader ?? throw new ArgumentNullException(nameof(loader));
            _releaser = releaser ?? throw new ArgumentNullException(nameof(releaser));

            _entries = new Dictionary<TKey, Entry>(keyComparer);
            _receiverToKey = new Dictionary<object, TKey>();

            _isReceiverAlive = isReceiverAlive ?? (_ => true);
        }
        
        public IReadonlyBind<AssetLoadData<TAsset, TInfo>> Acquire(string path, TInfo info, object receiver, bool forceReload = false)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("path is null/empty", nameof(path));
            if (receiver == null)
                throw new ArgumentNullException(nameof(receiver));

            Release(receiver);
            var key = _keyFactory.CreateKey(path, info);
            var entry = GetOrCreateEntry(key, path, info);
            PruneDeadReceivers(entry);
            entry.Receivers.Add(receiver);
            _receiverToKey[receiver] = key;
            entry.Flow.V = entry.Flow.V;
            if (forceReload || entry.Flow.V.State == AssetLoadState.None)
                StartLoad(key, entry);

            return entry.Flow;
        }

        public void Release(object receiver)
        {
            if (receiver == null)
                return;

            if (!_receiverToKey.Remove(receiver, out var key))
                return;

            if (!_entries.TryGetValue(key, out var entry))
                return;

            entry.Receivers.Remove(receiver);
            PruneDeadReceivers(entry);

            if (entry.Receivers.Count > 0)
                return;

            entry.Generation++;
            entry.Cts?.Cancel();
            entry.Cts?.Dispose();
            entry.Cts = null;

            var current = entry.Flow.V;
            if (current.State == AssetLoadState.Loaded && current.Asset != null)
                _releaser.Release(current.Asset, entry.Path, entry.Info);

            entry.Flow.V = AssetLoadData<TAsset, TInfo>.None;

            _entries.Remove(key);
        }

        public void Invalidate(string path, TInfo info)
        {
            var key = _keyFactory.CreateKey(path, info);
            if (!_entries.TryGetValue(key, out var entry))
                return;

            if (entry.Receivers.Count > 0)
            {
                StartLoad(key, entry);
                return;
            }

            entry.Generation++;
            entry.Cts?.Cancel();
            entry.Cts?.Dispose();
            entry.Cts = null;

            var current = entry.Flow.V;
            if (current.State == AssetLoadState.Loaded && current.Asset != null)
                _releaser.Release(current.Asset, entry.Path, entry.Info);

            entry.Flow.V = AssetLoadData<TAsset, TInfo>.None;
            _entries.Remove(key);
        }

        public void PruneAll()
        {
            List<TKey> toRemove = null;

            foreach (var kv in _entries)
            {
                var entry = kv.Value;
                PruneDeadReceivers(entry);

                if (entry.Receivers.Count == 0 && entry.Flow.V.State != AssetLoadState.None)
                {
                    toRemove ??= new List<TKey>();
                    toRemove.Add(kv.Key);
                }
            }

            if (toRemove == null)
                return;

            foreach (var k in toRemove)
            {
                var entry = _entries[k];

                entry.Generation++;
                entry.Cts?.Cancel();
                entry.Cts?.Dispose();

                var current = entry.Flow.V;
                if (current.State == AssetLoadState.Loaded && current.Asset != null)
                    _releaser.Release(current.Asset, entry.Path, entry.Info);

                entry.Flow.V = AssetLoadData<TAsset, TInfo>.None;
                _entries.Remove(k);
            }
        }

        private Entry GetOrCreateEntry(TKey key, string path, TInfo info)
        {
            if (_entries.TryGetValue(key, out var entry))
                return entry;

            entry = new Entry
            {
                Path = path,
                Info = info
            };
            entry.Flow.V = AssetLoadData<TAsset, TInfo>.None;

            _entries.Add(key, entry);
            return entry;
        }

        private void StartLoad(TKey key, Entry entry)
        {
            entry.Generation++;
            var gen = entry.Generation;

            entry.Cts?.Cancel();
            entry.Cts?.Dispose();
            entry.Cts = new CancellationTokenSource();

            entry.Flow.V = new AssetLoadData<TAsset, TInfo>(entry.Path, entry.Info, null, AssetLoadState.Loading);

            LoadRoutine(key, entry, gen, entry.Cts.Token).Forget();
        }

        private async UniTaskVoid LoadRoutine(TKey key, Entry entry, int gen, CancellationToken ct)
        {
            TAsset asset = null;
            Exception error = null;

            try
            {
                asset = await _loader.LoadAsync(entry.Path, entry.Info, ct);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                error = ex;
            }

            if (!_entries.TryGetValue(key, out var current) || !ReferenceEquals(current, entry) || entry.Generation != gen || entry.Receivers.Count == 0)
            {
                if (asset != null)
                    _releaser.Release(asset, entry.Path, entry.Info);
                return;
            }

            if (error != null)
            {
                entry.Flow.V = new AssetLoadData<TAsset, TInfo>(entry.Path, entry.Info, null, AssetLoadState.Failed, error);
                return;
            }

            if (asset == null)
            {
                entry.Flow.V = new AssetLoadData<TAsset, TInfo>(entry.Path, entry.Info, null, AssetLoadState.NotFound);
                return;
            }

            entry.Flow.V = new AssetLoadData<TAsset, TInfo>(entry.Path, entry.Info, asset, AssetLoadState.Loaded);
        }

        private void PruneDeadReceivers(Entry entry)
        {
            if (entry.Receivers.Count == 0)
                return;

            List<object> dead = null;
            foreach (var r in entry.Receivers)
            {
                if (!_isReceiverAlive(r))
                {
                    dead ??= new List<object>();
                    dead.Add(r);
                }
            }

            if (dead == null)
                return;

            foreach (var r in dead)
            {
                entry.Receivers.Remove(r);
                _receiverToKey.Remove(r);
            }
        }
    }
}