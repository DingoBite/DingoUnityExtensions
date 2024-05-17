using UnityEngine;

namespace DingoUnityExtensions.UnityViewProviders.Toggle
{
    public abstract class ToggleWithValueProvider<TViewValue, TValue> : ButtonAsToggleProvider
    {
        [field: SerializeField] public ViewValueContainer<TViewValue, TValue> ViewValueContainer { get; private set; }

        protected override void OnSetInteractable(bool value)
        {
            base.OnSetInteractable(value);
            ViewValueContainer.Interactable = value;
        }
    }
}