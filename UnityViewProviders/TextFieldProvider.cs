using System;
using DingoUnityExtensions.UnityViewProviders.Core;
using TMPro;
using UnityEngine;

namespace DingoUnityExtensions.UnityViewProviders
{
    [RequireComponent(typeof(TMP_InputField))]
    public class TextFieldProvider : UnityViewProvider<TMP_InputField, string>
    {
        private string _value;
        public event Action<string> OnChange;
        public event Action<string> OnSubmit;
        public event Action<string> OnCancel;

        private void ValueChange(string value)
        {
            _value = value;
            OnChange?.Invoke(value);
            SetValueWithNotify(value); 
        }
        
        private void OnSubmitValue(string value)
        {
            _value = value;
            OnSubmit?.Invoke(value);
            SetValueWithNotify(value); 
        }
        
        private void OnDeselectValue(string value)
        {
            _value = value;
            OnCancel?.Invoke(value);
        }

        public override string Value => _value;
        protected override void OnSetInteractable(bool value)
        {
            View.interactable = value;
        }

        public override void SetValueWithoutNotify(string value) => View.SetTextWithoutNotify(value);

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