using UnityEngine;
using UnityEngine.EventSystems;

namespace DingoUnityExtensions.MonoBehaviours.UI.RaycastSafetyArea
{
    public class OverHandlingArea : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public bool IsOver { get; private set; }

        public void OnPointerEnter(PointerEventData eventData) => IsOver = true;

        public void OnPointerExit(PointerEventData eventData) => IsOver = false;
    }
}