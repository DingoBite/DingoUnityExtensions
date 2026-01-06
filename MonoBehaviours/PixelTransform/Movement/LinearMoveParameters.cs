using DingoUnityExtensions.MonoBehaviours.PixelTransform.Movement.Core;
using UnityEngine;

namespace DingoUnityExtensions.MonoBehaviours.PixelTransform.Movement
{
    [CreateAssetMenu(fileName = nameof(LinearMoveParameters), menuName = MENU + nameof(LinearMoveParameters), order = 0)]
    public class LinearMoveParameters : MoveParameters
    {
        [Min(0f)] [SerializeField] private float _speedPxPerSecond = 64f;
        [SerializeField] private bool _snapOnArrive = true;
        [Min(0f)] [SerializeField] private float _arriveEpsilonPx = 0f;

        public float SpeedPxPerSecond => _speedPxPerSecond;
        public bool SnapOnArrive => _snapOnArrive;
        public float ArriveEpsilonPx => _arriveEpsilonPx;
    }
}