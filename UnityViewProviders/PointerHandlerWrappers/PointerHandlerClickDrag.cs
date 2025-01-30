using UnityEngine;
using UnityEngine.EventSystems;

namespace DingoUnityExtensions.UnityViewProviders.PointerHandlerWrappers
{
    public class PointerHandlerClickDrag : PointerHandlerClickEnter, IBeginDragEventWrapper, IEndDragEventWrapper, IDragEventWrapper
    {
        private const string HELD = "____held";
        
        [SerializeField] private bool _dragAsHeld;
        [SerializeField] private float _timeToHeldReach = -1;

        public event PointerWrapperDelegates.Event BeginDragEvent;
        public event PointerWrapperDelegates.Event EndDragEvent;
        public event PointerWrapperDelegates.Event DragEvent;
        public event PointerWrapperDelegates.Event HeldEvent;
        public event PointerWrapperDelegates.Event HeldTimeReachEvent;
        public event PointerWrapperDelegates.Event NonDragClickEvent;
        
        private PointerEventData _lastFrameDragEventData;
        
        private float _heldTime;
        private float _dragTime;
        private bool _heldTimeReached;
        private bool _isDown;
        private bool _isDragging;

        public bool IsDragAsHeld => _dragAsHeld;

        public override void OnPointerDown(PointerEventData eventData)
        {
            if (_dragAsHeld)
                CoroutineParent.AddLateUpdater((this, HELD), Held);
            _lastFrameDragEventData = eventData;
            _heldTime = Time.time;
            _heldTimeReached = false;
            _isDragging = false;
            _isDown = true;
            base.OnPointerDown(eventData);
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            if (_dragAsHeld)
                CoroutineParent.RemoveLateUpdater((this, HELD));
            if (!_isDragging && _isDown && !_heldTimeReached)
                NonDragClickEvent?.Invoke(eventData, Time.time - _heldTime);
            _isDown = false;
            base.OnPointerUp(eventData);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _isDragging = true;
            _dragTime = Time.time;
            if (_dragAsHeld)
                CoroutineParent.RemoveLateUpdater((this, HELD));
            BeginDragEvent?.Invoke(eventData, 0);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (_dragAsHeld)
                CoroutineParent.RemoveLateUpdater((this, HELD));
            EndDragEvent?.Invoke(eventData, Time.time - _dragTime);
        }

        private void Held()
        {
            var dragTime = Time.time - _heldTime;
            if (_timeToHeldReach < 0 || dragTime < _timeToHeldReach)
            {
                HeldEvent?.Invoke(_lastFrameDragEventData, dragTime);
            }
            else if (dragTime >= _timeToHeldReach)
            {
                if (_heldTimeReached)
                    return;
                HeldTimeReachEvent?.Invoke(_lastFrameDragEventData, dragTime);
                _heldTimeReached = true;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_dragAsHeld)
                CoroutineParent.RemoveLateUpdater((this, HELD));
            DragEvent?.Invoke(eventData, Time.time - _dragTime);
        }
    }
}