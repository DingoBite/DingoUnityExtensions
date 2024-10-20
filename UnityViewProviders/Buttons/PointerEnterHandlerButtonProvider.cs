using DingoUnityExtensions.UnityViewProviders.PointerHandlerWrappers;
using UnityEngine;

namespace DingoUnityExtensions.UnityViewProviders.Buttons
{
    [RequireComponent(typeof(PointerHandlerClickEnter))]
    public class PointerEnterHandlerButtonProvider : PointerEnterHandlersButtonProvider<PointerHandlerClickEnter>
    {
    }
}