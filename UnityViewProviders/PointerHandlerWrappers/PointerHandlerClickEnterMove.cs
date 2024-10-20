using UnityEngine.EventSystems;

namespace DingoUnityExtensions.UnityViewProviders.PointerHandlerWrappers
{
    public class PointerHandlerClickEnterMove : PointerHandlerClickEnter, IPointerMoveHandler
    {
        public void OnPointerMove(PointerEventData eventData) { }
    }
}