using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DingoUnityExtensions.MonoBehaviours.Singletons;
using UnityEngine;

namespace DingoUnityExtensions.ImageLoadGlobalSystem
{
    public class ImageGlobalLoadManager : SingletonBehaviour<ImageGlobalLoadManager>
    {
        [SerializeField] private float _enableRefresh = 0.1f;
        [SerializeField] private float _disableRefresh = 1;

        [SerializeField] private int _byFrameDisable = 5;
        [SerializeField] private int _byFrameEnable = 5;
        
        private readonly HashSet<ImageLoadWrapper> _wrappersToDisable = new();
        private readonly HashSet<ImageLoadWrapper> _wrappersToEnable = new();

        private readonly List<ImageLoadWrapper> _cacheWrappersToDisable = new();
        private readonly List<ImageLoadWrapper> _cacheWrappersToEnable = new();
        
        private float _enableTime;
        private float _disableTime;

        public void Enable(ImageLoadWrapper wrapper)
        {
            CoroutineParent.AddLateUpdater((this, 0), EnableRefresh);
            _wrappersToDisable.Remove(wrapper);
            _wrappersToEnable.Add(wrapper);
        }

        public void Disable(ImageLoadWrapper wrapper)
        {
            CoroutineParent.AddLateUpdater((this, 1), DisableRefresh);
            _wrappersToDisable.Add(wrapper);
        }

        private IEnumerator ManualEnableRefresh()
        {
            while (_wrappersToEnable.Count > 0)
            {
                _cacheWrappersToEnable.Clear();
                if (_byFrameEnable > 0)
                {
                    _cacheWrappersToEnable.AddRange(_wrappersToEnable.Take(_byFrameEnable));
                    _wrappersToEnable.RemoveWhere(w => _cacheWrappersToEnable.Contains(w));
                }
                else
                {
                    _cacheWrappersToEnable.AddRange(_wrappersToEnable);
                    _wrappersToEnable.Clear();
                }
                
                foreach (var wrapper in _cacheWrappersToEnable)
                {
                    wrapper.Value?.LoadPath(wrapper);
                }

                yield return null;
            }
        }

        private IEnumerator ManualDisableRefresh()
        {
            while (_wrappersToDisable.Count > 0)
            {
                _cacheWrappersToDisable.Clear();
                if (_byFrameEnable > 0)
                {
                    _cacheWrappersToDisable.AddRange(_wrappersToDisable.Take(_byFrameDisable));
                    _wrappersToDisable.RemoveWhere(w => _cacheWrappersToDisable.Contains(w));
                }
                else
                {
                    _cacheWrappersToDisable.AddRange(_wrappersToDisable);
                    _wrappersToDisable.Clear();
                }
                
                foreach (var wrapper in _cacheWrappersToDisable)
                {
                    wrapper.Value?.Unload(wrapper);
                }
                yield return null;
            }
        }

        private void DisableRefresh()
        {
            if (_disableTime < _disableRefresh)
            {
                _disableTime += Time.deltaTime;
                return;
            }

            _disableTime = 0;
            CoroutineParent.StartCoroutineWithCanceling((this, 3), ManualDisableRefresh);
        }
        
        private void EnableRefresh()
        {
            if (_enableTime < _enableRefresh)
            {
                _enableTime += Time.deltaTime;
                return;
            }

            _enableTime = 0f;
            CoroutineParent.StartCoroutineWithCanceling((this, 2), ManualEnableRefresh);
        }
    }
}