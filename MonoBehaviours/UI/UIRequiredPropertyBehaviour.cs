using UnityEngine;

namespace DingoUnityExtensions.MonoBehaviours.UI
{
    [RequireComponent(typeof(RectTransform))]
    public abstract class UIRequiredPropertyBehaviour<T> : RequiredPropertyBehaviour<T> where T : Component
    {
        private RectTransform _rectTransform;

        public RectTransform RectTransform => _rectTransform == null ? FindRectTransform() : _rectTransform;
        
        private RectTransform FindRectTransform()
        {
            _rectTransform = GetComponent<RectTransform>();
            return _rectTransform;
        }
    }
}