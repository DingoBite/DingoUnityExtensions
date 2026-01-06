using System;
using DingoUnityExtensions.MonoBehaviours.PixelTransform.Movement.Core;
using UnityEngine;

namespace DingoUnityExtensions.MonoBehaviours.PixelTransform.Movement
{
    public class LinearPixelMove : IPixelMove
    {
        private readonly LinearMoveParameters _p;
        private Vector2 _targetPx;

        public Vector2 TargetPx => _targetPx;
        public bool IsActive { get; private set; } = true;

        public event Action<IPixelMovable> Arrived;

        public LinearPixelMove(LinearMoveParameters parameters)
        {
            _p = parameters;
        }

        public void SetTarget(Vector2 targetPx) => _targetPx = targetPx;

        public void Resume() => IsActive = true;
        public void Pause() => IsActive = false;

        public bool Tick(float deltaTime, IPixelMovable movable)
        {
            if (!IsActive || movable == null || _p == null)
                return false;

            var dt = Mathf.Max(0f, deltaTime);
            var stepPx = Mathf.Max(0f, _p.SpeedPxPerSecond) * dt;

            var cur = movable.PositionPx;
            var next = Vector2.MoveTowards(cur, _targetPx, stepPx);

            movable.PositionPx = next;

            if (!IsArrived(next, _targetPx, _p.ArriveEpsilonPx))
                return false;

            if (_p.SnapOnArrive)
            {
                movable.SetPositionPx(movable.PositionPxRounded);
                movable.SnapPosition();
            }

            Arrived?.Invoke(movable);
            return true;
        }

        private static bool IsArrived(Vector2 p, Vector2 target, float epsPx)
        {
            if (epsPx <= 0f)
                return p == target;

            var d = p - target;
            return d.sqrMagnitude <= epsPx * epsPx;
        }
    }
}