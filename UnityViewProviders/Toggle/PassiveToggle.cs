using DingoUnityExtensions.UnityViewProviders.Core;
using DingoUnityExtensions.UnityViewProviders.Toggle.Core;
using UnityEngine;

namespace DingoUnityExtensions.UnityViewProviders.Toggle
{
    public class PassiveToggle : ValueContainer<bool>
    {
        [SerializeField] protected ToggleSwapInfoBase ToggleSwapInfo;

        protected override void SetValueWithoutNotify(bool value)
        {
            ToggleSwapInfo.SetViewActive(value);
        }
    }
}