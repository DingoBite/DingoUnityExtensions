using DingoUnityExtensions.UnityViewProviders.Toggle.Core;
using UnityEngine;

namespace DingoUnityExtensions.UnityViewProviders.Toggle
{
    [RequireComponent(typeof(UnityEngine.UI.Toggle))]
    public class ToggleProvider : ToggleBehaviourProvider<UnityEngine.UI.Toggle>
    {
        private bool _value;
        public override bool Value => _value;
        protected override void OnSetInteractable(bool value)
        {
            View.interactable = value;
        }
        
        protected override void SubscribeOnly() => View.onValueChanged.AddListener(SetValueWithNotify);
        protected override void UnsubscribeOnly() => View.onValueChanged.RemoveListener(SetValueWithNotify);
        protected override void SetViewValueWithoutNotify(bool value)
        {
            _value = value;
            View.SetIsOnWithoutNotify(value);
        }
    }
}