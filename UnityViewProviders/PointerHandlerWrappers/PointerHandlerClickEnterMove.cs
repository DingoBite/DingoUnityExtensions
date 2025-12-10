using UnityEngine;
using UnityEngine.EventSystems;

namespace DingoUnityExtensions.UnityViewProviders.PointerHandlerWrappers
{
    public class PointerHandlerClickEnterMove : PointerHandlerClickEnter, IPointerMoveHandler
    {
        public event PointerWrapperDelegates.Event PointerMoveEvent;
        
        public void OnPointerMove(PointerEventData eventData) => PointerMoveEvent?.Invoke(eventData, Time.time - EnterTime);
    }
}