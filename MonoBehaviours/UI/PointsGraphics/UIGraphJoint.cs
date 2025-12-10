using System;
using DingoUnityExtensions.MonoBehaviours.UI.PointsGraphics.Core;
using DingoUnityExtensions.MonoBehaviours.UI.UIGraph;
using NaughtyAttributes;
using UnityEngine;

namespace DingoUnityExtensions.MonoBehaviours.UI.PointsGraphics
{
    public class UIGraphJoint : UIJoint, IThicknessComponent, IRedrawable
    {
        [SerializeField] private UIGraph.UIGraph_Old _graphOld;
        [SerializeField] private Color _color = Color.white;
        [SerializeField, Min(0)] private float _thickness;
        [SerializeField, Min(0.001f)] private float _step = 0.1f;

        public float Thickness
        {
            get => _thickness;
            set => _thickness = Math.Clamp(value, 0.001f, Radius);
        }
        
        public float RadiusWithoutThickness => Math.Max(Radius - _thickness, 0);

        public override Color Color
        {
            get => _color;
            set
            {
                value.a = Alpha;
                _color = value;
                _graphOld.color = value;
            }
        }
        
#if VINSPECTOR_EXISTS
        [VInspector.Button]
#else
        [NaughtyAttributes.Button]
#endif
        public override void Redraw()
        {
            base.Redraw();
            if (_graphOld.IsDirty)
                return;
            Color = _color;
            _graphOld.Thickness = _thickness;
            DrawUVSphere(0.5f - 0.5f * (_thickness / Radius * 0.5f), new Vector2(0.5f, 0.5f), _step, _thickness, Color, _graphOld);
            _graphOld.SetDirty();
        }

        private void Reset()
        {
            _graphOld = GetComponent<UIGraph.UIGraph_Old>();
        }

        private static void DrawUVSphere(float uvRadius, Vector2 uvCenter, float step, float thickness, Color color, UIGraph.UIGraph_Old graphOld)
        {
            graphOld.Clear();
            var points = MathUtils.CalculateCircle(uvRadius, uvCenter, step);
            graphOld.AddGraphShape(new UIGraphShape(points, color, thickness));
        }
    }
}