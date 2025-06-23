#if ADDRESSABLES_EXISTS
using System;
using System.Collections.Generic;
using Bind;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace DingoUnityExtensions.Addressables
{
    public class AssetLoaderWrapper<T> where T : Object
    {
        private readonly Bind<T> _asset = new();
        private readonly List<string> _lods;
        
        private int _lodLevel = -1;

        public bool Loading { get; private set; }
        public AsyncOperationHandle<T> Handle { get; private set; }
        public IReadonlyBind<T> Asset => _asset;

        public AssetLoaderWrapper(List<string> lods)
        {
            _lods = lods;
        }

        public void Free()
        {
            Loading = false;
            if (Handle.IsValid())
                Handle.ReleaseHandleOnCompletion();

            _asset.V = null;
            Handle = default;
        }

        public void Load(int lodLevel = 0)
        {
            if ((Handle.IsValid() && Handle.IsDone || Loading) && _lodLevel == lodLevel)
                return;
            if (_lodLevel != lodLevel && Handle.IsValid())
                Handle.ReleaseHandleOnCompletion();
            
            _lodLevel = Math.Clamp(lodLevel, 0, _lods.Count - 1);
            Loading = true;

            Handle = _lods[_lodLevel].Load<T>();
            Handle.Completed -= LoadCompleted;
            Handle.Completed += LoadCompleted;
        }

        private void LoadCompleted(AsyncOperationHandle<T> handle)
        {
            Loading = false;
            _asset.V = handle.Result;
        }
    }
}
#endif