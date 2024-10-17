using System;
using UnityEngine;

namespace DingoUnityExtensions.DevView
{
    public class Line : MonoBehaviour
    {
        [SerializeField] private Transform _p1;
        [SerializeField] private Transform _p2;
        
        [SerializeField] private LineRenderer _lineRenderer;

        public void UpdateTr(Action<Transform> tr1, Action<Transform> tr2)
        {
            tr1?.Invoke(_p1);
            tr2?.Invoke(_p2);

            UpdateLine();
        }
        
        public Color C1
        {
            get => _lineRenderer.startColor;
            set => _lineRenderer.startColor = value;
        }

        public Color C2
        {
            get => _lineRenderer.endColor;
            set => _lineRenderer.endColor = value;
        }

        public Color C
        {
            get => (_lineRenderer.startColor + _lineRenderer.endColor) * 0.5f;
            set
            {
                _lineRenderer.startColor = value;
                _lineRenderer.endColor = value;
            }
        }

        private void UpdateLine()
        {
            _lineRenderer.SetPosition(0, _p1.position);
            _lineRenderer.SetPosition(1, _p2.position);
        }
    }
}