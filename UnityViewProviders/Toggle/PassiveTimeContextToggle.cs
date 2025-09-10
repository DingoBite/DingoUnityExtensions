using DingoUnityExtensions.UnityViewProviders.Core;
using DingoUnityExtensions.UnityViewProviders.Core.Data;
using DingoUnityExtensions.UnityViewProviders.Toggle.Core;
using UnityEngine;

namespace DingoUnityExtensions.UnityViewProviders.Toggle
{
    public class PassiveTimeContextToggle : ValueContainer<BoolTimeContext>
    {
        [SerializeField] protected ToggleSwapInfoBase ToggleSwapInfo;

        protected override void SetValueWithoutNotify(BoolTimeContext value) => ToggleSwapInfo.SetViewActive(value);
    }
}