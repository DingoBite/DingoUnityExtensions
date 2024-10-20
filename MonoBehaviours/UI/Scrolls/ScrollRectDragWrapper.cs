using System;
using DingoUnityExtensions.UnityViewProviders.PointerHandlerWrappers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DingoUnityExtensions.MonoBehaviours.UI.Scrolls
{
    public class ScrollRectDragWrapper : SubscribableBehaviour
    {
        [SerializeField] private PointerHandlerClickDrag _pointerHandlerClick;
        [SerializeField] private ScrollRect _scrollRect;

        public event PointerWrapperDelegates.Event BeginDragEvent;
        public event PointerWrapperDelegates.Event DragEvent;
        public event PointerWrapperDelegates.Event EndDragEvent;

        protected Vector2 StartDrag { get; private set; }
        protected Vector2 CurrentDrag { get; private set; }
        protected Vector2 EndDrag { get; private set; }

        protected float DragTime { get; private set; }

        public ScrollRect ScrollRect => _scrollRect;

        public void SetScrollRect(ScrollRect scrollRect)
        {
            if (scrollRect == null)
            {
                Debug.LogException(new NullReferenceException(nameof(scrollRect)), this);
                return;
            }
            _scrollRect = scrollRect;
        }
        
        private void OnBeginDrag(PointerEventData eventData, float time)
        {
            DragTime = time;
            if (_scrollRect != null)
                ParentScrollBeginDragEvent(eventData);
            StartDrag = eventData.position;
            BeginDragEvent?.Invoke(eventData, DragTime);
        }

        private void OnDrag(PointerEventData eventData, float time)
        {
            DragTime = time;
            if (_scrollRect != null)
                ParentScrollDragEvent(eventData);
            CurrentDrag = eventData.position;
            DragEvent?.Invoke(eventData, DragTime);
        }

        private void OnEndDrag(PointerEventData eventData, float time)
        {
            DragTime = time;
            if (_scrollRect != null)
                ParentScrollEndDragEvent(eventData);
            EndDrag = eventData.position;
            EndDragEvent?.Invoke(eventData, DragTime);
        }

        private void ParentScrollBeginDragEvent(PointerEventData eventData)
        {
            var pointerDrag = eventData.pointerDrag;
            eventData.pointerDrag = _scrollRect.gameObject;
            _scrollRect.OnInitializePotentialDrag(eventData);
            _scrollRect.OnBeginDrag(eventData);
            eventData.pointerDrag = pointerDrag;
        }

        private void ParentScrollDragEvent(PointerEventData eventData)
        {
            var pointerDrag = eventData.pointerDrag;
            eventData.pointerDrag = _scrollRect.gameObject;
            _scrollRect.OnInitializePotentialDrag(eventData);
            _scrollRect.OnDrag(eventData);
            eventData.pointerDrag = pointerDrag;
        }

        private void ParentScrollEndDragEvent(PointerEventData eventData)
        {
            var pointerDrag = eventData.pointerDrag;
            eventData.pointerDrag = _scrollRect.gameObject;
            _scrollRect.OnEndDrag(eventData);
            eventData.pointerDrag = pointerDrag;
        }

        private void Reset() => _scrollRect = GetComponent<ScrollRect>();

        protected override void SubscribeOnly()
        {
            _pointerHandlerClick.BeginDragEvent += OnBeginDrag;
            _pointerHandlerClick.DragEvent += OnDrag;
            _pointerHandlerClick.EndDragEvent += OnEndDrag;
        }

        protected override void UnsubscribeOnly()
        {
            _pointerHandlerClick.BeginDragEvent -= OnBeginDrag;
            _pointerHandlerClick.DragEvent -= OnDrag;
            _pointerHandlerClick.EndDragEvent -= OnEndDrag;
        }
    }
}