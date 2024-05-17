using DingoUnityExtensions.UnityViewProviders.Core;
using UnityEngine;
using UnityEngine.UI;

namespace DingoUnityExtensions.UnityViewProviders
{
    [RequireComponent(typeof(Button))]
    public class ButtonProvider : UnityViewProvider<Button>
    {
        protected override void SubscribeOnly() => View.onClick.AddListener(EventInvoke);
        protected override void UnsubscribeOnly() => View.onClick.RemoveListener(EventInvoke);
        protected override void OnSetInteractable(bool value)
        {
            View.interactable = value;
        }
    }
}