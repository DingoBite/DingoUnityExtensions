using DingoUnityExtensions.MonoBehaviours.UI;
using UnityEngine;
using UnityEngine.UI;

namespace DingoUnityExtensions.Art
{
    [ExecuteAlways]
    public class RectTransformSizeShaderProvider : UIBehaviour
    {
        private static readonly int RectSizeID = Shader.PropertyToID("_RectSize");
        private static readonly int PivotId = Shader.PropertyToID("_Pivot");

        [SerializeField] private Image _image;

        private MaterialPropertyBlock _materialPropertyBlock;
        private Material _mat;
        private Canvas _canvas;

        protected override void OnRectTransformDimensionsChange()
        {
            if (_mat == null)
                _mat = _image.material;
            if (_canvas == null)
                _canvas = GetComponentInParent<Canvas>();
            var scaleFactor = _canvas == null ? 1f : _canvas.scaleFactor;
            var pxSize = RectTransform.rect.size * scaleFactor;
            var pivot = RectTransform.pivot;
            _mat.SetVector(RectSizeID, new Vector4(pxSize.x, pxSize.y, 0, 0));
            _mat.SetVector(PivotId, new Vector4(pivot.x, pivot.y, 0, 0));
        }

        protected override void OnEnable() => OnRectTransformDimensionsChange();
    }
}