using UnityEngine;

namespace DingoUnityExtensions.DevView
{
    public class Point : MonoBehaviour
    {
        [SerializeField] private Transform _point;
        [SerializeField] private MeshRenderer _renderer;

        public Transform Tr => _point;
        
        public Color Color
        {
            get => _renderer.material.color;
            set => _renderer.material.color = value;
        }
    }
}