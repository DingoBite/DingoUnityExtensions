using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bind;
using DingoUnityExtensions.Utils;
using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_EDITOR && EMULATE_DELAYS
using Random = UnityEngine.Random;
#endif

namespace DingoUnityExtensions.ImageLoadGlobalSystem
{
    public static class ImageLoadGlobalCache
    {
        private sealed class CacheEntry
        {
            public readonly Bind<TextureLoadData> Flow = new();
            public readonly HashSet<object> Receivers = new();

            public CancellationTokenSource LoadCts;
            public int Generation;
        }

        private static readonly Dictionary<string, CacheEntry> Entries = new();
        private static readonly Dictionary<object, string> ReceiverToPath = new();

        public static Bind<TextureLoadData> GetOrRegister(string path)
        {
            if (!Entries.TryGetValue(path, out var entry))
            {
                entry = new CacheEntry();
                Entries.Add(path, entry);
            }

            return entry.Flow;
        }

        public static void Link(string path, object receiver, bool disableLogException = false)
        {
            if (string.IsNullOrWhiteSpace(path) || receiver == null)
                return;

            UnLink(receiver);
            var entry = GetOrCreateEntry(path);
            CleanupDeadReceivers(entry);

            entry.Receivers.Add(receiver);
            ReceiverToPath[receiver] = path;

            entry.Flow.V = entry.Flow.V;

            if (entry.Flow.V.State == ImageLoadState.None)
                StartLoad(entry, path, disableLogException);
        }

        public static void UnLink(object receiver)
        {
            if (receiver == null)
                return;

            if (!ReceiverToPath.Remove(receiver, out var path))
                return;

            if (!Entries.TryGetValue(path, out var entry))
                return;

            entry.Receivers.Remove(receiver);
            CleanupDeadReceivers(entry);

            if (entry.Receivers.Count > 0)
                return;

            entry.Generation++;
            CancelLoad(entry);

            if (entry.Flow.V.State == ImageLoadState.Loaded && entry.Flow.V.Texture != null)
                Object.Destroy(entry.Flow.V.Texture);

            entry.Flow.V = TextureLoadData.None;

            Entries.Remove(path);
        }

        private static CacheEntry GetOrCreateEntry(string path)
        {
            if (!Entries.TryGetValue(path, out var entry))
            {
                entry = new CacheEntry();
                Entries.Add(path, entry);
            }

            return entry;
        }

        private static void StartLoad(CacheEntry entry, string path, bool disableLogException)
        {
            entry.Generation++;
            var gen = entry.Generation;
            CancelLoad(entry);
            var cts = new CancellationTokenSource();
            entry.LoadCts = cts;

            entry.Flow.V = new TextureLoadData(null, ImageLoadState.Loading, path);

            _ = LoadImageAsync(entry, gen, path, disableLogException, cts);
        }

        private static async Task LoadImageAsync(CacheEntry entry, int gen, string path, bool disableLogException, CancellationTokenSource cts)
        {
            var cancellationToken = cts.Token;
            Texture2D texture = null;
            
            try
            {
#if UNITY_EDITOR && EMULATE_DELAYS
                await Task.Delay((int)(Random.value * 3000f), cancellationToken);
#endif
                texture = await MultiplatformLoadUtils.LoadTexture2DAsync(path, disableLogException, cancellationToken);

                if (cancellationToken.IsCancellationRequested)
                    return;

                if (entry.Generation != gen || entry.Receivers.Count == 0)
                {
                    return;
                }

                if (texture == null)
                    entry.Flow.V = new TextureLoadData(null, ImageLoadState.NotFound, path);
                else
                    entry.Flow.V = new TextureLoadData(texture, ImageLoadState.Loaded, path);

                texture = null;
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception e)
            {
                if (!disableLogException)
                {
                    Debug.LogError(path);
                    Debug.LogException(e);
                }

                if (!cancellationToken.IsCancellationRequested && entry.Generation == gen && entry.Receivers.Count > 0)
                    entry.Flow.V = new TextureLoadData(null, ImageLoadState.NotFound, path);
            }
            finally
            {
                if (texture != null)
                    Object.Destroy(texture);

                if (ReferenceEquals(entry.LoadCts, cts))
                {
                    entry.LoadCts = null;
                    cts.Dispose();
                }
            }
        }

        private static void CancelLoad(CacheEntry entry)
        {
            var cts = entry.LoadCts;
            if (cts == null)
                return;

            entry.LoadCts = null;

            try
            {
                if (!cts.IsCancellationRequested)
                    cts.Cancel();
            }
            catch (ObjectDisposedException)
            {
            }

            cts.Dispose();
        }

        private static void CleanupDeadReceivers(CacheEntry entry)
        {
            if (entry.Receivers.Count == 0)
                return;

            List<object> dead = null;

            foreach (var r in entry.Receivers)
            {
                if (r is Object uo && uo == null)
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
                ReceiverToPath.Remove(r);
            }
        }
    }

    public class ImageLoadHandle
    {
        public const string IMAGE_LOAD_HANDLE = "IMAGE_LOAD_HANDLE";
        public string Path { get; private set; }

        private readonly Bind<TextureLoadData> _textureFlow;
        private readonly bool _disableLogException;
        public IReadonlyBind<TextureLoadData> TextureFlow => _textureFlow;

        public ImageLoadHandle(string path, bool disableLogException = false)
        {
            _disableLogException = disableLogException;
            Path = path;
            if (Path == null)
                return;

            _textureFlow = ImageLoadGlobalCache.GetOrRegister(Path);
        }

        public void LoadFor(object receiver)
        {
            if (string.IsNullOrWhiteSpace(Path))
            {
                Debug.LogErrorFormat("Cannot load image path == null/empty");
                return;
            }

            ImageLoadGlobalCache.Link(Path, receiver, _disableLogException);
        }

        public void UnloadFor(object receiver)
        {
            ImageLoadGlobalCache.UnLink(receiver);
        }
    }
}
