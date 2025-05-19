using DingoUnityExtensions.UnityViewProviders.Core.Data;
using DingoUnityExtensions.UnityViewProviders.Toggle.Core;
using UnityEngine;
using UnityEngine.Events;

namespace DingoUnityExtensions.UnityViewProviders.Toggle
{
    public class EventToggleSwapInfo : ToggleSwapInfoBase
    {
        [SerializeField] private UnityEvent _enableEvent;
        [SerializeField] private UnityEvent _disableEvent;
        [SerializeField] private UnityEvent<bool> _toggleEvent;
        [SerializeField] private bool _invert;

        public override void SetViewActive(BoolTimeContext value)
        {
            var bValue = value.Bool();
            bValue = _invert ? !bValue : bValue;
            if (bValue)
                _enableEvent.Invoke();
            else 
                _disableEvent.Invoke();
            _toggleEvent.Invoke(bValue);
        }
    }
}