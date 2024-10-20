using System;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace DingoUnityExtensions.MonoBehaviours.UI.UIGraph
{
    public class UIGraph : MaskableGraphic
    {
        private static readonly Vector2 NullPoint = new(-1e6f, -1e6f);
        private static bool IsNullPoint(in Vector2 point) => point == NullPoint;
        
        private static readonly int StencilComp = Shader.PropertyToID("_StencilComp");

        [SerializeField] private bool _xStepFromCount;
        [SerializeField] private List<Vector2> _uvPoints = new();
        [SerializeField] private float _thickness;
        [SerializeField] private Gradient _gradient = new(){alphaKeys = new GradientAlphaKey[]{new(0,0), new(0, 1)}};
        [SerializeField] private Gradient _underGraphGradient = new(){alphaKeys = new GradientAlphaKey[]{new(0,0), new(0, 1)}};
        [SerializeField] private bool _underGraphGradientNormalize;
        
        [SerializeField] private bool _isSmooth;
        [SerializeField] private int _smoothMode = 4;
        [SerializeField, ShowIf(nameof(_isSmooth))] private float _xStep = 0.1f;

        [SerializeField] private bool _debugElements;

        [SerializeField] private float _overlappingPercent = 1;
        [SerializeField] private bool _overlapJump;
        [SerializeField] private bool _clampUVValues = true;

        [SerializeField] private double _pointsMergeThreshold = 1e-3;
        [SerializeField] private bool _smallYChangeMerge;
        [SerializeField] private int _indexOffsetForRemove = 128;

        [SerializeField] private CompareFunction _parentMaskCompareFunction = CompareFunction.Equal;
        
        [SerializeField] private Material _invertedMaskMaterial;
        [SerializeField] private AnimationCurve _smoothCurve = new();

        private readonly List<UIGraphShape> _graphShapes = new(64);

        private float XStep => Math.Max(_xStep, 0.001f);

        public IReadOnlyList<Vector2> UVPoints => _uvPoints;
        public Gradient Gradient => _gradient;
        public bool IsDirty { get; private set; }

        public float Thickness
        {
            get => _thickness;
            set
            {
                _thickness = Math.Max(value, 0);
                SetDirty();
            }
        }
        
        public override Material materialForRendering
        {
            get
            {
                try
                {
                    _invertedMaskMaterial.SetInt(StencilComp, (int)_parentMaskCompareFunction);
                }
                catch (Exception e)
                {
                    _invertedMaskMaterial = new Material(base.materialForRendering);
                    _invertedMaskMaterial.SetInt(StencilComp, (int)_parentMaskCompareFunction);
#if UNITY_EDITOR
                    Debug.LogWarning(e);
#endif
                }
                return _invertedMaskMaterial;
            }
        }
        
        private readonly List<Vector2> _cacheUVGraphicPoints = new(1024);

        private float _width;
        private float _height;

        private Vector2 _cachedNormal1;
        private Vector2 _cachedNormal2;

        private float _maxX;
        private float _xOffset;
        private int _indexOffset;

        private int _lastCalculatedCount;
        private Vector2 _origin;

        public Rect Rect => rectTransform.rect;

        public void SetUnderGraphGradient(Gradient gradient)
        {
            if (gradient != null)
                _underGraphGradient = gradient;
        }
        
        public void SetPoints(IEnumerable<Vector2> uvPoints)
        {
            _uvPoints.Clear();
            foreach (var uvPoint in uvPoints)
            {
                AddPoint(uvPoint);
            }
        }
        
        /// <summary>
        /// On Clearing UVPoints Array, index will be broken
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        public void SetPointUnsafe(int index, Vector2 value)
        {
            if (index >= _uvPoints.Count)
                return;
            if (index < 0)
                index = _uvPoints.Count + index;
            if (index < 0)
                return;
            
            if (_clampUVValues)
                value.x = Math.Max(value.x, 0);
            if (_overlappingPercent > 1 + Vector2.kEpsilon)
                value.x = Math.Min(value.x, 1);
            if (_clampUVValues)
                value.y = Math.Clamp(value.y, 0, 1);
            if (value.x > _maxX)
                _maxX = value.x + _xOffset;
            _uvPoints[index] = value;
        }

        public void AddPoint(Vector2 uvPoint, bool checkForMerge = true)
        {
            var x = float.IsNaN(uvPoint.x) ? 0 : uvPoint.x;
            var y = float.IsNaN(uvPoint.y) ? 0 : uvPoint.y;
            uvPoint = new Vector2(x, y);

            if (_overlappingPercent > 1 + Vector2.kEpsilon)
                uvPoint.x = Math.Clamp(uvPoint.x, 0, 1);

            if (_clampUVValues)
                uvPoint.y = Math.Clamp(uvPoint.y, 0, 1);
            if (uvPoint.x > _maxX)
                _maxX = uvPoint.x + _xOffset;

            if (checkForMerge && _uvPoints.Count > 2)
            {
                var last = _uvPoints[^1];
                var preLast = _uvPoints[^2];
                var dir1 = preLast - last;
                var dir2 = last - uvPoint;
                var smallLineChanges = dir1.magnitude < _pointsMergeThreshold && dir2.magnitude < _pointsMergeThreshold;
                var smallYChanges = _smallYChangeMerge && Math.Abs(preLast.y - last.y) < _pointsMergeThreshold && Math.Abs(last.y - uvPoint.y) < _pointsMergeThreshold;
                if (smallLineChanges || smallYChanges)
                    _uvPoints[^1] = uvPoint;
                else
                    _uvPoints.Add(uvPoint);
            }
            else
            {
                _uvPoints.Add(uvPoint);
            }
        }

        public void Rescale(Vector2 scale)
        {
            for (var i = 0; i < _uvPoints.Count; i++)
            {
                var point = _uvPoints[i];
                point *= scale;
                _uvPoints[i] = point;
            }
        }

        public int AddGraphShape(UIGraphShape graphShape)
        {
            _graphShapes.Add(graphShape);
            return _graphShapes.Count - 1;
        }

        public void RemoveGraphShape(int index)
        {
            _graphShapes[index] = UIGraphShape.NullShape;
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            IsDirty = false;
            vh.Clear();
            
            var points = _cacheUVGraphicPoints;
            points.Clear();
            PreprocessPoints(points, _uvPoints);

            var r = rectTransform.rect;
            _width = r.width;
            _height = r.height;

            var zeroPointOffset = rectTransform.pivot;
            zeroPointOffset.x *= r.width;
            zeroPointOffset.y *= r.height;

            var vertex = UIVertex.simpleVert;

            _origin = -zeroPointOffset;

            var pointsCount = points.Count;
            var startIndex = 0;
            if (pointsCount > 1)
            {
                if (_xStepFromCount)
                {
                    var step = 1f / (pointsCount - 1);
                    for (var i = 0; i < pointsCount; i++)
                    {
                        var p = points[i];
                        p.x = i * step;
                        points[i] = p;
                    }
                }

                var p1 = new Vector2();
                var alphaKeys = _underGraphGradient.alphaKeys;
                if (alphaKeys.Any(a => a.alpha > float.Epsilon))
                {
                    for (var i = 0; i < pointsCount; i++)
                    {
                        var uvPoint = points[i];
                        PutPoint(ref p1, uvPoint);
                        p1 += _origin;
                        var addPoint = p1;
                        addPoint.y = _origin.y;
                        if (_underGraphGradientNormalize)
                        {
                            var time = Math.Clamp(uvPoint.y, 0, 1);
                            vertex.color = _underGraphGradient.Evaluate(1 - time);
                        }
                        else
                        {
                            vertex.color = _underGraphGradient.Evaluate(0);
                        }
                        vertex.position = p1;
                        vh.AddVert(vertex);
                        vertex.color = _underGraphGradient.Evaluate(1);
                        vertex.position = addPoint;
                        vh.AddVert(vertex);
                        if (i < pointsCount - 1)
                            AddTrianglesNullSafety(vh, startIndex + i * 2, p1);
                    }

                    startIndex = pointsCount * 2;
                }

                if (color.a > Vector2.kEpsilon)
                {
                    vertex.color = GetColor(0);
                    var p2 = new Vector2();

                    // Zero edge
                    var uvP1 = points[0];
                    var uvP2 = points[1];
                    PutPoint(ref p1, uvP1);
                    PutPoint(ref p2, uvP2);
                    PutNormalNullSafety(out var normal, p1, p2);
                    var position = _origin + p1;
                    AddThickEdge(vh, ref vertex, position, normal, _thickness);
                    AddTrianglesNullSafety(vh, startIndex, uvP1, uvP2);

                    // TODO Beauty connections
                    for (var i = 1; i < pointsCount - 1; i++)
                    {
                        uvP1 = points[i];
                        uvP2 = points[i + 1];
                        PutPoint(ref p1, uvP1);
                        PutPoint(ref p2, uvP2);
                        position = _origin + p1;
                        vertex.color = GetColor((float) i / points.Count);
                        AddThickEdge(vh, ref vertex, position, normal, _thickness);

                        PutNormalNullSafety(out normal, uvP1, uvP2);
                        AddThickEdge(vh, ref vertex, position, normal, _thickness);

                        AddTrianglesNullSafety(vh, startIndex + i * 4, uvP1, uvP2);
                        AddTrianglesNullSafety(vh, startIndex + i * 4 - 2, uvP1, uvP2);
                    }

                    // End edge
                    uvP1 = points[^2];
                    uvP2 = points[^1];
                    PutPoint(ref p1, uvP1);
                    PutPoint(ref p2, uvP2);
                    PutNormalNullSafety(out normal, p1, p2);
                    position = _origin + p2;
                    vertex.color = GetColor(1);
                    AddThickEdge(vh, ref vertex, position, normal, _thickness);
                    startIndex += (pointsCount - 1) * 4;
                }
            }

            foreach (var graphShape in _graphShapes)
            {
                if (UIGraphShape.IsNull(graphShape) || Math.Abs(graphShape.Thickness) < Vector2.kEpsilon)
                    continue;

                DrawGraphShape(vh, graphShape, ref vertex, ref startIndex);
            }
        }

        private Color GetColor(float progress)
        {
            if (_gradient == null)
                return color;
            var alphaKeys = _gradient.alphaKeys;
            if (alphaKeys.All(a => a.alpha <= float.Epsilon))
                return color;

            return color * _gradient.Evaluate(progress);
        }
        
        private void DrawGraphShape(VertexHelper vh, in UIGraphShape graphShape, ref UIVertex vertex, ref int startIndex)
        {
            var points = graphShape.Points.ToList();
            switch (graphShape.ShapeType)
            {
                case ShapeType.Line:
                    DrawLineShape(vh, points, ref vertex, in graphShape, ref startIndex);
                    break;
                case ShapeType.FilledShape:
                    DrawFilledShape(vh, points, ref vertex, in graphShape, ref startIndex);
                    break;
                case ShapeType.StrokeShape:
                    DrawStrokeShape(vh, points, ref vertex, in graphShape, ref startIndex);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void DrawLineShape(VertexHelper vh, IList<Vector2> points, ref UIVertex vertex, in UIGraphShape graphShape, ref int startIndex)
        {
            var thickness = graphShape.Thickness;
            var pointsCount = points.Count;
            if (pointsCount <= 1 || thickness < Vector2.kEpsilon)
                return;
            
            if (_xStepFromCount)
            {
                var step = 1f / (pointsCount - 1);
                for (var i = 0; i < pointsCount; i++)
                {
                    var p = points[i];
                    p.x = i * step;
                    points[i] = p;
                }
            }

            var p1 = new Vector2();

            var c = graphShape.EvaluateColor(0);
            // if (_underGraphFillColor.a > Vector2.kEpsilon)
            // {
            //     var uc = _underGraphFillColor;
            //     uc.a = c.a;
            //     vertex.color = uc;
            //     for (var i = 0; i < pointsCount; i++)
            //     {
            //         PutPoint(ref p1, points[i]);
            //         p1 += _origin;
            //         var addPoint = p1;
            //         addPoint.y = _origin.y;
            //         AddEdge(vh, ref vertex, p1, addPoint);
            //         if (i < pointsCount - 1)
            //             AddTrianglesNullSafety(vh, startIndex + i * 2, p1);
            //     }
            //
            //     startIndex += pointsCount * 2;
            // }

            if (c.a > Vector2.kEpsilon)
            {
                vertex.color = c;
                var p2 = new Vector2();

                // Zero edge
                var uvP1 = points[0];
                var uvP2 = points[1];
                PutPoint(ref p1, uvP1);
                PutPoint(ref p2, uvP2);
                PutNormalNullSafety(out var normal, p1, p2);
                var position = _origin + p1;
                AddThickEdge(vh, ref vertex, position, normal, thickness);
                AddTrianglesNullSafety(vh, startIndex, uvP1, uvP2);

                // TODO Beauty connections
                for (var i = 1; i < pointsCount - 1; i++)
                {
                    uvP1 = points[i];
                    uvP2 = points[i + 1];
                    PutPoint(ref p1, uvP1);
                    PutPoint(ref p2, uvP2);
                    position = _origin + p1;
                    vertex.color = graphShape.EvaluateColor((float) i / points.Count);
                    
                    AddThickEdge(vh, ref vertex, position, normal, thickness);
                    PutNormalNullSafety(out normal, uvP1, uvP2);
                    AddThickEdge(vh, ref vertex, position, normal, thickness);

                    AddTrianglesNullSafety(vh, startIndex + i * 4, uvP1, uvP2);
                    AddTrianglesNullSafety(vh, startIndex + i * 4 - 2, uvP1, uvP2);
                }

                // End edge
                uvP1 = points[^2];
                uvP2 = points[^1];
                PutPoint(ref p1, uvP1);
                PutPoint(ref p2, uvP2);
                PutNormalNullSafety(out normal, p1, p2);
                position = _origin + p2;
                vertex.color = graphShape.EvaluateColor(1);
                AddThickEdge(vh, ref vertex, position, normal, thickness);
                startIndex += (pointsCount - 1) * 4;
            }
        }
        
        private void DrawStrokeShape(VertexHelper vh, IList<Vector2> points, ref UIVertex vertex, in UIGraphShape graphShape, ref int startIndex)
        {
            var thickness = graphShape.Thickness;
            var pointsCount = points.Count;
            if (pointsCount < 3 || thickness < Vector2.kEpsilon)
                return;

            vertex.color = graphShape.EvaluateColor(0);
            var p1 = new Vector2();
            var p2 = new Vector2();

            // Zero edge
            var uvP1 = points[0];
            var uvP2 = points[1];
            PutPoint(ref p1, uvP1);
            PutPoint(ref p2, uvP2);
            var normal = Vector2.up;
            // PutNormalNullSafety(out var normal, p1, p2);
            var position = _origin + p1;
            AddThickEdge(vh, ref vertex, position, normal, thickness);
            AddTrianglesNullSafety(vh, startIndex, uvP1, uvP2);

            for (var i = 1; i < pointsCount - 1; i++)
            {
                uvP1 = points[i];
                uvP2 = points[i + 1];
                PutPoint(ref p1, uvP1);
                PutPoint(ref p2, uvP2);
                position = _origin + p1;
                AddThickEdge(vh, ref vertex, position, normal, thickness);

                PutNormalNullSafety(out normal, uvP1, uvP2);
                AddThickEdge(vh, ref vertex, position, normal, thickness);

                AddTrianglesNullSafety(vh, startIndex + i * 4, uvP1, uvP2);
                AddTrianglesNullSafety(vh, startIndex + i * 4 - 2, uvP1, uvP2);
            }

            uvP1 = points[^2];
            uvP2 = points[^1];
            PutPoint(ref p1, uvP1);
            PutPoint(ref p2, uvP2);
            // PutNormalNullSafety(out normal, p1, p2);
            normal = Vector2.down;
            position = _origin + p2;
            AddThickEdge(vh, ref vertex, position, normal, thickness);
            startIndex += (pointsCount - 1) * 4;
        }

        private void DrawFilledShape(VertexHelper vh, IList<Vector2> points, ref UIVertex vertex, in UIGraphShape c, ref int startIndex)
        {
            var pointsCount = points.Count;
            if (pointsCount < 3)
                return;

            vertex.color = GetColor(0);
            var p1 = new Vector2();
            var p2 = new Vector2();

            // Zero edge
            var uvP1 = points[0];
            var uvP2 = points[1];
            PutPoint(ref p1, uvP1);
            PutPoint(ref p2, uvP2);
            PutNormalNullSafety(out var normal, uvP1, uvP2);
            var position = _origin + p1;
            // AddThickEdge(vh, ref vertex, position, normal, thickness);
            AddTrianglesNullSafety(vh, startIndex, uvP1, uvP2);

            // TODO Beauty connections
            for (var i = 1; i < pointsCount - 1; i++)
            {
                uvP1 = points[i];
                uvP2 = points[i + 1];
                PutPoint(ref p1, uvP1);
                PutPoint(ref p2, uvP2);
                position = _origin + p1;

                PutNormalNullSafety(out normal, uvP1, uvP2);

                AddTrianglesNullSafety(vh, startIndex + i * 4, uvP1, uvP2);
                AddTrianglesNullSafety(vh, startIndex + i * 4 - 2, uvP1, uvP2);
            }

            // End edge
            uvP1 = points[^2];
            uvP2 = points[^1];
            PutNormalNullSafety(out normal, uvP1, uvP2);

            PutPoint(ref p1, points[^1]);
            position = _origin + p1;
            // AddThickEdge(vh, ref vertex, position, normal, _thickness);
            startIndex += (pointsCount - 1) * 4;
        }

        private void PreprocessPoints(List<Vector2> points, IReadOnlyList<Vector2> originPoints)
        {
            _smoothCurve.ClearKeys();
            var overlapValue = _maxX - _overlappingPercent;
            if (_overlappingPercent > 1 + Vector2.kEpsilon)
            {
                var prevOffset = _xOffset;
                _xOffset = 0;
                for (var i = 0; i < originPoints.Count; i++)
                {
                    var uvPoint = originPoints[i];
                    if (!_isSmooth || i - 1 < 0)
                        AddPointToCollection(points, uvPoint);
                    else
                        AddPointToCollection(points, uvPoint, originPoints[i - 1].x);
                }

                _xOffset = prevOffset;
                return;
            }

            if (overlapValue < 0)
            {
                FillPointWithOffsets(points);
                return;
            }

            if (!_overlapJump)
            {
                points.AddRange(originPoints);
                return;
            }

            var offset = overlapValue;
            var prevPoint = Vector2.zero;
            var startOffset = _indexOffset;
            for (var i = _indexOffset; i < originPoints.Count; i++)
            {
                var uvPoint = originPoints[i];
                if (IsNullPoint(uvPoint))
                {
                    points.Add(uvPoint);
                    continue;
                }

                uvPoint.x += _xOffset;
                var newZeroXInOldSpace = uvPoint.x - offset;
                if (newZeroXInOldSpace < 0)
                {
                    IncrementIndexOffsetWithClear();
                    prevPoint = uvPoint;
                    continue;
                }

                if (uvPoint.x < Vector2.kEpsilon)
                    break;

                if (i == startOffset)
                {
                    if (i == 0)
                        break;
                    prevPoint = originPoints[i - 1];
                    prevPoint.x += _xOffset;
                }

                // prevPoint.x bellow zero
                var percent = -prevPoint.x / (uvPoint.x - prevPoint.x);
                var newZeroYInNewSpace = Mathf.Lerp(prevPoint.y, uvPoint.y, percent);
                points.Add(new Vector2(0, newZeroYInNewSpace));
                break;
            }

            _xOffset -= offset;
            _maxX -= offset;
            FillPointWithOffsets(points);
        }

        private void IncrementIndexOffsetWithClear()
        {
            if (_indexOffsetForRemove < 0)
            {
                _indexOffset++;
                return;
            }

            if (_indexOffsetForRemove == 0)
            {
                _uvPoints.RemoveAt(0);
                return;
            }

            _indexOffset++;
            if (_indexOffset >= _indexOffsetForRemove)
            {
                _indexOffset = 0;
                _uvPoints.RemoveRange(0, _indexOffsetForRemove);
            }
        }

        private void FillPointWithOffsets(in ICollection<Vector2> points)
        {
            for (var i = _indexOffset; i < _uvPoints.Count; i++)
            {
                var uvPoint = _uvPoints[i];
                if (!IsNullPoint(uvPoint))
                    uvPoint.x += _xOffset;
                if (!_isSmooth || i - 1 < 0)
                    AddPointToCollection(points, uvPoint);
                else
                    AddPointToCollection(points, uvPoint, _uvPoints[i - 1].x + _xOffset);
            }
        }

        private void AddPointToCollection(in ICollection<Vector2> points, in Vector2 uvPoint)
        {
            points.Add(uvPoint);
            if (_isSmooth)
            {
                AddVectorToSmoothCurve(uvPoint);
            }
        }

        private void AddPointToCollection(in ICollection<Vector2> points, in Vector2 uvPoint, in float prevX)
        {
            var y = _smoothCurve.Evaluate(prevX);
            if (Math.Abs(uvPoint.y - y) < Vector2.kEpsilon)
            {
                AddPointToCollection(points, uvPoint);
                return;
            }
            
            AddVectorToSmoothCurve(uvPoint);
            for (var x = prevX + XStep; x < uvPoint.x - XStep; x += XStep)
            {
                y = _smoothCurve.Evaluate(x);
                points.Add(new Vector2(x, y));
            }
            points.Add(uvPoint);
        }

        private void AddVectorToSmoothCurve(in Vector2 uvPoint)
        {
            var key = new Keyframe(uvPoint.x, uvPoint.y)
            {
                tangentMode = _smoothMode
            };
            _smoothCurve.AddKey(key);
        }

        private void AddThickEdge(VertexHelper vh, ref UIVertex vertex, in Vector2 position, in Vector2 normal, in float thickness)
        {
            var thicknessVector = normal * thickness;
            var bottom = position - thicknessVector * 0.5f;
            vertex.position = bottom;
            vh.AddVert(vertex);
            var top = position + thicknessVector * 0.5f;
            vertex.position = top;
            vh.AddVert(vertex);
        }

        private void AddTrianglesNullSafety(VertexHelper vh, in int i, in Vector2 p1, in Vector2 p2)
        {
            if (IsNullPoint(p1) || IsNullPoint(p2))
                return;
            AddTriangles(vh, i);
        }

        private void AddTrianglesNullSafety(VertexHelper vh, in int i, in Vector2 p1)
        {
            if (IsNullPoint(p1))
                return;
            AddTriangles(vh, i);
        }

        private void AddTriangles(VertexHelper vh, in int i)
        {
            vh.AddTriangle(i, i + 1, i + 2);
            vh.AddTriangle(i + 1, i + 2, i + 3);
        }

        private void PutPoint(ref Vector2 pos, in Vector2 uvPoint)
        {
            pos.x = _width * uvPoint.x;
            pos.y = _height * uvPoint.y;
        }

        private void PutNormalNullSafety(out Vector2 n, in Vector2 p1, in Vector2 p2)
        {
            if (IsNullPoint(p1) || IsNullPoint(p2))
                n = Vector2.up;
            else
                n = Vector2.Perpendicular(p2 - p1).normalized;
        }

        private void PutNormal(out Vector2 n, in Vector2 p1, in Vector2 p2, in Vector2 p3)
        {
            PutNormalNullSafety(out _cachedNormal1, p1, p2);
            PutNormalNullSafety(out _cachedNormal2, p2, p3);
            n = ((_cachedNormal1 + _cachedNormal2) * 0.5f).normalized;
        }

        public void ClearOver(float x, float? newY = null)
        {
            _maxX = x;
            for (var i = _uvPoints.Count - 1; i >= 0; i--)
            {
                var point = _uvPoints[i];
                if (point.x < x)
                {
                    _uvPoints[i] = new Vector2(x, newY ?? _uvPoints[Math.Max(i - 1, 0)].y);
                    break;
                }
                if (i > 0)
                {
                    var prevPoint = _uvPoints[i - 1];
                    if (prevPoint.x <= x && point.x > x)
                    {
                        _uvPoints[i - 1] = new Vector2(x, newY ?? prevPoint.y);
                        _uvPoints.RemoveAt(i);
                    }
                }
                else if (point.x > x)
                {
                    _uvPoints.RemoveAt(i);
                }
            }
        }

        public void Clear()
        {
            _xOffset = 0;
            _maxX = float.MinValue;
            _indexOffset = 0;
            _uvPoints?.Clear();
            _graphShapes?.Clear();
            _smoothCurve.ClearKeys();
            SetDirty();
        }

        [Button, ShowIf(nameof(_debugElements))]
        public void SetDirty()
        {
            IsDirty = true;
            SetVerticesDirty();
            SetMaterialDirty();
        }
        
        [Button, ShowIf(nameof(_debugElements))]
        private void PutNullPoint()
        {
            _uvPoints.Add(NullPoint);
        }
    }
}