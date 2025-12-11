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
    public class UIShapes : MaskableGraphic
    {
        private readonly List<UIGraphShape> _graphShapes = new(64);
        
        private NativeList<ShapeJobInput> _shapeInputs;
        private NativeList<float2> _shapePoints;
        private NativeList<uint> _shapeLut;
        private NativeList<GraphVertex> _shapeVerts;
        private NativeList<int> _shapeIndices;

        private float _width;
        private float _height;

        private const int ShapeLutSize = 256;
        
        protected override void OnEnable()
        {
            base.OnEnable();
            EnsureNative();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            DisposeNative();
        }

        private void EnsureNative()
        {
            if (!_shapeInputs.IsCreated)
                _shapeInputs = new NativeList<ShapeJobInput>(Allocator.Persistent);
            if (!_shapePoints.IsCreated)
                _shapePoints = new NativeList<float2>(Allocator.Persistent);
            if (!_shapeLut.IsCreated)
                _shapeLut = new NativeList<uint>(Allocator.Persistent);
            if (!_shapeVerts.IsCreated)
                _shapeVerts = new NativeList<GraphVertex>(Allocator.Persistent);
            if (!_shapeIndices.IsCreated)
                _shapeIndices = new NativeList<int>(Allocator.Persistent);
        }

        private void DisposeNative()
        {
            if (_shapeInputs.IsCreated)
                _shapeInputs.Dispose();
            if (_shapePoints.IsCreated)
                _shapePoints.Dispose();
            if (_shapeLut.IsCreated)
                _shapeLut.Dispose();
            if (_shapeVerts.IsCreated)
                _shapeVerts.Dispose();
            if (_shapeIndices.IsCreated)
                _shapeIndices.Dispose();
        }

        public int AddShape(UIGraphShape shape)
        {
            _graphShapes.Add(shape);
            SetVerticesDirty();
            return _graphShapes.Count - 1;
        }

        public int AddGraphShape(UIGraphShape shape) => AddShape(shape);

        public void RemoveShape(int index)
        {
            if ((uint)index >= (uint)_graphShapes.Count)
                return;

            _graphShapes[index] = UIGraphShape.NullShape;
            SetVerticesDirty();
        }

        public void ClearShapes()
        {
            _graphShapes.Clear();

            if (_shapeInputs.IsCreated)
                _shapeInputs.Clear();
            if (_shapePoints.IsCreated)
                _shapePoints.Clear();
            if (_shapeLut.IsCreated)
                _shapeLut.Clear();
            if (_shapeVerts.IsCreated)
                _shapeVerts.Clear();
            if (_shapeIndices.IsCreated)
                _shapeIndices.Clear();

            SetVerticesDirty();
            SetMaterialDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            EnsureNative();

            vh.Clear();

            var r = rectTransform.rect;
            _width = r.width;
            _height = r.height;

            var pivot = rectTransform.pivot;
            var origin = new float2(-pivot.x * _width, -pivot.y * _height);

            _shapeInputs.Clear();
            _shapePoints.Clear();
            _shapeLut.Clear();
            _shapeVerts.Clear();
            _shapeIndices.Clear();

            if (_graphShapes.Count == 0)
                return;

            var baseColor = color;
            var lutOffset = 0;

            for (int s = 0; s < _graphShapes.Count; s++)
            {
                var shape = _graphShapes[s];
                if (UIGraphShape.IsNull(shape))
                    continue;

                switch (shape.ShapeType)
                {
                    case ShapeType.Line:
                    case ShapeType.StrokeShape:
                        if (shape.Thickness < Vector2.kEpsilon)
                            continue;
                        break;
                    case ShapeType.FilledShape: break;
                    default: continue;
                }

                var pointsSrc = shape.Points as IList<Vector2> ?? new List<Vector2>(shape.Points);
                var count = pointsSrc.Count;

                if (shape.ShapeType == ShapeType.FilledShape)
                {
                    if (count < 3)
                        continue;
                }
                else
                {
                    if (count < 2)
                        continue;
                }

                var input = new ShapeJobInput
                {
                    Type = (int)shape.ShapeType,
                    PointsOffset = _shapePoints.Length,
                    PointsCount = count,
                    Thickness = shape.Thickness,
                    LutOffset = lutOffset
                };

                for (int i = 0; i < count; i++)
                {
                    var uv = pointsSrc[i];
                    _shapePoints.Add(new float2(uv.x, uv.y));
                }

                for (int i = 0; i < ShapeLutSize; i++)
                {
                    var t = i / (ShapeLutSize - 1f);
                    var c = shape.EvaluateColor(t) * baseColor;
                    _shapeLut.Add(Pack(c));
                }

                lutOffset += ShapeLutSize;
                _shapeInputs.Add(input);
            }

            if (_shapeInputs.Length == 0)
                return;

            var job = new BuildShapesJob
            {
                Shapes = _shapeInputs.AsArray(),
                Points = _shapePoints.AsArray(),
                Lut = _shapeLut.AsArray(),
                OutVerts = _shapeVerts,
                OutIndices = _shapeIndices,
                Origin = origin,
                Width = _width,
                Height = _height,
                LutSize = ShapeLutSize
            };

            job.Run();

            var baseIndex = 0;

            for (int i = 0; i < _shapeVerts.Length; i++)
            {
                var v = _shapeVerts[i];
                var ui = UIVertex.simpleVert;
                ui.position = new Vector3(v.Pos.x, v.Pos.y, 0f);
                ui.color = Unpack(v.Color);
                vh.AddVert(ui);
            }

            for (int i = 0; i < _shapeIndices.Length; i += 3)
            {
                vh.AddTriangle(baseIndex + _shapeIndices[i], baseIndex + _shapeIndices[i + 1], baseIndex + _shapeIndices[i + 2]);
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

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            SetVerticesDirty();
            SetMaterialDirty();
        }
#endif

        private struct ShapeJobInput
        {
            public int Type;
            public int PointsOffset;
            public int PointsCount;
            public float Thickness;
            public int LutOffset;
        }

        private struct GraphVertex
        {
            public float2 Pos;
            public uint Color;
        }

        [BurstCompile]
        private struct BuildShapesJob : IJob
        {
            [ReadOnly] public NativeArray<ShapeJobInput> Shapes;
            [ReadOnly] public NativeArray<float2> Points;
            [ReadOnly] public NativeArray<uint> Lut;

            public NativeList<GraphVertex> OutVerts;
            public NativeList<int> OutIndices;

            public float2 Origin;
            public float Width;
            public float Height;
            public int LutSize;

            public void Execute()
            {
                for (int s = 0; s < Shapes.Length; s++)
                {
                    var shape = Shapes[s];
                    switch ((ShapeType)shape.Type)
                    {
                        case ShapeType.Line:
                        case ShapeType.StrokeShape:
                            BuildLine(shape);
                            break;
                        case ShapeType.FilledShape:
                            BuildFill(shape);
                            break;
                    }
                }
            }

            private void BuildLine(ShapeJobInput shape)
            {
                var pc = shape.PointsCount;
                if (pc < 2 || shape.Thickness <= 1e-6f)
                    return;

                var offset = shape.PointsOffset;
                var baseVert = OutVerts.Length;

                var uv0 = Points[offset + 0];
                var uv1 = Points[offset + 1];

                var p0 = Local(uv0);
                var p1 = Local(uv1);

                var nPrev = Normal(p0, p1);

                var c0 = ColorAt(shape, 0, pc);
                AddThick(p0, nPrev, c0, shape.Thickness);

                AddQuad(baseVert);

                for (int i = 1; i < pc - 1; i++)
                {
                    var uvi = Points[offset + i];
                    var uvNext = Points[offset + i + 1];

                    var pos = Local(uvi);
                    var col = ColorAt(shape, i, pc);

                    AddThick(pos, nPrev, col, shape.Thickness);

                    var pNext = Local(uvNext);
                    var nNext = Normal(pos, pNext);

                    AddThick(pos, nNext, col, shape.Thickness);

                    var vBase = baseVert + i * 4;
                    AddQuad(vBase);
                    AddQuad(vBase - 2);

                    nPrev = nNext;
                }

                var uvLast = Points[offset + pc - 1];
                var pLast = Local(uvLast);
                var cLast = ColorAt(shape, pc - 1, pc);
                AddThick(pLast, nPrev, cLast, shape.Thickness);
            }

            private void BuildFill(ShapeJobInput shape)
            {
                var pc = shape.PointsCount;
                if (pc < 3)
                    return;

                var offset = shape.PointsOffset;
                var baseVert = OutVerts.Length;

                var c = ColorAt(shape, 0, pc);

                for (int i = 0; i < pc; i++)
                {
                    var uv = Points[offset + i];
                    var p = Local(uv);
                    OutVerts.Add(new GraphVertex { Pos = p, Color = c });
                }

                for (int i = 1; i < pc - 1; i++)
                {
                    OutIndices.Add(baseVert);
                    OutIndices.Add(baseVert + i);
                    OutIndices.Add(baseVert + i + 1);
                }
            }

            private float2 Local(float2 uv)
            {
                return new float2(Width * uv.x, Height * uv.y) + Origin;
            }

            private float2 Normal(float2 p1, float2 p2)
            {
                var d = p2 - p1;
                if (math.lengthsq(d) < 1e-12f)
                    return new float2(0f, 1f);
                return math.normalize(new float2(-d.y, d.x));
            }

            private void AddThick(float2 pos, float2 n, uint col, float thickness)
            {
                var t = n * (thickness * 0.5f);
                OutVerts.Add(new GraphVertex { Pos = pos - t, Color = col });
                OutVerts.Add(new GraphVertex { Pos = pos + t, Color = col });
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

            private uint ColorAt(ShapeJobInput shape, int i, int pc)
            {
                float t;
                if (i <= 0)
                    t = 0f;
                else if (i >= pc - 1)
                    t = 1f;
                else
                    t = (float)i / pc;

                return SampleLut(shape, t);
            }

            private uint SampleLut(ShapeJobInput shape, float t)
            {
                var idx = (int)math.round(math.clamp(t, 0f, 1f) * (LutSize - 1));
                return Lut[shape.LutOffset + idx];
            }
        }
    }
}