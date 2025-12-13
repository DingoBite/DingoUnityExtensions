using System;
using DingoUnityExtensions.UnityViewProviders.Core;
using DingoUnityExtensions.UnityViewProviders.Core.Data;
using DingoUnityExtensions.UnityViewProviders.Navigation;
using DingoUnityExtensions.UnityViewProviders.Toggle.Core;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DingoUnityExtensions.UnityViewProviders.Text
{
    public class TextFieldProvider : UnityViewProvider<TMP_InputField, string>, INavigationMoveFilter
    {
        [Flags]
        private enum TextFieldProviderEvent
        {
            None = 0,
            Change = 1 << 0,
            Submit = 1 << 1
        }

        [SerializeField] private TextFieldProviderEvent _invokeOn = TextFieldProviderEvent.Change;
        [SerializeField] private ToggleSwapInfoBase _selectToggle;
        [SerializeField] private bool _selectImmediately;
        [SerializeField] private ContainerNavigationNode _navigationNode;
        [SerializeField] private MoveDirection _onDeselectNavigate;
        [SerializeField] private MoveDirection _onSubmitNavigate;
        
        private bool _wasCanceled;

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

            if (_navigationNode != null)
                _navigationNode.ForceNavigationMove(_onSubmitNavigate);
        }
        
        private void OnDeselectValue(string value)
        {
            Value = value;
            OnCancel?.Invoke(value);
        }

        protected override void OnSetInteractable(bool value) => View.interactable = value;
        protected override void SetValueWithoutNotify(string value) => View.SetTextWithoutNotify(value);

        protected override void OnSelected(bool value)
        {
            if (value)
                _wasCanceled = false;
            if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != gameObject && value)
                EventSystem.current.SetSelectedGameObject(gameObject);
            _selectToggle.SetViewActive(value.TimeContext(_selectImmediately));
        }

        private void OnDeselect(string text)
        {
            if (!Interactable)
                return;
            
            Selected = false;
        }

        private void OnSelect(string text)
        {
            if (!Interactable)
                return;
            Selected = true;
        }

        private void OnEndEdit(string text)
        {
            if (!Interactable)
                return;
            if (View.wasCanceled)
            {
                _wasCanceled = true;
                _navigationNode.ForceNavigationMove(_onDeselectNavigate);
            }
        }

        public bool AllowNavigationMove(AxisEventData eventData) => !View.isFocused;

        private EscapeResult Escape()
        {
            if (_wasCanceled)
            {
                _wasCanceled = false;
                return EscapeResult.Block;
            }
            return EscapeResult.None;
        }

        protected override void SubscribeOnly()
        {
            _wasCanceled = false;
            View.onValueChanged.AddListener(ValueChange);
            View.onSubmit.AddListener(OnSubmitValue);
            View.onDeselect.AddListener(OnDeselectValue);
            View.onSelect.AddListener(OnSelect);
            View.onDeselect.AddListener(OnDeselect);
            View.onEndEdit.AddListener(OnEndEdit);
            EscapeRouter.AddAction(Escape);
        }

        protected override void UnsubscribeOnly()
        {
            View.onValueChanged.RemoveListener(ValueChange);
            View.onSubmit.RemoveListener(OnSubmitValue);
            View.onDeselect.RemoveListener(OnDeselectValue);
            View.onSelect.RemoveListener(OnSelect);
            View.onDeselect.RemoveListener(OnDeselect);
            View.onEndEdit.RemoveListener(OnEndEdit);
            EscapeRouter.RemoveAction(Escape);
        }
    }
}