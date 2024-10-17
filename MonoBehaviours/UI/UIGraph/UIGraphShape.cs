using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DingoUnityExtensions.MonoBehaviours.UI.UIGraph
{
    public struct UIGraphShape
    {
        public static bool IsNull(in UIGraphShape graphShape) => graphShape.Points.Count == 0;
        public static readonly UIGraphShape NullShape = new(Array.Empty<Vector2>(), Color.clear);

        public readonly IReadOnlyList<Vector2> Points;
        public readonly Vector2 Center;
        public readonly float Thickness;
        public readonly ShapeType ShapeType;
        
        private readonly Color32 _color;
        private readonly Gradient _gradient;

        public readonly Color32 EvaluateColor(float value)
        {
            if (_gradient == null)
                return _color;
            var alphaKeys = _gradient.alphaKeys;
            if (alphaKeys.All(a => a.alpha <= float.Epsilon))
                return _color;

            return _color * _gradient.Evaluate(value);
        }

        private UIGraphShape(IReadOnlyList<Vector2> points, Color color, Gradient gradient, float thickness, bool isLine)
        {
            Points = points;
            _color = color;
            _gradient = gradient;
            Thickness = Math.Max(thickness, -1);
            Center = points.Aggregate(Vector2.zero, (v1, v2) => v1 + v2) / points.Count;
            if (isLine)
                ShapeType = ShapeType.Line;
            else
                ShapeType = thickness <= Vector2.kEpsilon ? ShapeType.FilledShape : ShapeType.StrokeShape;
        }
        
        public UIGraphShape(IReadOnlyList<Vector2> points, Color color, float thickness = -1, bool isLine = false) 
            : this(points, color, null, thickness, isLine)
        {
        }
        
        public UIGraphShape(IReadOnlyList<Vector2> points, Gradient gradient, float thickness = -1, bool isLine = false) 
            : this(points, Color.clear, gradient, thickness, isLine)
        {
        }
    }
}