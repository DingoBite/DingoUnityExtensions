using UnityEngine.EventSystems;

namespace DingoUnityExtensions.UnityViewProviders.PointerHandlerWrappers
{
    public static class PointerWrapperDelegates
    {
        public delegate void Event(PointerEventData data, float time);
    }
    
    public interface IPointerClickEventWrapper : IPointerClickHandler
    {
        public event PointerWrapperDelegates.Event PointerClickEvent;
    }
    
    public interface IPointerDownEventWrapper : IPointerDownHandler
    {
        public event PointerWrapperDelegates.Event PointerDownEvent;
    }
    
    public interface IPointerUpEventWrapper : IPointerUpHandler
    {
        public event PointerWrapperDelegates.Event PointerUpEvent;
    }

    public interface IBeginDragEventWrapper : IBeginDragHandler
    {
        public event PointerWrapperDelegates.Event BeginDragEvent;
    }
    
    public interface IEndDragEventWrapper : IBeginDragHandler
    {
        public event PointerWrapperDelegates.Event EndDragEvent;
    }
    
    public interface IDragEventWrapper : IBeginDragHandler
    {
        public event PointerWrapperDelegates.Event DragEvent;
    }

    public interface IPointerEnterEventWrapper : IPointerEnterHandler
    {
        public event PointerWrapperDelegates.Event PointerEnterEvent;
    }
    
    public interface IPointerExitEventWrapper : IPointerExitHandler
    {
        public event PointerWrapperDelegates.Event PointerExitEvent;
    }
    
    public interface IPointerMoveEventWrapper : IPointerMoveHandler
    {
        public event PointerWrapperDelegates.Event PointerMoveEvent;
    }
}