using DingoUnityExtensions.UnityViewProviders.Core.Data;
using UnityEngine;

namespace DingoUnityExtensions.UnityViewProviders.Toggle.Core
{
    public abstract class ToggleSwapInfoBase : MonoBehaviour
    {
        public abstract void SetViewActive(BoolTimeContext value);
    }
}