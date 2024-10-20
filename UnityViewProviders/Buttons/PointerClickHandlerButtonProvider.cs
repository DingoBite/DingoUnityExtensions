using DingoUnityExtensions.UnityViewProviders.PointerHandlerWrappers;
using UnityEngine;

namespace DingoUnityExtensions.UnityViewProviders.Buttons
{
    [RequireComponent(typeof(PointerHandlerClick))]
    public class PointerClickHandlerButtonProvider : PointerHandlersButtonProvider<PointerHandlerClick>
    {
    }
}