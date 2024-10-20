using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DingoUnityExtensions.MonoBehaviours.UI.Scrolls
{
    [RequireComponent(typeof(ScrollRectDragWrapper))]
    public class ThresholdFilterDragScroll : SubscribableBehaviour
    {
        public event Action<Vector2> FilterDragStart; 
        
        [field: SerializeField] public ScrollRectDragWrapper Wrapper { get; private set; }
        
        [SerializeField] private Vector2 _minFilter;

        private bool _started;
        private bool _filteredStarted;
        private Vector2 _startPosition;

        private void Reset() => Wrapper = GetComponent<ScrollRectDragWrapper>();

        public void OnBeginDragEvent(PointerEventData data, float t)
        {
            _started = true;
            _startPosition = data.position;
        }

        public void OnDragEvent(PointerEventData data, float t)
        {
            if (_filteredStarted)
                return;

            var position = data.position;
            
            var isFilterX = _startPosition.x < 0 || Math.Abs(position.x - _startPosition.x) - _minFilter.x > 0;
            var isFilterY = _startPosition.y < 0 || Math.Abs(position.y - _startPosition.y) - _minFilter.y > 0;
            if (isFilterX && isFilterY)
            {
                _filteredStarted = true;
                FilterDragStart?.Invoke(position);
            }
        }

        public void OnEndDragEvent(PointerEventData data, float t)
        {
            _started = false;
            _filteredStarted = false;
        }
        
        protected override void SubscribeOnly()
        {
            Wrapper.BeginDragEvent += OnBeginDragEvent;
            Wrapper.DragEvent += OnDragEvent;
            Wrapper.EndDragEvent += OnEndDragEvent;
        }

        protected override void UnsubscribeOnly()
        {
            Wrapper.BeginDragEvent -= OnBeginDragEvent;
            Wrapper.DragEvent -= OnDragEvent;
            Wrapper.EndDragEvent -= OnEndDragEvent;
        }
    }
}