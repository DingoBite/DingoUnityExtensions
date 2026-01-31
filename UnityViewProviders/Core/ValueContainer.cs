using System;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

namespace DingoUnityExtensions.UnityViewProviders.Core
{
    public abstract class ValueContainer<TValue> : ContainerBase
    {
        public abstract class List : ValueContainer<TValue>
        {
            [SerializeField] private List<ValueContainer<TValue>> _stack = new();
            [SerializeField] private bool _manageActiveness;

            public override bool ValueChangeFromExternalSource
            {
                get => base.ValueChangeFromExternalSource;
                set
                {
                    foreach (var valueContainer in _stack)
                    {
                        valueContainer.ValueChangeFromExternalSource = value;
                    }

                    base.ValueChangeFromExternalSource = value;
                }
            }

            public override bool ValueChangeNoDependViewState
            {
                get => base.ValueChangeNoDependViewState;
                set
                {
                    foreach (var valueContainer in _stack)
                    {
                        valueContainer.ValueChangeNoDependViewState = value;
                    }

                    base.ValueChangeNoDependViewState = value;
                }
            }

            protected override void SetValueWithoutNotify(TValue value)
            {
                foreach (var container in _stack)
                {
                    container.UpdateValueWithoutNotify(value);
                }
            }

            protected override void Validate()
            {
                foreach (var container in _stack)
                {
                    container.Validate();
                }
            }

            protected override void OnSetInteractable(bool value)
            {
                foreach (var container in _stack)
                {
                    container.Interactable = value;
                }
            }

            protected override void OnSelected(bool value)
            {
                foreach (var container in _stack)
                {
                    container.Selected = value;
                }
            }

            protected override void SubscribeOnly()
            {
                foreach (var container in _stack)
                {
                    container.OnValueChange += ValueChangeInvoke;
                }
            }

            protected override void UnsubscribeOnly()
            {
                foreach (var container in _stack)
                {
                    container.OnValueChange -= ValueChangeInvoke;
                }
            }

            protected override void OnEnable()
            {
                base.OnEnable();
                if (!_manageActiveness)
                    return;
                foreach (var container in _stack)
                {
                    if (container.gameObject == gameObject)
                        continue;
                    container.SetActiveContainer(true);
                }
            }

