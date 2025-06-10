using DingoUnityExtensions.MonoBehaviours.Singletons;
using UnityEngine;

namespace DingoUnityExtensions.MonoBehaviours.UI.RaycastSafetyArea
{
    public class RaycastSafetyArea : SingletonProtectedBehaviour<RaycastSafetyArea>
    {
        [SerializeField] private OverHandlingArea _handlingArea;

        public static bool CheckOverSafetyArea(bool invert = false)
        {
            if (Instance == null)
                return false;
            return invert ? Instance._handlingArea.IsOver : !Instance._handlingArea.IsOver;
        }
    }
}