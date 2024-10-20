using DingoUnityExtensions.UnityViewProviders.Core;
using UnityEngine;

namespace DingoUnityExtensions.UnityViewProviders.Toggle
{
    [RequireComponent(typeof(EventContainer))]
    public class EventContainerAsToggleProvider : ToggleBehaviourProvider<EventContainer>
    {
        [SerializeField] private bool _lockInputDisable;
        [SerializeField] private bool _invert;
        
        protected override void OnSetInteractable(bool value) => View.Interactable = value;

        protected virtual void OnButtonClick()
        {
            if (_lockInputDisable)
                SetValueWithNotify(true);
            else
                SetValueWithNotify(!Value);
        }

        protected override void SubscribeOnly() => View.OnEvent += OnButtonClick;
        protected override void UnsubscribeOnly() => View.OnEvent -= OnButtonClick;
        protected override void SetViewValueWithoutNotify(bool value) => Value = _invert ? !value : value;
    }
}