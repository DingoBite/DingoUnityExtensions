using UnityEngine;
using UnityEngine.EventSystems;

namespace DingoUnityExtensions.UnityViewProviders.PointerHandlerWrappers
{
    public class PointerHandlerClickEnter : PointerHandlerClick, IPointerEnterEventWrapper, IPointerExitEventWrapper
    {
        public event PointerWrapperDelegates.Event PointerEnterEvent;
        public event PointerWrapperDelegates.Event PointerExitEvent;

        private float _enterTime;
        
        protected bool Entered { get; private set; }
        protected bool Exit { get; private set; }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            _enterTime = Time.time;
            Entered = true;
            PointerEnterEvent?.Invoke(eventData, 0);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Exit = true;
            Entered = false;
            PointerExitEvent?.Invoke(eventData, Time.time - _enterTime);
        }
    }
}