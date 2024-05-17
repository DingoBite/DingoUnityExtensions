using DingoUnityExtensions.UnityViewProviders.Core;

namespace DingoUnityExtensions.UnityViewProviders.Toggle
{
    public abstract class ViewValueContainer<TViewValue, TValue> : UnityViewProvider<ValueContainer<TViewValue>, TValue>
    {
        private TValue _value;

        public override TValue Value => _value;

        protected abstract TViewValue Convert(TValue value); 
        
        public sealed override void SetValueWithoutNotify(TValue value)
        {
            _value = value;
            View.SetValueWithoutNotify(Convert(value));
        }

        protected override void OnSetInteractable(bool value)
        {
            base.OnSetInteractable(value);
            View.Interactable = Interactable;
        }

        protected override void SubscribeOnly()
        {
        }

        protected override void UnsubscribeOnly()
        {
        }
    }
}