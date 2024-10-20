using UnityEngine;
using UnityEngine.EventSystems;

namespace DingoUnityExtensions.UnityViewProviders.PointerHandlerWrappers
{
    public class PointerHandlerClickDrag : PointerHandlerClickEnter, IBeginDragEventWrapper, IEndDragEventWrapper, IDragEventWrapper
    {
        public event PointerWrapperDelegates.Event BeginDragEvent;
        public event PointerWrapperDelegates.Event EndDragEvent;
        public event PointerWrapperDelegates.Event DragEvent;

        private float _dragTime;

        public void OnBeginDrag(PointerEventData eventData)
        {
            _dragTime = Time.time;
            BeginDragEvent?.Invoke(eventData, 0);
        }
        
        public void OnEndDrag(PointerEventData eventData)
        {
            EndDragEvent?.Invoke(eventData, Time.time - _dragTime);
        }

        public void OnDrag(PointerEventData eventData)
        {
            DragEvent?.Invoke(eventData, Time.time - _dragTime);
        }
    }
}