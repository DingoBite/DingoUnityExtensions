using DingoUnityExtensions.Extensions;
using DingoUnityExtensions.MonoBehaviours;
using UnityEngine;

namespace DingoUnityExtensions.ImageLoadGlobalSystem
{
    public class ImageLoadWrapperAutoLoadManager : PopulateParent<ImageLoadWrapper>
    {
        [SerializeField] private RectTransform _viewPort;
        [SerializeField] private int _skipFrames;

        private int _skippedFrames;
        
        private void OnLateUpdate()
        {
            if (_viewPort == null)
                return;
            if (_skipFrames > 0)
            {
                _skippedFrames++;
                if (_skippedFrames < _skipFrames)
                    return;
                _skippedFrames = 0;
            }
            
            foreach (var imageLoadWrapper in GetComponents())
            {
                if (_viewPort.WorldOverlaps(imageLoadWrapper.RectTransform))
                    imageLoadWrapper.Load();
                else 
                    imageLoadWrapper.Unload();
            }
        }

        private void Reset() => _viewPort = GetComponent<RectTransform>();

        private void OnBecameVisible()
        {
            _skippedFrames = 0;
            CoroutineParent.AddLateUpdater(this, OnLateUpdate);
        }

        private void OnBecameInvisible() => CoroutineParent.RemoveLateUpdater(this);
    }
}