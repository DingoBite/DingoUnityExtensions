using System;
using DingoUnityExtensions.MonoBehaviours.UI.PointsGraphics.Core;
using UnityEngine;

namespace DingoUnityExtensions.MonoBehaviours.UI.PointsGraphics
{
    public class UIGraphCurve : UIBehaviour, IThicknessComponent, IColorComponent, IClearable, IAlphaComponent
    {
        [SerializeField] private UIGraph.UIGraph _graph;
        [SerializeField] private Color _color;
        [SerializeField, Range(0, 1)] private float _alpha;
        [SerializeField, Min(0)] private float _thickness;
        [SerializeField] private float _step = 0.01f;

        public float Thickness
        {
            get => _graph.Thickness;
            set
            {
                _thickness = Math.Max(value, 0);
                _graph.Thickness = _thickness;
            }
        }

        public Color Color
        {
            get => _color;
            set
            {
                _color = value;
                _graph.color = value;
            }
        }

        public float Alpha
        {
            get => _alpha;
            set
            {
                _alpha = value;
                var c = Color;
                c.a = value;
                Color = c;
            }
        }

        public void DrawElasticLine(Vector2 p1, float r1, Vector2 p2, float r2, Rect? rect = null)
        {
            _graph.Clear();
            var points = MathUtils.CalculateElasticPoint(p1, r1, p2, r2, _step);
            foreach (var point in points)
            {
                var uvPoint = rect == null ? point : new Vector2(point.x / rect.Value.width, point.y / rect.Value.height);
                _graph.AddPoint(uvPoint);
            }
            _graph.SetDirty();
        }
        
        public void Clear()
        {
            _graph.Clear();
        }

        private void OnValidate()
        {
            Color = _color;
            Alpha = _alpha;
            Thickness = _thickness;
        }
    }
}