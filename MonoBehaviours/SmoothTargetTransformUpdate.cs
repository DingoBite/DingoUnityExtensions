using DingoUnityExtensions.Extensions;
using UnityEngine;

namespace DingoUnityExtensions.MonoBehaviours
{
    public class SmoothTargetTransformUpdate : MonoBehaviour
    {
        [SerializeField] private TransformTargetPair _transformTargetPair;
        [SerializeField] private float _speed = 10f;
        
        private void Update() => _transformTargetPair.Move(_speed * Time.deltaTime);
    }
}