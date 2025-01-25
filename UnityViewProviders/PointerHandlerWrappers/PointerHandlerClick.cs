using UnityEngine;
using UnityEngine.EventSystems;

namespace DingoUnityExtensions.UnityViewProviders.PointerHandlerWrappers
{
    public class PointerHandlerClick : MonoBehaviour, IPointerClickEventWrapper, IPointerDownEventWrapper, IPointerUpEventWrapper
    {
        public event PointerWrapperDelegates.Event PointerClickEvent;
        public event PointerWrapperDelegates.Event PointerDownEvent;
        public event PointerWrapperDelegates.Event PointerUpEvent;
        
        private float _downTime;
        
        public float LastDownTime { get; private set; }
        protected bool Down { get; private set; }
        protected bool Up { get; private set; }

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            Down = false;
            Up = true;
            LastDownTime = Time.time - _downTime;
            PointerClickEvent?.Invoke(eventData, 0);
        }

        public virtual void OnPointerDown(PointerEventData eventData)
        {
            _downTime = Time.time;
            Up = false;
            Down = true;
            PointerDownEvent?.Invoke(eventData, Time.time - _downTime);
        }

        public virtual void OnPointerUp(PointerEventData eventData)
        {
            Up = true;
            Down = false;
            LastDownTime = Time.time - _downTime;
            PointerUpEvent?.Invoke(eventData, LastDownTime);
        }
    }
}