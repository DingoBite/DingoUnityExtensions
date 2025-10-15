using UnityEngine;
using UnityEngine.UI;
using UIBehaviour = DingoUnityExtensions.MonoBehaviours.UI.UIBehaviour;

namespace DingoUnityExtensions.Art
{
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public class RectTransformSizeShaderProvider : UIBehaviour, IMaterialModifier
    {
        private static readonly int RectSizeID = Shader.PropertyToID("_RectSize");
        private static readonly int PivotID = Shader.PropertyToID("_Pivot");

        [SerializeField] private Graphic _graphic;
        
        private Material _runtimeMat;

        protected override void OnEnable()
        {
            base.OnEnable();
            if (!_graphic)
                _graphic = GetComponent<Graphic>();
            MarkDirty();
        }

        protected override void OnDisable()
        {
            ReleaseRuntimeMaterial();
            MarkDirty();
            base.OnDisable();
        }

        protected override void OnCanvasHierarchyChanged() => MarkDirty();
        protected override void OnRectTransformDimensionsChange() => MarkDirty();
#if UNITY_EDITOR
        protected override void OnValidate()
        {
            if (isActiveAndEnabled)
                MarkDirty();
        }
#endif

        public void MarkDirty()
        {
            if (_graphic)
                _graphic.SetMaterialDirty();
        }

        private void ReleaseRuntimeMaterial()
        {
            if (_runtimeMat)
            {
#if UNITY_EDITOR
                DestroyImmediate(_runtimeMat);
#else
                Destroy(_runtimeMat);
#endif
                _runtimeMat = null;
            }
        }

        public Material GetModifiedMaterial(Material baseMat)
        {
            if (!isActiveAndEnabled || baseMat == null)
                return baseMat;

            if (_runtimeMat == null || _runtimeMat.shader != baseMat.shader)
            {
                ReleaseRuntimeMaterial();
                _runtimeMat = new Material(baseMat) { hideFlags = HideFlags.DontSave };
            }
            else
            {
                _runtimeMat.CopyPropertiesFromMaterial(baseMat);
            }

            var rt = (RectTransform)transform;
            var canvas = _graphic ? _graphic.canvas : null;
            var scale = canvas ? canvas.scaleFactor : 1f;

            var px = rt.rect.size * scale;
            var pivot = rt.pivot;

            _runtimeMat.SetVector(RectSizeID, new Vector4(px.x, px.y, 0, 0));
            _runtimeMat.SetVector(PivotID, new Vector4(pivot.x, pivot.y, 0, 0));
            return _runtimeMat;
        }
    }
}
