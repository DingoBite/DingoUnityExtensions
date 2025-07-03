using UnityEngine;

namespace DingoUnityExtensions.MonoBehaviours.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class UIBehaviour : UnityEngine.EventSystems.UIBehaviour
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