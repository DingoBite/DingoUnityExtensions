using System.Collections.Generic;
using DingoUnityExtensions.MonoBehaviours.PixelTransform.Movement;
using DingoUnityExtensions.MonoBehaviours.PixelTransform.Movement.Snake;
using NaughtyAttributes;
using UnityEngine;

namespace DingoUnityExtensions.MonoBehaviours.PixelTransform.Sample
{
    [DisallowMultipleComponent]
    public sealed class PixelSnakeSample : SubscribableBehaviour
    {
        [Header("Snake")]
        [SerializeField] private PixelSnappedTransform _head;
        [SerializeField] private List<PixelSnappedTransform> _body = new();

        [Header("Movement")]
        [SerializeField] private LinearMoveParameters _headMoveParameters;
        [SerializeField] private Vector2Int _cellSizePx = new(64, 64);
        [SerializeField] private int _segmentSpacingPx = 64;

        [Header("Runtime")]
        [SerializeField] private bool _autoTick = true;
        [SerializeField] private bool _moving = true;

        [Header("Trail")]
        [SerializeField] private int _extraTrailPx = 256;

        private LinearPixelMove _headMove;
        private SnakeTrailFollower _follower;

        private Vector2Int _dir = Vector2Int.right;
        private Vector2Int _pendingDir;
        private bool _hasPendingDir;

        private bool _hasTarget;

        public void TickDeltaTime() => Tick(Time.deltaTime);

        public void Tick(float dt)
        {
            EnsureSystems();

            if (!_moving || _head == null || _headMove == null || _follower == null)
                return;

            ReadInput();

            if (!_hasTarget)
                SetNextCellTarget();

            var arrived = _headMove.Tick(dt, _head);

            _head.SnapPosition();

            _follower.RecordHead(_head.PositionPxRounded);
            _follower.Apply(_body);

            if (!arrived)
                return;

            ApplyPendingDirection();
            SetNextCellTarget();
        }

        [Button]
        public void ResumeMove() => _moving = true;

        [Button]
        public void PauseMove() => _moving = false;

        [Button]
        public void ResetSnakeToHead()
        {
            if (_head == null)
                return;

            EnsureSystems();

            var headPx = _head.PositionPxRounded;

            for (var i = 0; i < _body.Count; i++)
            {
                var seg = _body[i];
                if (seg == null)
                    continue;

                seg.SetPositionPx(headPx);
                seg.SnapPosition();
            }

            _follower?.Reset(_head.PositionPxRounded);

            _dir = Vector2Int.right;
            _hasPendingDir = false;
            _hasTarget = false;
        }

        private void EnsureSystems()
        {
            if (_headMove == null && _headMoveParameters != null)
                _headMove = new LinearPixelMove(_headMoveParameters);

            var spacing = Mathf.Max(1, _segmentSpacingPx);
            var capacity = ComputeTrailCapacityPx(spacing);

            if (_follower == null)
                _follower = new SnakeTrailFollower(capacity, spacing);
            else
            {
                _follower.SetSpacing(spacing);
                _follower.EnsureCapacity(capacity);
            }

            if (_head != null && _follower != null && !_hasTarget && _followerSpacingMismatch())
            { }

            if (_head != null && _follower != null && _followerNeedsInit())
                _follower.Reset(_head.PositionPxRounded);

            bool _followerNeedsInit()
            {
                return false;
            }

            bool _followerSpacingMismatch()
            {
                return false;
            }
        }

        private int ComputeTrailCapacityPx(int spacingPx)
        {
            var segCount = _body != null ? _body.Count : 0;
            var need = (segCount + 4) * spacingPx + Mathf.Max(0, _extraTrailPx);
            return Mathf.Max(64, need);
        }

        private void SetNextCellTarget()
        {
            if (_headMove == null || _head == null)
                return;

            var cell = new Vector2Int(
                _dir.x * Mathf.Max(1, _cellSizePx.x),
                _dir.y * Mathf.Max(1, _cellSizePx.y));

            var from = _head.PositionPxRounded;
            var to = from + cell;

            _headMove.SetTarget(to);
            _hasTarget = true;
        }

        private void ReadInput()
        {
            var next = _dir;

            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
                next = Vector2Int.up;
            else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
                next = Vector2Int.down;
            else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
                next = Vector2Int.left;
            else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
                next = Vector2Int.right;

            if (next == _dir)
                return;

            if (_body != null && _body.Count > 0 && IsOpposite(next, _dir))
                return;

            _pendingDir = next;
            _hasPendingDir = true;
        }

        private void ApplyPendingDirection()
        {
            if (!_hasPendingDir)
                return;

            _dir = _pendingDir;
            _hasPendingDir = false;
        }

        private static bool IsOpposite(Vector2Int a, Vector2Int b) => a.x + b.x == 0 && a.y + b.y == 0;

        protected override void SubscribeOnly()
        {
            if (_autoTick)
                CoroutineParent.AddLateUpdater(this, TickDeltaTime);
        }

        protected override void UnsubscribeOnly()
        {
            CoroutineParent.RemoveLateUpdater(this);
        }
    }
}
