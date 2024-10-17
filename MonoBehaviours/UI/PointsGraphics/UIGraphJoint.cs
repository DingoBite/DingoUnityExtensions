using System;
using DingoUnityExtensions.MonoBehaviours.UI.PointsGraphics.Core;
using DingoUnityExtensions.MonoBehaviours.UI.UIGraph;
using NaughtyAttributes;
using UnityEngine;

namespace DingoUnityExtensions.MonoBehaviours.UI.PointsGraphics
{
    public class UIGraphJoint : UIJoint, IThicknessComponent, IRedrawable
    {
        [SerializeField] private UIGraph.UIGraph _graph;
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
                _graph.color = value;
            }
        }


        [Button]
        public override void Redraw()
        {
            base.Redraw();
            if (_graph.IsDirty)
                return;
            Color = _color;
            _graph.Thickness = _thickness;
            DrawUVSphere(0.5f - 0.5f * (_thickness / Radius * 0.5f), new Vector2(0.5f, 0.5f), _step, _thickness, Color, _graph);
            _graph.SetDirty();
        }

        private void Reset()
        {
            _graph = GetComponent<UIGraph.UIGraph>();
        }

        private static void DrawUVSphere(float uvRadius, Vector2 uvCenter, float step, float thickness, Color color, UIGraph.UIGraph graph)
        {
            graph.Clear();
            var points = MathUtils.CalculateCircle(uvRadius, uvCenter, step);
            graph.AddGraphShape(new UIGraphShape(points, color, thickness));
        }
    }
}