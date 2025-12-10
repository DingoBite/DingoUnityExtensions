using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace DingoUnityExtensions.MonoBehaviours.UI.UIGraph
{
    public class UIGraphic : MaskableGraphic
    {
        private static readonly float2 NullPoint = new float2(-1e6f, -1e6f);
        private static bool IsNullPoint(float2 p) => p.x == NullPoint.x && p.y == NullPoint.y;

        private static readonly int StencilComp = Shader.PropertyToID("_StencilComp");

        [Header("Source")] [SerializeField] private List<Vector2> _uvPoints = new();

        [Header("Line")] [SerializeField] private float _thickness = 2f;
        [SerializeField] private bool _xStepFromCount;
        [SerializeField] private bool _clampUVValues = true;

        [Header("Overlap")] [SerializeField] private float _overlappingPercent = 1;
        [SerializeField] private bool _overlapJump;
        [SerializeField] private int _indexOffsetForRemove = 128;

        [Header("Merge")] [SerializeField] private double _pointsMergeThreshold = 1e-3;
        [SerializeField] private bool _smallYChangeMerge;

        [Header("Smooth")] [SerializeField] private bool _isSmooth;
        [SerializeField, Min(0.001f)] private float _xStep = 0.1f;
        [SerializeField] private int _smoothMode = 4;

        [Header("Colors")] [SerializeField] private Gradient _gradient;
        [SerializeField] private Gradient _underGraphGradient;
        [SerializeField] private bool _underGraphGradientNormalize;

        [Header("Mask")] [SerializeField] private CompareFunction _parentMaskCompareFunction = CompareFunction.Equal;
        [SerializeField] private Material _invertedMaskMaterial;

        private readonly List<UIGraphShape> _graphShapes = new(64);
        
        private NativeList<float2> _pointsNative;
        private NativeList<float2> _preprocessed;
        private NativeList<GraphVertex> _lineVerts;
        private NativeList<int> _lineIndices;
        private NativeList<GraphVertex> _underVerts;
        private NativeList<int> _underIndices;

        private NativeArray<uint> _gradientLut;
        private NativeArray<uint> _underLut;

        private float _width, _height;
        private float _xOffset;
        private float _maxX = float.MinValue;
        private int _indexOffset;

        private const int LutSize = 256;

        public override Material materialForRendering
        {
            get
            {
                try
                {
                    _invertedMaskMaterial.SetInt(StencilComp, (int)_parentMaskCompareFunction);
                }
                catch
                {
                    _invertedMaskMaterial = new Material(base.materialForRendering);
                    _invertedMaskMaterial.SetInt(StencilComp, (int)_parentMaskCompareFunction);
                }

                return _invertedMaskMaterial;
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            EnsureNative();
            RebuildLuts();
            SyncManagedToNative();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            DisposeNative();
        }

        private void EnsureNative()
        {
            if (!_pointsNative.IsCreated)
                _pointsNative = new NativeList<float2>(Allocator.Persistent);
            if (!_preprocessed.IsCreated)
                _preprocessed = new NativeList<float2>(Allocator.Persistent);

            if (!_lineVerts.IsCreated)
                _lineVerts = new NativeList<GraphVertex>(Allocator.Persistent);
            if (!_lineIndices.IsCreated)
                _lineIndices = new NativeList<int>(Allocator.Persistent);

            if (!_underVerts.IsCreated)
                _underVerts = new NativeList<GraphVertex>(Allocator.Persistent);
            if (!_underIndices.IsCreated)
                _underIndices = new NativeList<int>(Allocator.Persistent);

            if (!_gradientLut.IsCreated)
                _gradientLut = new NativeArray<uint>(LutSize, Allocator.Persistent);
            if (!_underLut.IsCreated)
                _underLut = new NativeArray<uint>(LutSize, Allocator.Persistent);
        }

        private void DisposeNative()
        {
            if (_pointsNative.IsCreated)
                _pointsNative.Dispose();
            if (_preprocessed.IsCreated)
                _preprocessed.Dispose();

            if (_lineVerts.IsCreated)
                _lineVerts.Dispose();
            if (_lineIndices.IsCreated)
                _lineIndices.Dispose();

            if (_underVerts.IsCreated)
                _underVerts.Dispose();
            if (_underIndices.IsCreated)
                _underIndices.Dispose();

            if (_gradientLut.IsCreated)
                _gradientLut.Dispose();
            if (_underLut.IsCreated)
                _underLut.Dispose();
        }

        public void SetPoints(IEnumerable<Vector2> points)
        {
            _uvPoints.Clear();
            foreach (var p in points)
                AddPoint(p);

            SyncManagedToNative();
            SetVerticesDirty();
        }

        public void AddPoint(Vector2 uvPoint, bool checkForMerge = true)
        {
            var x = float.IsNaN(uvPoint.x) ? 0 : uvPoint.x;
            var y = float.IsNaN(uvPoint.y) ? 0 : uvPoint.y;
            var p = new Vector2(x, y);

            if (_overlappingPercent > 1 + Vector2.kEpsilon)
                p.x = Mathf.Clamp01(p.x);

            if (_clampUVValues)
                p.y = Mathf.Clamp01(p.y);

            if (p.x > _maxX)
                _maxX = p.x + _xOffset;

            if (checkForMerge && _uvPoints.Count > 2)
            {
                var last = _uvPoints[^1];
                var preLast = _uvPoints[^2];
                var dir1 = preLast - last;
                var dir2 = last - p;
                var smallLineChanges = dir1.magnitude < _pointsMergeThreshold && dir2.magnitude < _pointsMergeThreshold;
                var smallYChanges = _smallYChangeMerge && Math.Abs(preLast.y - last.y) < _pointsMergeThreshold && Math.Abs(last.y - p.y) < _pointsMergeThreshold;

                if (smallLineChanges || smallYChanges)
                    _uvPoints[^1] = p;
                else
                    _uvPoints.Add(p);
            }
            else
            {
                _uvPoints.Add(p);
            }
        }
        
        public int AddShape(UIGraphShape shape)
        {
            _graphShapes.Add(shape);
            SetVerticesDirty();
            return _graphShapes.Count - 1;
        }

        // Для совместимости со старым API
        public int AddGraphShape(UIGraphShape shape) => AddShape(shape);

        public void RemoveShape(int index)
        {
            if ((uint)index >= (uint)_graphShapes.Count)
                return;

            // Поведение как в старом классе без сдвига индексов
            _graphShapes[index] = UIGraphShape.NullShape;
            SetVerticesDirty();
        }

        public void Clear()
        {
            _xOffset = 0;
            _maxX = float.MinValue;
            _indexOffset = 0;

            _uvPoints.Clear();
            _graphShapes.Clear();

            if (_pointsNative.IsCreated) _pointsNative.Clear();
            if (_preprocessed.IsCreated) _preprocessed.Clear();

            if (_lineVerts.IsCreated) _lineVerts.Clear();
            if (_lineIndices.IsCreated) _lineIndices.Clear();

            if (_underVerts.IsCreated) _underVerts.Clear();
            if (_underIndices.IsCreated) _underIndices.Clear();

            SetVerticesDirty();
            SetMaterialDirty();
        }

        private void SyncManagedToNative()
        {
            EnsureNative();
            _pointsNative.Clear();
            _pointsNative.Capacity = math.max(_pointsNative.Capacity, _uvPoints.Count);

            for (var i = 0; i < _uvPoints.Count; i++)
            {
                var p = _uvPoints[i];
                _pointsNative.Add(new float2(p.x, p.y));
            }
        }

        private void RebuildLuts()
        {
            EnsureNative();
            BuildGradientLut(_gradient, _gradientLut, color);
            BuildGradientLut(_underGraphGradient, _underLut, Color.white);
        }

        private static void BuildGradientLut(Gradient g, NativeArray<uint> dst, Color multiply)
        {
            if (g == null)
            {
                for (var i = 0; i < dst.Length; i++)
                    dst[i] = Pack(multiply);
                return;
            }

            for (var i = 0; i < dst.Length; i++)
            {
                var t = i / (dst.Length - 1f);
                var c = g.Evaluate(t) * multiply;
                dst[i] = Pack(c);
            }
        }

        private static uint Pack(Color c)
        {
            var c32 = (Color32)c;
            return (uint)(c32.r | (c32.g << 8) | (c32.b << 16) | (c32.a << 24));
        }

        private static Color32 Unpack(uint u)
        {
            return new Color32((byte)(u & 0xFF), (byte)((u >> 8) & 0xFF), (byte)((u >> 16) & 0xFF), (byte)((u >> 24) & 0xFF));
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            EnsureNative();

            var r = rectTransform.rect;
            _width = r.width;
            _height = r.height;

            var pivot = rectTransform.pivot;
            var origin = new float2(-pivot.x * _width, -pivot.y * _height);

            _preprocessed.Clear();

            var preprocessJob = new PreprocessJob
            {
                Input = _pointsNative.AsArray(),
                Output = _preprocessed,
                XStepFromCount = _xStepFromCount,
                IsSmooth = _isSmooth,
                XStep = math.max(_xStep, 0.001f),
                ClampUV = _clampUVValues,
                OverlappingPercent = _overlappingPercent,
                OverlapJump = _overlapJump,
                XOffset = _xOffset,
                MaxX = _maxX,
                IndexOffset = _indexOffset,
                IndexOffsetForRemove = _indexOffsetForRemove,

                PointsMergeThreshold = (float)_pointsMergeThreshold,
                SmallYChangeMerge = _smallYChangeMerge
            };

            preprocessJob.Run();

            _lineVerts.Clear();
            _lineIndices.Clear();

            var lineJob = new BuildPolylineJob
            {
                Points = _preprocessed.AsArray(),
                OutVerts = _lineVerts,
                OutIndices = _lineIndices,
                Origin = origin,
                Width = _width,
                Height = _height,
                Thickness = _thickness,
                GradientLut = _gradientLut
            };

            lineJob.Run();

            _underVerts.Clear();
            _underIndices.Clear();

            bool drawUnder = false;
            if (_underGraphGradient != null)
            {
                var aks = _underGraphGradient.alphaKeys;
                for (int i = 0; i < aks.Length; i++)
                {
                    if (aks[i].alpha > 0.0001f) { drawUnder = true; break; }
                }
            }

            _underVerts.Clear();
            _underIndices.Clear();

            if (drawUnder)
            {
                var underJob = new BuildUnderfillJob
                {
                    Points = _preprocessed.AsArray(),
                    OutVerts = _underVerts,
                    OutIndices = _underIndices,
                    Origin = origin,
                    Width = _width,
                    Height = _height,
                    UnderLut = _underLut,
                    Normalize = _underGraphGradientNormalize
                };

                underJob.Run();
            }

            vh.Clear();

            var baseIndex = 0;
            AddToVH(vh, _underVerts, _underIndices, ref baseIndex);
            AddToVH(vh, _lineVerts, _lineIndices, ref baseIndex);
            var originV2 = new Vector2(origin.x, origin.y);
            DrawShapesManaged(vh, ref baseIndex, originV2);
        }

        private static void AddToVH(VertexHelper vh, NativeList<GraphVertex> verts, NativeList<int> indices, ref int baseIndex)
        {
            for (var i = 0; i < verts.Length; i++)
            {
                var v = verts[i];
                var ui = UIVertex.simpleVert;
                ui.position = new Vector3(v.Pos.x, v.Pos.y, 0);
                ui.color = Unpack(v.Color);
                vh.AddVert(ui);
            }

            for (var i = 0; i < indices.Length; i += 3)
            {
                vh.AddTriangle(baseIndex + indices[i], baseIndex + indices[i + 1], baseIndex + indices[i + 2]);
            }

            baseIndex += verts.Length;
        }

        private void DrawShapesManaged(VertexHelper vh, ref int startIndex, Vector2 origin)
        {
            if (_graphShapes.Count == 0)
                return;

            var vertex = UIVertex.simpleVert;

            for (int s = 0; s < _graphShapes.Count; s++)
            {
                var graphShape = _graphShapes[s];

                if (UIGraphShape.IsNull(graphShape))
                    continue;

                if (graphShape.ShapeType == ShapeType.Line && Mathf.Abs(graphShape.Thickness) < Vector2.kEpsilon)
                    continue;

                DrawGraphShapeManaged(vh, graphShape, ref vertex, ref startIndex, origin);
            }
        }

        private void DrawGraphShapeManaged(VertexHelper vh, in UIGraphShape graphShape, ref UIVertex vertex, ref int startIndex, Vector2 origin)
        {
            // Избегаем ToList каждый кадр если Points уже список.
            // Если Points это IEnumerable, то да, будет аллокация.
            var points = graphShape.Points as IList<Vector2> ?? new List<Vector2>(graphShape.Points);

            switch (graphShape.ShapeType)
            {
                case ShapeType.Line:
                    DrawLineShapeManaged(vh, points, ref vertex, graphShape, ref startIndex, origin);
                    break;
                case ShapeType.FilledShape:
                    DrawFilledShapeManaged(vh, points, ref vertex, graphShape, ref startIndex, origin);
                    break;
                case ShapeType.StrokeShape:
                    DrawStrokeShapeManaged(vh, points, ref vertex, graphShape, ref startIndex, origin);
                    break;
            }
        }

        private void DrawLineShapeManaged(VertexHelper vh, IList<Vector2> points, ref UIVertex vertex, in UIGraphShape graphShape,
            ref int startIndex, Vector2 origin)
        {
            var thickness = graphShape.Thickness;
            var pointsCount = points.Count;
            if (pointsCount <= 1 || thickness < Vector2.kEpsilon)
                return;

            if (_xStepFromCount && pointsCount > 1)
            {
                var step = 1f / (pointsCount - 1);
                for (var i = 0; i < pointsCount; i++)
                {
                    var p = points[i];
                    p.x = i * step;
                    points[i] = p;
                }
            }

            var uvP1 = points[0];
            var uvP2 = points[1];

            var p1 = ToLocal(uvP1, origin);
            var p2 = ToLocal(uvP2, origin);

            var normal = GetNormal(p1, p2);

            var c = graphShape.EvaluateColor(0);
            if (c.a <= Vector2.kEpsilon)
                return;

            vertex.color = c;

            AddThickEdgeManaged(vh, ref vertex, p1, normal, thickness);
            AddTrianglesNullSafetyManaged(vh, startIndex, uvP1, uvP2);

            for (var i = 1; i < pointsCount - 1; i++)
            {
                uvP1 = points[i];
                uvP2 = points[i + 1];

                p1 = ToLocal(uvP1, origin);
                p2 = ToLocal(uvP2, origin);

                vertex.color = graphShape.EvaluateColor((float)i / pointsCount);

                AddThickEdgeManaged(vh, ref vertex, p1, normal, thickness);

                normal = GetNormal(p1, p2);
                AddThickEdgeManaged(vh, ref vertex, p1, normal, thickness);

                AddTrianglesNullSafetyManaged(vh, startIndex + i * 4, uvP1, uvP2);
                AddTrianglesNullSafetyManaged(vh, startIndex + i * 4 - 2, uvP1, uvP2);
            }

            uvP1 = points[^2];
            uvP2 = points[^1];

            p1 = ToLocal(uvP1, origin);
            p2 = ToLocal(uvP2, origin);

            normal = GetNormal(p1, p2);

            vertex.color = graphShape.EvaluateColor(1);
            AddThickEdgeManaged(vh, ref vertex, p2, normal, thickness);

            startIndex += (pointsCount - 1) * 4;
        }

        private void DrawStrokeShapeManaged(VertexHelper vh, IList<Vector2> points, ref UIVertex vertex, in UIGraphShape graphShape,
            ref int startIndex, Vector2 origin)
        {
            var thickness = graphShape.Thickness;
            var pointsCount = points.Count;
            if (pointsCount < 3 || thickness < Vector2.kEpsilon)
                return;

            vertex.color = graphShape.EvaluateColor(0);

            var uvP1 = points[0];
            var uvP2 = points[1];

            var p1 = ToLocal(uvP1, origin);
            var p2 = ToLocal(uvP2, origin);

            var normal = Vector2.up;

            AddThickEdgeManaged(vh, ref vertex, p1, normal, thickness);
            AddTrianglesNullSafetyManaged(vh, startIndex, uvP1, uvP2);

            for (var i = 1; i < pointsCount - 1; i++)
            {
                uvP1 = points[i];
                uvP2 = points[i + 1];

                p1 = ToLocal(uvP1, origin);
                p2 = ToLocal(uvP2, origin);

                AddThickEdgeManaged(vh, ref vertex, p1, normal, thickness);

                normal = GetNormal(p1, p2);
                AddThickEdgeManaged(vh, ref vertex, p1, normal, thickness);

                AddTrianglesNullSafetyManaged(vh, startIndex + i * 4, uvP1, uvP2);
                AddTrianglesNullSafetyManaged(vh, startIndex + i * 4 - 2, uvP1, uvP2);
            }

            uvP1 = points[^2];
            uvP2 = points[^1];

            p1 = ToLocal(uvP1, origin);
            p2 = ToLocal(uvP2, origin);

            normal = Vector2.down;

            AddThickEdgeManaged(vh, ref vertex, p2, normal, thickness);

            startIndex += (pointsCount - 1) * 4;
        }

        private void DrawFilledShapeManaged(VertexHelper vh, IList<Vector2> points, ref UIVertex vertex, in UIGraphShape graphShape,
            ref int startIndex, Vector2 origin)
        {
            var pointsCount = points.Count;
            if (pointsCount < 3)
                return;

            vertex.color = graphShape.EvaluateColor(0);

            var localVerts = new List<Vector2>(pointsCount);

            for (var i = 0; i < pointsCount; i++)
            {
                var uv = points[i];
                var p = ToLocal(uv, origin);
                localVerts.Add(p);
            }

            for (var i = 0; i < pointsCount; i++)
            {
                vertex.position = localVerts[i];
                vh.AddVert(vertex);
            }

            for (var i = 1; i < pointsCount - 1; i++)
            {
                vh.AddTriangle(startIndex, startIndex + i, startIndex + i + 1);
            }

            startIndex += pointsCount;
        }

        private Vector2 ToLocal(in Vector2 uv, in Vector2 origin)
        {
            return new Vector2(_width * uv.x, _height * uv.y) + origin;
        }

        private static Vector2 GetNormal(in Vector2 p1, in Vector2 p2)
        {
            var d = p2 - p1;
            if (d.sqrMagnitude < 1e-12f)
                return Vector2.up;
            return Vector2.Perpendicular(d).normalized;
        }

        private static void AddThickEdgeManaged(VertexHelper vh, ref UIVertex vertex, in Vector2 position, in Vector2 normal, float thickness)
        {
            var t = normal * thickness * 0.5f;

            vertex.position = position - t;
            vh.AddVert(vertex);

            vertex.position = position + t;
            vh.AddVert(vertex);
        }

        private static void AddTrianglesManaged(VertexHelper vh, int i)
        {
            vh.AddTriangle(i, i + 1, i + 2);
            vh.AddTriangle(i + 1, i + 2, i + 3);
        }

        private static void AddTrianglesNullSafetyManaged(VertexHelper vh, int i, in Vector2 p1, in Vector2 p2)
        {
            if (IsNullPoint(p1) || IsNullPoint(p2))
                return;
            AddTrianglesManaged(vh, i);
        }
        
#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            _thickness = Mathf.Max(0, _thickness);
            _xStep = Mathf.Max(0.001f, _xStep);
            _overlappingPercent = Mathf.Max(0, _overlappingPercent);

            // Достаточно для корректного превью в редакторе.
            EnsureNative();
            RebuildLuts();
            SyncManagedToNative();

            SetVerticesDirty();
            SetMaterialDirty();
        }
#endif
        
        [BurstCompile]
        private struct PreprocessJob : IJob
        {
            [ReadOnly] public NativeArray<float2> Input;
            public NativeList<float2> Output;

            public bool XStepFromCount;
            public bool IsSmooth;
            public float XStep;

            public bool ClampUV;
            public float OverlappingPercent;
            public bool OverlapJump;

            public float XOffset;
            public float MaxX;
            public int IndexOffset;
            public int IndexOffsetForRemove;

            public float PointsMergeThreshold;
            public bool SmallYChangeMerge;

            public void Execute()
            {
                Output.Clear();

                var count = Input.Length;
                if (count == 0)
                    return;

                if (XStepFromCount && count > 1)
                {
                    var step = 1f / (count - 1);
                    for (var i = 0; i < count; i++)
                    {
                        var p = Input[i];
                        p.x = i * step;
                        AddPoint(p);
                    }
                }
                else
                {
                    for (var i = IndexOffset; i < count; i++)
                    {
                        var p = Input[i];
                        if (!IsNullPoint(p))
                            p.x += XOffset;
                        AddPoint(p);
                    }
                }
            }

            private void AddPoint(float2 p)
            {
                if (OverlappingPercent > 1f + 1e-6f)
                    p.x = math.clamp(p.x, 0f, 1f);

                if (ClampUV)
                    p.y = math.clamp(p.y, 0f, 1f);

                Output.Add(p);
            }
        }

        private struct GraphVertex
        {
            public float2 Pos;
            public uint Color;
        }

        [BurstCompile]
        private struct BuildPolylineJob : IJob
        {
            [ReadOnly] public NativeArray<float2> Points;
            [ReadOnly] public NativeArray<uint> GradientLut;

            public NativeList<GraphVertex> OutVerts;
            public NativeList<int> OutIndices;

            public float2 Origin;
            public float Width;
            public float Height;
            public float Thickness;

            public void Execute()
            {
                var pc = Points.Length;
                if (pc < 2 || Thickness <= 1e-6f)
                    return;

                var expectedVerts = 4 * pc - 4;
                OutVerts.Capacity = math.max(OutVerts.Capacity, expectedVerts);

                var expectedIndices = 6 * ((pc - 1) + (pc - 2));
                OutIndices.Capacity = math.max(OutIndices.Capacity, expectedIndices);

                var uv0 = Points[0];
                var uv1 = Points[1];

                var nPrev = NormalSafe(uv0, uv1);

                AddThick(Local(uv0), nPrev, ColorAt(0, pc));

                if (!IsNullPoint(uv0) && !IsNullPoint(uv1))
                    AddQuad(0);

                for (var i = 1; i < pc - 1; i++)
                {
                    var uvi = Points[i];
                    var uvNext = Points[i + 1];

                    var pos = Local(uvi);
                    var col = ColorAt(i, pc);

                    AddThick(pos, nPrev, col);

                    var nNext = NormalSafe(uvi, uvNext);

                    AddThick(pos, nNext, col);

                    var baseI = i * 4;

                    if (!IsNullPoint(uvi) && !IsNullPoint(uvNext))
                    {
                        AddQuad(baseI);
                        AddQuad(baseI - 2);
                    }

                    nPrev = nNext;
                }

                var uvLast = Points[pc - 1];
                AddThick(Local(uvLast), nPrev, ColorAt(pc - 1, pc));
            }

            private uint ColorAt(int i, int pc)
            {
                float t = i <= 0 ? 0f : (i >= pc - 1 ? 1f : (float)i / pc);
                return SampleLut(GradientLut, t);
            }

            private float2 NormalSafe(float2 aUv, float2 bUv)
            {
                if (IsNullPoint(aUv) || IsNullPoint(bUv))
                    return new float2(0, 1);

                var a = Local(aUv);
                var b = Local(bUv);
                var d = b - a;
                return math.normalizesafe(new float2(-d.y, d.x), new float2(0, 1));
            }

            private float2 Local(float2 uv) => new float2(Width * uv.x, Height * uv.y) + Origin;

            private void AddThick(float2 pos, float2 n, uint col)
            {
                var tvec = n * (Thickness * 0.5f);
                OutVerts.Add(new GraphVertex { Pos = pos - tvec, Color = col });
                OutVerts.Add(new GraphVertex { Pos = pos + tvec, Color = col });
            }

            private void AddQuad(int i)
            {
                OutIndices.Add(i);
                OutIndices.Add(i + 1);
                OutIndices.Add(i + 2);
                OutIndices.Add(i + 1);
                OutIndices.Add(i + 2);
                OutIndices.Add(i + 3);
            }

            private static uint SampleLut(NativeArray<uint> lut, float t)
            {
                var i = (int)math.round(math.clamp(t, 0f, 1f) * (lut.Length - 1));
                return lut[i];
            }
        }

        [BurstCompile]
        private struct BuildUnderfillJob : IJob
        {
            [ReadOnly] public NativeArray<float2> Points;
            [ReadOnly] public NativeArray<uint> UnderLut;

            public NativeList<GraphVertex> OutVerts;
            public NativeList<int> OutIndices;

            public float2 Origin;
            public float Width;
            public float Height;
            public bool Normalize;

            public void Execute()
            {
                var pc = Points.Length;
                if (pc < 2)
                    return;

                OutVerts.Capacity = math.max(OutVerts.Capacity, pc * 2);
                OutIndices.Capacity = math.max(OutIndices.Capacity, (pc - 1) * 6);

                for (var i = 0; i < pc; i++)
                {
                    var uv = Points[i];
                    if (uv.x == NullPoint.x && uv.y == NullPoint.y)
                        continue;

                    var top = new float2(Width * uv.x, Height * uv.y) + Origin;
                    var bottom = new float2(top.x, Origin.y);

                    var cTop = Normalize ? SampleLut(UnderLut, 1f - math.clamp(uv.y, 0f, 1f)) : SampleLut(UnderLut, 0);

                    var cBottom = SampleLut(UnderLut, 1f);

                    var vIndex = OutVerts.Length;
                    OutVerts.Add(new GraphVertex { Pos = top, Color = cTop });
                    OutVerts.Add(new GraphVertex { Pos = bottom, Color = cBottom });

                    if (i < pc - 1)
                    {
                        OutIndices.Add(vIndex);
                        OutIndices.Add(vIndex + 1);
                        OutIndices.Add(vIndex + 2);

                        OutIndices.Add(vIndex + 1);
                        OutIndices.Add(vIndex + 2);
                        OutIndices.Add(vIndex + 3);
                    }
                }
            }

            private static uint SampleLut(NativeArray<uint> lut, float t)
            {
                var i = (int)math.round(math.clamp(t, 0f, 1f) * (lut.Length - 1));
                return lut[i];
            }
        }
    }
}