using DingoUnityExtensions.Extensions;
using DingoUnityExtensions.UnityViewProviders.Core;
using UnityEngine;

namespace DingoUnityExtensions.MonoBehaviours.UI.ToggleDropdownLayoutBased
{
    public class DropdownToggleContainer : ValueContainer<bool>
    {
        [SerializeField] private ValueContainer<bool> _toggle;
        [field: SerializeField] public ValueContainer<string> Title { get; set; }

        protected override void SetValueWithoutNotify(bool value) => _toggle.UpdateValueWithoutNotify(value);

        protected override void SubscribeOnly() => _toggle.SafeSubscribe(SetValueWithNotify);
        protected override void UnsubscribeOnly() => _toggle.UnSubscribe(SetValueWithNotify);
    }
}