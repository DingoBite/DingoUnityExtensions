using DingoUnityExtensions.UnityViewProviders.Core;

namespace DingoUnityExtensions.UnityViewProviders.Toggle
{
    public abstract class ViewValueContainer<TViewValue, TValue> : UnityViewProvider<ValueContainer<TViewValue>, TValue>
    {
        protected abstract TViewValue Convert(TValue value);

        protected sealed override void SetValueWithoutNotify(TValue value) => View.UpdateValueWithoutNotify(Convert(value));

        protected override void OnSetInteractable(bool value)
        {
            base.OnSetInteractable(value);
            View.Interactable = Interactable;
        }
    }
}