using DingoUnityExtensions.MonoBehaviours.Singletons;
using UnityEngine;

namespace DingoUnityExtensions.MonoBehaviours.UI.RaycastSafetyArea
{
    public class RaycastSafetyArea : ProtectedSingletonBehaviour<RaycastSafetyArea>
    {
        [SerializeField] private OverHandlingArea _handlingArea;

        public static bool CheckOverSafetyArea(bool invert = false)
        {
            if (Instance == null || Instance._handlingArea == null)
                return true;
            return invert ? Instance._handlingArea.IsOver : !Instance._handlingArea.IsOver;
        }
    }
}