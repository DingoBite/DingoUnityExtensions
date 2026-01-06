using UnityEngine;

namespace DingoUnityExtensions.MonoBehaviours.PixelTransform.Movement.Snake
{
    public sealed class PixelTrailBuffer
    {
        private Vector2Int[] _buf;
        private int _head;
        private int _count;

        public int Capacity => _buf != null ? _buf.Length : 0;
        public int Count => _count;

        public int HeadIndex => _head;

        public int TailIndex
        {
            get
            {
                if (_buf == null || _buf.Length <= 0 || _count <= 0)
                    return 0;

                var idx = _head - _count;
                idx %= _buf.Length;
                if (idx < 0)
                    idx += _buf.Length;

                return idx;
            }
        }

        public int NewestIndex
        {
            get
            {
                if (_buf == null || _buf.Length <= 0 || _count <= 0)
                    return 0;

                var idx = _head - 1;
                idx %= _buf.Length;
                if (idx < 0)
                    idx += _buf.Length;

                return idx;
            }
        }

        public PixelTrailBuffer(int capacity)
        {
            _buf = new Vector2Int[Mathf.Max(1, capacity)];
            _head = 0;
            _count = 0;
        }

        public void Clear(bool wipeArray = false)
        {
            _head = 0;
            _count = 0;

            if (!wipeArray || _buf == null)
                return;

            for (var i = 0; i < _buf.Length; i++)
                _buf[i] = default;
        }

        public void EnsureCapacity(int capacity)
        {
            capacity = Mathf.Max(1, capacity);

            if (_buf != null && _buf.Length == capacity)
                return;

            var old = _buf;
            var oldCap = old != null ? old.Length : 0;
            var oldHead = _head;
            var oldCount = _count;

            _buf = new Vector2Int[capacity];
            _head = 0;
            _count = 0;

            if (old == null || oldCap <= 0 || oldCount <= 0)
                return;

            var copyCount = Mathf.Min(oldCount, capacity);

            var start = oldHead - oldCount;
            start %= oldCap;
            if (start < 0)
                start += oldCap;

            for (var i = 0; i < copyCount; i++)
            {
                var idx = (start + (oldCount - copyCount) + i) % oldCap;
                _buf[i] = old[idx];
            }

            _count = copyCount;
            _head = copyCount % _buf.Length;
        }

        public void Push(Vector2Int v)
        {
            if (_buf == null || _buf.Length <= 0)
                return;

            _buf[_head] = v;
            _head = (_head + 1) % _buf.Length;

            if (_count < _buf.Length)
                _count++;
        }

        public Vector2Int GetOldestOrDefault()
        {
            if (_count <= 0 || _buf == null || _buf.Length <= 0)
                return default;

            return _buf[TailIndex];
        }

        public bool TryGetByAge(int ageFromNewest, out Vector2Int v)
        {
            v = default;

            if (_count <= 0 || _buf == null || _buf.Length <= 0)
                return false;

            if (ageFromNewest < 0 || ageFromNewest >= _count)
                return false;

            var idx = NewestIndex - ageFromNewest;
            idx %= _buf.Length;
            if (idx < 0)
                idx += _buf.Length;

            v = _buf[idx];
            return true;
        }
    }
}
