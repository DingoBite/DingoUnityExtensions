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

        public override void SetViewActive(bool value)
        {
            if (value)
                _enableEvent.Invoke();
            else 
                _disableEvent.Invoke();
            _toggleEvent.Invoke(value);
        }
    }
}