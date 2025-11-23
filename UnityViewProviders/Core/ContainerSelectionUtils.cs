using UnityEngine.EventSystems;

namespace DingoUnityExtensions.UnityViewProviders.Core
{
    public static class ContainerSelectionUtils
    {
        public static void SelectViaUnity(this ContainerBase container, BaseEventData eventData = null)
        {
            if (container == null)
                return;
            if (!container.Interactable || !container.Selectable)
                return;

            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(container.gameObject, eventData);
            else
                container.Selected = true;
        }
    }
}