            protected override void OnDisable()
            {
                base.OnDisable();
                if (!_manageActiveness)
                    return;
                foreach (var container in _stack)
                {
                    if (container != null && container.gameObject == gameObject)
                        continue;
                    try
                    {
                        container.SetActiveContainer(false);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
            }

            public override void SetActiveContainer(bool value)
            {
                if (!_manageActiveness)
                {
                    base.SetActiveContainer(value);
                    return;
                }

                foreach (var container in _stack)
                {
                    container.SetActiveContainer(value);
                }
            }
        }

        private const string FINALIZING_VIEW = "Finalizing View";

        [SerializeField] private bool _isInteractable = true;
        [SerializeField] private bool _isSelectable;

        [Tooltip("Can be null")] [Foldout(FINALIZING_VIEW)] [SerializeField]
        private Transform _blocker;

        [Foldout(FINALIZING_VIEW)] [SerializeField]
        private bool _debugDefaultValueUpdate;

        [Foldout(FINALIZING_VIEW), ShowIf(nameof(_debugDefaultValueUpdate))] [SerializeField]
        private TValue _debugDefaultValue;

        [Foldout(FINALIZING_VIEW)] [SerializeField]
        private bool _placeholderAtNonInteractable;

        [HideInInspector] [SerializeField] private TValue _previousNotInteractablePlaceholder;

        private bool _selected;
        private bool _awaked;

        private bool _disabledValueChanged;
        private bool _updateOnEnable;
        private bool _valueChangeFromExternalSource;
        private bool _valueChangeNoDependViewState;

        protected virtual TValue NonInteractablePlaceholder => default;

        public virtual bool ValueChangeFromExternalSource { get => _valueChangeFromExternalSource; set => _valueChangeFromExternalSource = value; }

        public virtual bool ValueChangeNoDependViewState { get => _valueChangeNoDependViewState; set => _valueChangeNoDependViewState = value; }

        public sealed override bool Interactable
        {
            get => gameObject.activeInHierarchy && _isInteractable;
            set
            {
                _isInteractable = value;
                if (_blocker != null)
                    _blocker.gameObject.SetActive(!value);
                if (_placeholderAtNonInteractable)
                {
                    if (!value)
                    {
                        _previousNotInteractablePlaceholder = Value;
                        SetValueWithoutNotify(NonInteractablePlaceholder);
                    }
                    else if (Value != null && Value.Equals(NonInteractablePlaceholder))
                    {
                        SetValueWithoutNotify(_previousNotInteractablePlaceholder);
                    }
                }

                if (!_isInteractable)
                {
                    _selected = false;
                    OnSelected(false);
                }

                OnSetInteractable(value);
            }
        }

        public sealed override bool Selectable => _isSelectable;

        public sealed override bool Selected
        {
            get => _selected && _isSelectable;
            set
            {
                if (!_isSelectable || _selected == value || !_isInteractable)
                    return;
                _selected = value;
                OnSelected(_selected);
            }
        }

        public sealed override Type ValueType => typeof(TValue);

        private void Awake()
        {
            if (_awaked)
                return;
            _awaked = true;
            OnAwake();
        }

        protected virtual void OnAwake() { }

        public event Action<TValue> OnValueChange;
        public event Action OnAnyChange;

        public TValue Value { get; protected set; }

        protected virtual void PreviousValueFree(TValue previousData) { }

        protected void SetValueWithNotify(TValue value)
        {
            if (!_awaked)
                Awake();

            if (!ValueChangeFromExternalSource)
            {
                UpdateValueWithoutNotify(value);
                ValueChangeInvoke(Value);
            }
            else
            {
                ValueChangeInvoke(value);
            }
        }

        protected void ValueChangeInvoke(TValue value)
        {
            if (!_awaked)
                Awake();

            OnValueChange?.Invoke(value);
            OnAnyChange?.Invoke();
        }

        protected void ForceSetValue(TValue value)
        {
            try
            {
                PreviousValueFree(Value);
            }
            catch (Exception e)
            {
                Debug.LogException(e, this);
            }

            Value = value;
            try
            {
                SetValueWithoutNotify(value);
            }
            catch (Exception e)
            {
                Debug.LogException(e, this);
            }
        }

        protected virtual void OnSetInteractable(bool value) { }
        protected virtual void OnSelected(bool value) { }

        public sealed override void UpdateBoxedValueWithoutNotify(object value)
        {
            if (value is TValue obj)
                UpdateValueWithoutNotify(obj);
            else if (value is null)
                UpdateValueWithoutNotify(default);
        }

        public void UpdateValueWithoutNotify(TValue value)
        {
            if (!_awaked)
                Awake();

            var viewUnavailable = !gameObject.activeInHierarchy || !enabled;

            if (!ValueChangeNoDependViewState && !_updateOnEnable && viewUnavailable)
            {
                try
                {
                    PreviousValueFree(Value);
                }
                catch (Exception e)
                {
                    Debug.LogException(e, this);
                }

                Value = value;
                _disabledValueChanged = true;
                return;
            }

            if (_updateOnEnable)
                _updateOnEnable = false;

            if (ValueChangeNoDependViewState)
                _disabledValueChanged = false;

            try
            {
                PreviousValueFree(Value);
            }
            catch (Exception e)
            {
                Debug.LogException(e, this);
            }

            Value = value;

            try
            {
                SetValueWithoutNotify(value);
            }
            catch (Exception e)
            {
                Debug.LogException(e, this);
            }
        }

        protected abstract void SetValueWithoutNotify(TValue value);

        private void OnValidate()
        {
            Validate();
            Interactable = _isInteractable;
            if (!_debugDefaultValueUpdate)
                return;

            try
            {
                SetValueWithoutNotify(_debugDefaultValue);
            }
            catch (Exception e)
            {
                Debug.LogException(e, this);
            }
        }

        protected virtual void Validate() { }

        protected override void OnEnable()
        {
            base.OnEnable();

            if (ValueChangeNoDependViewState)
            {
                _disabledValueChanged = false;
                _updateOnEnable = false;
                return;
            }

            if (!_disabledValueChanged)
                return;

            _updateOnEnable = true;
            UpdateValueWithoutNotify(Value);
        }

        public override void SetActiveContainer(bool value)
        {
            gameObject.SetActive(value);
        }
    }
}