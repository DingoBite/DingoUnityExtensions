using DingoUnityExtensions.UnityViewProviders.Core;
using DingoUnityExtensions.UnityViewProviders.Core.Data;
using DingoUnityExtensions.UnityViewProviders.Toggle.Core;
using UnityEngine;

namespace DingoUnityExtensions.UnityViewProviders.Toggle
{
    public class PassiveToggle : ValueContainer<bool>
    {
        [SerializeField] protected ToggleSwapInfoBase ToggleSwapInfo;
        [SerializeField] private bool _isImmediately;
        
        protected override void SetValueWithoutNotify(bool value)
        {
            ToggleSwapInfo.SetViewActive(value.TimeContext(_isImmediately));
        }
    }
}