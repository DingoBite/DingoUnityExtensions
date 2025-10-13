using UnityEngine;
using UnityEngine.UI;
using UIBehaviour = DingoUnityExtensions.MonoBehaviours.UI.UIBehaviour;

namespace DingoUnityExtensions.Art
{
    [ExecuteAlways]
    public class RectTransformSizeShaderProvider : UIBehaviour
    {
        private static readonly int RectSizeID = Shader.PropertyToID("_RectSize");
        private static readonly int PivotId = Shader.PropertyToID("_Pivot");

        [SerializeField] private Image _image;
        [SerializeField] private Canvas _canvas;
        
        private Vector2 _lastPxSize = new(-1f, -1f);
        private Vector2 _lastPivot = new(-1f, -1f);
        private float _lastScaleFactor = -1f;

        protected override void OnEnable()
        {
            base.OnEnable();
            _canvas = GetComponentInParent<Canvas>();
            Canvas.willRenderCanvases += OnWillRenderCanvases;
            UpdateNow(true);
        }

        protected override void OnDisable()
        {
            Canvas.willRenderCanvases -= OnWillRenderCanvases;
            base.OnDisable();
        }

        protected override void OnCanvasHierarchyChanged()
        {
            base.OnCanvasHierarchyChanged();
            _canvas = GetComponentInParent<Canvas>();
        }

        private void OnWillRenderCanvases()
        {
            UpdateNow(false);
        }

        private void UpdateNow(bool force)
        {
            if (_image == null)
                return;

            if (_canvas == null)
                _canvas = GetComponentInParent<Canvas>();

            var scaleFactor = _canvas == null ? 1f : _canvas.scaleFactor;
            var pxSize = RectTransform.rect.size * scaleFactor;
            var pivot = RectTransform.pivot;

            if (!force && _lastPxSize == pxSize && _lastPivot == pivot && Mathf.Approximately(_lastScaleFactor, scaleFactor))
                return;

            _lastPxSize = pxSize;
            _lastPivot = pivot;
            _lastScaleFactor = scaleFactor;

            var mat = _image.materialForRendering;
            mat.SetVector(RectSizeID, new Vector4(pxSize.x, pxSize.y, 0f, 0f));
            mat.SetVector(PivotId, new Vector4(pivot.x, pivot.y, 0f, 0f));
        }
    }
}
