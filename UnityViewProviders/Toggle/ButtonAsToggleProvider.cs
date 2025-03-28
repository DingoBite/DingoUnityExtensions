using UnityEngine;
using UnityEngine.UI;

namespace DingoUnityExtensions.UnityViewProviders.Toggle
{
    [RequireComponent(typeof(Button))]
    public class ButtonAsToggleProvider : ToggleBehaviourProvider<Button>
    {
        [SerializeField] private bool _lockInputDisable;
        [SerializeField] private bool _invert;
        
        protected override void OnSetInteractable(bool value) => View.interactable = value;

        protected virtual void OnButtonClick()
        {
            if (_lockInputDisable)
                SetValueWithNotify(true);
            else
                SetValueWithNotify(!Value);
        }

        protected override void SubscribeOnly() => View.onClick.AddListener(OnButtonClick);
        protected override void UnsubscribeOnly() => View.onClick.RemoveListener(OnButtonClick);
        protected override void SetViewValueWithoutNotify(bool value) => Value = _invert ? !value : value;
    }
}