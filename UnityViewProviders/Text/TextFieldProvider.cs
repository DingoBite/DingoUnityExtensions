using System;
using DingoUnityExtensions.UnityViewProviders.Core;
using TMPro;
using UnityEngine;

namespace DingoUnityExtensions.UnityViewProviders.Text
{
    [RequireComponent(typeof(TMP_InputField))]
    public class TextFieldProvider : UnityViewProvider<TMP_InputField, string>
    {
        [Flags]
        private enum TextFieldProviderEvent
        {
            None = 0,
            Change = 1 << 0,
            Submit = 1 << 1
        }

        [SerializeField] private TextFieldProviderEvent _invokeOn = TextFieldProviderEvent.Change;
        
        public event Action<string> OnChange;
        public event Action<string> OnSubmit;
        public event Action<string> OnCancel;

        private void ValueChange(string value)
        {
            Value = value;
            OnChange?.Invoke(value);
            if (_invokeOn.HasFlag(TextFieldProviderEvent.Change))
                SetValueWithNotify(value); 
        }
        
        private void OnSubmitValue(string value)
        {
            Value = value;
            OnSubmit?.Invoke(value);
            if (_invokeOn.HasFlag(TextFieldProviderEvent.Submit))
                SetValueWithNotify(value); 
        }
        
        private void OnDeselectValue(string value)
        {
            Value = value;
            OnCancel?.Invoke(value);
        }

        protected override void OnSetInteractable(bool value) => View.interactable = value;
        protected override void SetValueWithoutNotify(string value) => View.SetTextWithoutNotify(value);

        protected override void SubscribeOnly()
        {
            View.onValueChanged.AddListener(ValueChange);
            View.onSubmit.AddListener(OnSubmitValue);
            View.onDeselect.AddListener(OnDeselectValue);
        }

        protected override void UnsubscribeOnly()
        {
            View.onValueChanged.RemoveListener(ValueChange);
            View.onSubmit.RemoveListener(OnSubmitValue);
            View.onDeselect.RemoveListener(OnDeselectValue);
        }
    }
}