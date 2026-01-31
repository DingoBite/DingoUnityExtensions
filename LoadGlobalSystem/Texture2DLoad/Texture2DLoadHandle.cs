using System;
using System.Collections.Generic;
using Bind;
using UnityEngine;

namespace DingoUnityExtensions.LoadGlobalSystem.Texture2DLoad
{
    public sealed class Texture2DLoadHandle
    {
        public string Path { get; private set; }
        public Texture2DLoadInfo Info { get; private set; }

        private readonly GlobalAssetCache<Texture2DCacheKey, Texture2D, Texture2DLoadInfo> _cache;
        private readonly Bind<AssetLoadData<Texture2D, Texture2DLoadInfo>> _proxyFlow = new(AssetLoadData<Texture2D, Texture2DLoadInfo>.None);
        private IReadonlyBind<AssetLoadData<Texture2D, Texture2DLoadInfo>> _sourceFlow;
        private readonly HashSet<object> _activeReceivers = new();

        public IReadonlyBind<AssetLoadData<Texture2D, Texture2DLoadInfo>> Flow => _proxyFlow;

        public Texture2DLoadHandle(string path, Texture2DLoadInfo info = default, GlobalAssetCache<Texture2DCacheKey, Texture2D, Texture2DLoadInfo> cache = null)
        {
            Path = path;
            Info = info;
            _cache = cache ?? Texture2DGlobal.Cache;
        }

        public void LoadFor(object receiver) => LoadFor(receiver, false);
        
        public void LoadFor(object receiver, bool forceReload)
        {
            if (receiver == null)
                Debug.LogException(new NullReferenceException(nameof(receiver)));
            if (string.IsNullOrWhiteSpace(Path))
                Debug.LogException(new ArgumentException("Path is null/empty.", nameof(Path)));

            _activeReceivers.Add(receiver);

            var srcReadonly = _cache.Acquire(Path, Info, receiver, forceReload);

            if (!ReferenceEquals(_sourceFlow, srcReadonly))
            {
                DetachFromSource();
                _sourceFlow = srcReadonly;
                _sourceFlow.AddListener(_proxyFlow.SetValue);
            }

            _proxyFlow.V = _sourceFlow.V;
        }

        public void UnloadFor(object receiver)
        {
            if (receiver == null)
                return;

            _cache.Release(receiver);
            _activeReceivers.Remove(receiver);

            if (_activeReceivers.Count == 0)
            {
                DetachFromSource();
                _proxyFlow.V = AssetLoadData<Texture2D, Texture2DLoadInfo>.None;
            }
        }

        public void Invalidate()
        {
            if (string.IsNullOrWhiteSpace(Path))
                return;

            _cache.Invalidate(Path, Info);
        }

        public void Set(string newPath, Texture2DLoadInfo newInfo = default, bool dropToNone = true)
        {
            Path = newPath;
            Info = newInfo;

            if (dropToNone)
                _proxyFlow.V = AssetLoadData<Texture2D, Texture2DLoadInfo>.None;
        }

        private void DetachFromSource()
        {
            _sourceFlow = null;
        }
    }
}