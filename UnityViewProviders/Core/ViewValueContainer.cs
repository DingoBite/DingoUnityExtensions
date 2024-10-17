namespace DingoUnityExtensions.UnityViewProviders.Core
{
    public abstract class ViewValueContainer<TViewValue, TValue> : UnityViewProvider<ValueContainer<TViewValue>, TValue>
    {
        protected abstract TViewValue Convert(TValue value);
        protected abstract TValue Convert(TViewValue value);

        protected sealed override void SetValueWithoutNotify(TValue value) => View.UpdateValueWithoutNotify(Convert(value));

        protected void SetViewValueWithNotify(TViewValue viewValue) => SetValueWithNotify(Convert(viewValue));

        protected override void OnSetInteractable(bool value)
        {
            base.OnSetInteractable(value);
            View.Interactable = value;
        }
    }
}