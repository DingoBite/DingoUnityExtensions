using UnityEngine;
using UnityEngine.UI;

namespace DingoUnityExtensions.MonoBehaviours.UI
{
    [RequireComponent(typeof(AspectRatioFitter))]
    public class AspectRatioFitterRawImage : UIBehaviour
    {
        [SerializeField] private RawImage _rawImage; 
        [SerializeField] private AspectRatioFitter _aspectRatioFitter;

        public void UpdateRawImage(Texture texture)
        {
            _rawImage.texture = texture;
            UpdateAspect();
        }

        public void UpdateAspect()
        {
            if (_rawImage.texture == null)
                return;
            if (!_aspectRatioFitter.gameObject.activeInHierarchy && !enabled)
            {
                RectTransform.offsetMin = Vector2.zero;
                RectTransform.offsetMax = Vector2.zero;
                return;
            }
            var width = _rawImage.texture.width;
            var height = _rawImage.texture.height;
            _aspectRatioFitter.aspectRatio = (float)height / width;
        }

        private void Reset()
        {
            TryGetComponent(out _rawImage);
            TryGetComponent(out _aspectRatioFitter);
        }
    }
}