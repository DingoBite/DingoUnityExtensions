using DingoUnityExtensions.UnityViewProviders.Core;
using DingoUnityExtensions.UnityViewProviders.Core.Data;
using DingoUnityExtensions.UnityViewProviders.Toggle.Core;
using UnityEngine;

namespace DingoUnityExtensions.UnityViewProviders.Toggle
{
    public class BoolTimeContextContainer : ValueContainer<BoolTimeContext>
    {
        [SerializeField] private ToggleSwapInfoBase _toggleSwapInfo;
        [SerializeField] private ValueContainer<bool> _toggle;

        protected override void SetValueWithoutNotify(BoolTimeContext value)
        {
            var boolValue = value.Bool();
            if (_toggle != null)
                _toggle.UpdateValueWithoutNotify(boolValue);
            _toggleSwapInfo.SetViewActive(value);
        }
    }
}