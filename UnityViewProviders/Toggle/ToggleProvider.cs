using UnityEngine;

namespace DingoUnityExtensions.UnityViewProviders.Toggle
{
    [RequireComponent(typeof(UnityEngine.UI.Toggle))]
    public class ToggleProvider : ToggleBehaviourProvider<UnityEngine.UI.Toggle>
    {
        protected override void OnSetInteractable(bool value) => View.interactable = value;
        protected override void SubscribeOnly() => View.onValueChanged.AddListener(SetValueWithNotify);
        protected override void UnsubscribeOnly() => View.onValueChanged.RemoveListener(SetValueWithNotify);
        protected override void SetViewValueWithoutNotify(bool value) => View.SetIsOnWithoutNotify(value);
    }
}