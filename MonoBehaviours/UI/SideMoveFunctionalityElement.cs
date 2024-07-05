using System;
using DingoUnityExtensions.UnityViewProviders.Core;
using UnityEngine;

namespace DingoUnityExtensions.MonoBehaviours.UI
{
    // IN DEV MODE
    public class SideMoveFunctionalityElement : MonoBehaviour
    {
        [Header("Controls")] 
        [SerializeField] private EventContainer _deleteButton;
        [SerializeField] private EventContainer _statsButton;
        [SerializeField] private DragController _dragController;
        [SerializeField] private RectTransform _movableContent;

        [Header("Parameters")] 
        [SerializeField] private AnimationCurve _offsetCurve;

        [SerializeField] private float _startOffsetMove = 1;
        [SerializeField] private float _maxX = -100;
        [SerializeField] private float _minX;
        [SerializeField] private float _lockThreshold = 30;
        
        [SerializeField] private float _debugScrollValue;
        
        private float _startDrag;
        private float _currentDrag;
        private float _endDrag;

        private bool _stopNextInput;
        
        private float _scrollValue;

        private bool _isMoved;

        public void SmoothDisable()
        {
            ReturnEndDrag();
        }

        private void StartDrag(Vector2 value)
        {
            _stopNextInput = true;
            _startDrag = value.x;
        }

        private void Drag(Vector2 value)
        {
            if (_isMoved)
                _currentDrag = value.x + (_maxX - _minX);
            else 
                _currentDrag = value.x;
            UpdateScroll();
        }

        private void UpdateScroll()
        {
            var dist = _currentDrag - _startDrag;
            var percent = dist / (_maxX - _minX);
            SetScrollValue(percent);
        }

        private void EndDrag(Vector2 value)
        {
            _endDrag = value.x;
            var dist = _endDrag - _startDrag;
            var absDist = Math.Abs(dist);
            var isReturn = _isMoved ? dist > absDist - _lockThreshold : -dist < _lockThreshold;
            if (isReturn)
                ReturnEndDrag();
            else
                LockEndDrag();
        }

        private void ReturnEndDrag()
        {
            _isMoved = false;
        }

        private void LockEndDrag()
        {
            _isMoved = true;
        }

        private void SetScrollValue(float value)
        {
            if (_startOffsetMove > 0)
            {
                value = Math.Clamp(value, 0, 1 + _startOffsetMove);
                if (value > _startOffsetMove)
                {
                    var offset = value - _startOffsetMove;
                    offset = SmoothOffset(offset);
                    value = _startOffsetMove + offset;
                }
            }
            else
            {
                value = Math.Clamp(value, 0, 1);
            }
            var pos = _movableContent.anchoredPosition;
            pos.x = _minX + value * (_maxX - _minX);
            _movableContent.anchoredPosition = pos;
            _scrollValue = value;
        }

        private float SmoothOffset(float value)
        {
            return _offsetCurve.Evaluate(value);
        }
        
        private float GetScrollValue() => _scrollValue;

        public void ResetView()
        {
            _isMoved = false;
            SetScrollValue(0);
        }
        
        protected void SubscribeOnly()
        {
            _dragController.StartDrag += StartDrag;
            _dragController.Drag += Drag;
            _dragController.EndDrag += EndDrag;
        }

        protected void UnsubscribeOnly()
        {
            _dragController.StartDrag -= StartDrag;
            _dragController.Drag -= Drag;
            _dragController.EndDrag -= EndDrag;
        }
        
        protected void Validate()
        {
            if (_debugScrollValue >= 0 && _movableContent != null)
                SetScrollValue(_debugScrollValue);
        }
    }
}