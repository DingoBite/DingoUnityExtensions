using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DingoUnityExtensions.MonoBehaviours.UI
{
    public class DragController : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        [SerializeField] private float _timeToStartDrag;
        [SerializeField] private float _distanceToDragThreshold = 0.01f;
        
        public event Action<Vector2> StartDrag;
        public event Action<Vector2> Drag;
        public event Action<Vector2> EndDrag;

        private Vector2 _preStartDrag;
        private Vector2 _startDrag;
        private Vector2 _currentDrag;
        private Vector2 _endDrag;

        private bool _started;
        private float _currentDragTime;

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_timeToStartDrag > Vector2.kEpsilon)
            {
                _preStartDrag = eventData.position;
                _currentDragTime = 0;
                return;
            }
            
            _startDrag = eventData.position;
            StartDrag?.Invoke(_startDrag);
            _started = true;
        }
        
        public void OnDrag(PointerEventData eventData)
        {
            var position = eventData.position;
            if (_timeToStartDrag > Vector2.kEpsilon)
            {
                if (_currentDragTime < _timeToStartDrag)
                {
                    _currentDragTime += Time.deltaTime;
                    return;
                }
                var dist = Vector2.Distance(_preStartDrag, position);
                if (dist < _distanceToDragThreshold)
                    return;

                if (!_started)
                {
                    _startDrag = position;
                    StartDrag?.Invoke(_startDrag);
                    _started = true;
                }
                
                _currentDrag = position;
                Drag?.Invoke(_currentDrag);
                return;
            }

            _currentDrag = position;
            Drag?.Invoke(_currentDrag);
        }
        
        public void OnEndDrag(PointerEventData eventData)
        {
            _started = false;
            if (_timeToStartDrag > Vector2.kEpsilon)
            {
                if (_currentDragTime < _timeToStartDrag)
                {
                    _currentDragTime = 0;
                    return;
                }
            }

            _currentDragTime = 0;
            _endDrag = eventData.position;
            EndDrag?.Invoke(_endDrag);
        }
    }
}