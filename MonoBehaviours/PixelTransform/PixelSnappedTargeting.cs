using System;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace DingoUnityExtensions.MonoBehaviours.PixelTransform
{
    [DisallowMultipleComponent]
    public sealed class PixelSnappedTargeting : SubscribableBehaviour, IPixelSnappedTransform
    {
        public enum TargetPoint
        {
            From,
            To
        }

        [SerializeField] private PixelPerfectCamera _ppc;
        [SerializeField] private PixelSnappedTransform _from;
        [SerializeField] private PixelSnappedTransform _to;
        [SerializeField] private List<PixelSnappedTransform> _movers = new();

        [SerializeField] private float _speedPxPerSecond = 64f;
        [SerializeField] private TargetPoint _target = TargetPoint.To;

        [SerializeField] private bool _autoTick = true;
        [SerializeField] private bool _moving = true;

        [SerializeField] private bool _snapOnArrive = true;
        [SerializeField] private float _arriveEpsilonPx = 0f;

        public event Action Arrived;

        public PixelSnappedTransform From => _from;
        public PixelSnappedTransform To => _to;
        public IReadOnlyList<PixelSnappedTransform> Movers => _movers;

        private readonly Dictionary<PixelSnappedTransform, Vector2> _moverPx = new();

        public void SetupPixelPerfectCamera(Camera c)
        {
            _ppc = c != null ? c.GetComponent<PixelPerfectCamera>() : null;
            EnsureCameraForAll();
        }

        public void TickDeltaTime() => Tick(Time.deltaTime);

        public void Tick(float dt)
        {
            if (!_moving)
                return;

            EnsureCameraForAll();

            var targetPx = GetTargetPx();
            var stepPx = Mathf.Max(0f, _speedPxPerSecond) * Mathf.Max(0f, dt);

            var allArrived = true;

            for (var i = 0; i < _movers.Count; i++)
            {
                var mover = _movers[i];
                if (mover == null)
                    continue;

                var curPx = GetMoverPx(mover);
                var nextPx = Vector2.MoveTowards(curPx, targetPx, stepPx);

                _moverPx[mover] = nextPx;
                mover.SetPosition(nextPx);

                if (!IsArrived(nextPx, targetPx))
                    allArrived = false;
            }

            if (!allArrived)
                return;

            _moving = false;

            if (_snapOnArrive)
                SnapMoversToPixels();

            Arrived?.Invoke();
        }

        public void SetTarget(TargetPoint target, bool snapTargets = true)
        {
            _target = target;
            if (snapTargets)
                SnapTargets();
        }

        [Button]
        public void StartMoveTo() => StartMove(TargetPoint.To);

        [Button]
        public void StartMoveFrom() => StartMove(TargetPoint.From);

        public void StartMove(TargetPoint target)
        {
            _target = target;
            _moving = true;
            SyncMoverCacheFromCurrent();
        }

        [Button]
        public void StopMove()
        {
            _moving = false;
        }

        public void SetFromPositionPx(Vector2Int px, bool snapTargets = true)
        {
            if (_from != null)
                _from.SetPositionPx(px);
            if (snapTargets)
                SnapTargets();
        }

        public void SetToPositionPx(Vector2Int px, bool snapTargets = true)
        {
            if (_to != null)
                _to.SetPositionPx(px);
            if (snapTargets)
                SnapTargets();
        }

        public void SetFromPosition(Vector2 px, bool snapTargets = true)
        {
            if (_from != null)
                _from.SetPosition(px);
            if (snapTargets)
                SnapTargets();
        }

        public void SetToPosition(Vector2 px, bool snapTargets = true)
        {
            if (_to != null)
                _to.SetPosition(px);
            if (snapTargets)
                SnapTargets();
        }

        public void SnapTargets()
        {
            if (_from != null)
                _from.SetPosition(_from.GetAnchoredPosition());
            if (_to != null)
                _to.SetPosition(_to.GetAnchoredPosition());
        }

        public void SnapMoversToFrom()
        {
            if (_from == null)
                return;

            var px = _from.GetAnchoredPosition();
            for (var i = 0; i < _movers.Count; i++)
            {
                var mover = _movers[i];
                if (mover == null)
                    continue;

                mover.SetPosition(px);
                _moverPx[mover] = px;
            }
        }

        public void SnapMoversToTo()
        {
            if (_to == null)
                return;

            var px = _to.GetAnchoredPosition();
            for (var i = 0; i < _movers.Count; i++)
            {
                var mover = _movers[i];
                if (mover == null)
                    continue;

                mover.SetPosition(px);
                _moverPx[mover] = px;
            }
        }

        public void SnapMoversToPixels()
        {
            for (var i = 0; i < _movers.Count; i++)
            {
                var mover = _movers[i];
                if (mover == null)
                    continue;

                mover.SetPositionPx(mover.GetAnchoredPixels());
                _moverPx[mover] = mover.GetAnchoredPosition();
            }
        }

        public void AddMover(PixelSnappedTransform tr)
        {
            if (tr == null)
                return;

            if (!_movers.Contains(tr))
                _movers.Add(tr);

            _moverPx[tr] = tr.GetAnchoredPosition();
        }

        public void RemoveMover(PixelSnappedTransform tr)
        {
            if (tr == null)
                return;

            _movers.Remove(tr);
            _moverPx.Remove(tr);
        }

        private Vector2 GetTargetPx()
        {
            var pst = _target == TargetPoint.From ? _from : _to;
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

        private bool IsArrived(Vector2 moverPx, Vector2 targetPx)
        {
            if (_arriveEpsilonPx <= 0f)
                return moverPx == targetPx;

            return (moverPx - targetPx).sqrMagnitude <= _arriveEpsilonPx * _arriveEpsilonPx;
        }

        private void EnsureCameraForAll()
        {
            if (_ppc == null)
                return;

            var cam = _ppc.GetComponent<Camera>();
            if (cam == null)
                return;

            if (_from != null)
                _from.SetupPixelPerfectCamera(cam);
            if (_to != null)
                _to.SetupPixelPerfectCamera(cam);

            for (var i = 0; i < _movers.Count; i++)
            {
                var m = _movers[i];
                if (m != null)
                    m.SetupPixelPerfectCamera(cam);
            }
        }

        private void SyncMoverCacheFromCurrent()
        {
            for (var i = 0; i < _movers.Count; i++)
            {
                var mover = _movers[i];
                if (mover == null)
                    continue;

                _moverPx[mover] = mover.GetAnchoredPosition();
            }
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
