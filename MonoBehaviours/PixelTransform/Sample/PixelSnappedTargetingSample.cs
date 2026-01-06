using System.Collections.Generic;
using DingoUnityExtensions.MonoBehaviours.PixelTransform.Movement;
using NaughtyAttributes;
using UnityEngine;

namespace DingoUnityExtensions.MonoBehaviours.PixelTransform.Sample
{
    [DisallowMultipleComponent]
    public sealed class PixelSnappedTargetingSample : SubscribableBehaviour
    {
        [SerializeField] private PixelSnappedTransform _to;
        [SerializeField] private List<PixelSnappedTransform> _movers = new();
        [SerializeField] private LinearMoveParameters _moveParameters;
        
        [SerializeField] private bool _autoTick = true;

        private LinearPixelMove _move;

        private readonly Dictionary<PixelSnappedTransform, Vector2> _moverPx = new();
        
        public void TickDeltaTime() => Tick(Time.deltaTime);

        public void Tick(float dt)
        {
            EnsureMove();
            if (_move == null || !_move.IsActive)
                return;

            var targetPx = GetTargetPx();
            _move.SetTarget(targetPx);

            foreach (var mover in _movers)
            {
                if (mover == null)
                    continue;

                var arrived = _move.Tick(dt, mover);
            }
        }
        
        private void EnsureMove()
        {
            if (_move == null || _moveParameters == null)
                _move = new LinearPixelMove(_moveParameters);
        }

        [Button]
        public void StartMove() => _move?.Resume();
        [Button]
        public void PauseMove() => _move?.Pause();
        
        public void SnapTargets()
        {
            if (_to != null)
                _to.SetPosition(_to.GetAnchoredPosition());
        }
        
        private Vector2 GetTargetPx()
        {
            var pst = _to;
            return pst != null ? pst.GetAnchoredPosition() : Vector2.zero;
        }

        private Vector2 GetMoverPx(PixelSnappedTransform mover)
        {
            if (mover == null)
                return Vector2.zero;

            if (_moverPx.TryGetValue(mover, out var v))
                return v;

            v = mover.GetAnchoredPosition();
            _moverPx[mover] = v;
            return v;
        }

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
