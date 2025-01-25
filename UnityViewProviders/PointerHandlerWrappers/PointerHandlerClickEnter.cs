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

        private bool _blockNextEnter;
        
        public override void OnPointerClick(PointerEventData eventData)
        {
            if (Entered)
                _blockNextEnter = true;
            base.OnPointerClick(eventData);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_blockNextEnter)
            {
                _blockNextEnter = false;
                return;
            }
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