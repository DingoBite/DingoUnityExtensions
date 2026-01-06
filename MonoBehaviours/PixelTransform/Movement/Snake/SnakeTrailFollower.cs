using System.Collections.Generic;
using DingoUnityExtensions.MonoBehaviours.PixelTransform.Movement.Core;
using UnityEngine;

namespace DingoUnityExtensions.MonoBehaviours.PixelTransform.Movement.Snake
{
    public sealed class SnakeTrailFollower
    {
        private readonly PixelTrailBuffer _trail;

        private int _spacingPx;
        private bool _hasLast;
        private Vector2Int _last;

        public int SpacingPx => _spacingPx;

        public SnakeTrailFollower(int capacityPx, int spacingPx)
        {
            _trail = new PixelTrailBuffer(Mathf.Max(1, capacityPx));
            _spacingPx = Mathf.Max(1, spacingPx);
        }

        public void SetSpacing(int spacingPx) => _spacingPx = Mathf.Max(1, spacingPx);

        public void EnsureCapacity(int capacityPx) => _trail.EnsureCapacity(Mathf.Max(1, capacityPx));

        public void Reset(Vector2Int headPx)
        {
            _trail.Clear();
            _trail.Push(headPx);

            _last = headPx;
            _hasLast = true;
        }

        public void ResetSeeded(Vector2Int headPx, Vector2Int dir, int segmentsCount)
        {
            _trail.Clear();

            var step = new Vector2Int(Mathf.Clamp(dir.x, -1, 1), Mathf.Clamp(dir.y, -1, 1));
            if (step == Vector2Int.zero)
                step = Vector2Int.right;

            var seed = Mathf.Max(0, segmentsCount * _spacingPx);

            for (var k = seed; k >= 0; k--)
                _trail.Push(headPx - step * k);

            _last = headPx;
            _hasLast = true;
        }
        
        public void RecordHead(Vector2Int headPx)
        {
            if (!_hasLast)
            {
                Reset(headPx);
                return;
            }

            if (headPx == _last)
                return;

            AppendLinePixels(_last, headPx);
            _last = headPx;
        }

        public void Apply<T>(IReadOnlyList<T> segments) where T : class, IPixelMovable
        {
            if (segments == null || segments.Count <= 0)
                return;

            var oldest = _trail.GetOldestOrDefault();

            for (var i = 0; i < segments.Count; i++)
            {
                var seg = segments[i];
                if (seg == null)
                    continue;

                var age = (i + 1) * _spacingPx;

                if (!_trail.TryGetByAge(age, out var px))
                    px = oldest;

                seg.SetPositionPx(px);
                seg.SnapPosition();
            }
        }

        private void AppendLinePixels(Vector2Int from, Vector2Int to)
        {
            var x0 = from.x;
            var y0 = from.y;

            var x1 = to.x;
            var y1 = to.y;

            var dx = Mathf.Abs(x1 - x0);
            var sx = x0 < x1 ? 1 : -1;

            var dy = -Mathf.Abs(y1 - y0);
            var sy = y0 < y1 ? 1 : -1;

            var err = dx + dy;

            while (true)
            {
                if (x0 == x1 && y0 == y1)
                    break;

                var e2 = 2 * err;

                if (e2 >= dy)
                {
                    err += dy;
                    x0 += sx;
                }

                if (e2 <= dx)
                {
                    err += dx;
                    y0 += sy;
                }

                _trail.Push(new Vector2Int(x0, y0));
            }
        }
    }
}
