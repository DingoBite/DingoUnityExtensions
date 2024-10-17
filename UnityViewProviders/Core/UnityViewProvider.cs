using System;
using DingoUnityExtensions.MonoBehaviours;
using NaughtyAttributes;
using UnityEngine;

namespace DingoUnityExtensions.UnityViewProviders.Core
{
    public abstract class ContainerBase : SubscribableBehaviour
    {
        protected override void SubscribeOnly() { }
        protected override void UnsubscribeOnly() { }
    }

    public abstract class ValueContainer<TValue> : ContainerBase
    {
        private const string FINALIZING_VIEW = "Finalizing View";

        [Tooltip("Can be null")]
        [SerializeField]
        private Transform _blocker;

        [SerializeField] private bool _isInteractable = true;
        // [Tooltip("Is view value managed from scripts or from input in place")]
        // [SerializeField] private bool _isExternalValueManager;
        [Foldout(FINALIZING_VIEW), Tooltip("Update view OnValidate from _debugDefaultValue")]
        [SerializeField] private bool _debugDefaultValueUpdate;
        [Foldout(FINALIZING_VIEW), ShowIf(nameof(_debugDefaultValueUpdate))]
        [SerializeField] private TValue _debugDefaultValue;
        [Foldout(FINALIZING_VIEW), Tooltip("Change view by value, that default on Not Interactable")]
        [SerializeField] private bool _placeholderAtNonInteractable;
        
        [HideInInspector]
        [SerializeField] private TValue _previousNotInteractablePlaceholder;

        protected virtual TValue NonInteractablePlaceholder => default;
        public bool ValueChangeFromExternalSource { get; set; }

        public bool Interactable
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

                OnSetInteractable(value);
            }
        }

        private void Awake()
        {
            if (_debugDefaultValueUpdate)
                UpdateValueWithoutNotify(_debugDefaultValue);
            OnAwake();
        }

        protected virtual void OnAwake() {}

        public event Action<TValue> OnValueChange;

        private bool _disabledValueChanged;
        private bool _updateOnEnable;
        
        public TValue Value { get; protected set; }

        protected void SetValueWithNotify(TValue value)
        {
            if (!ValueChangeFromExternalSource)
            {
                UpdateValueWithoutNotify(value);
                OnValueChange?.Invoke(Value);
            }
            else
            {
                OnValueChange?.Invoke(value);
            }
        }

        protected virtual void OnSetInteractable(bool value) { }

        public void UpdateValueWithoutNotify(TValue value)
        {
            if (!_updateOnEnable && (!gameObject.activeInHierarchy || !enabled))
            {
                Value = value;
                _disabledValueChanged = true;
                return;
            }
            if (_updateOnEnable && _disabledValueChanged)
            {
                _disabledValueChanged = false;
                _updateOnEnable = false;
            }
            Value = value;
            SetValueWithoutNotify(value);
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
        
        protected virtual void Validate() {}

        protected override void OnEnable()
        {
            base.OnEnable();
            if (!_disabledValueChanged)
                return;
            _updateOnEnable = true;
            UpdateValueWithoutNotify(Value);
        }
    }

    public abstract class EventContainer : SubscribableBehaviour
    {
        [Tooltip("Can be null")] 
        [SerializeField]
        private Transform _blocker;

        [SerializeField] private bool _isInteractable = true;

        public bool Interactable
        {
            get => gameObject.activeInHierarchy && _isInteractable;
            set
            {
                _isInteractable = value;
                if (_blocker != null)
                    _blocker.gameObject.SetActive(!value);
                OnSetInteractable(value);
            }
        }

        public event Action OnEvent;

        protected virtual void EventInvoke() => OnEvent?.Invoke();
        protected abstract void OnSetInteractable(bool value);

        private void OnValidate()
        {
            Interactable = _isInteractable;
        }
    }

    public abstract class UnityViewProvider<TView, TValue> : ValueContainer<TValue> where TView : Component
    {
        private TView _view;

        protected TView View
        {
            get
            {
                if (_view == null)
                    _view = GetComponent<TView>();
                return _view;
            }
        }

        protected abstract override void SubscribeOnly();
        protected abstract override void UnsubscribeOnly();
    }

    public abstract class UnityViewProvider<TView> : EventContainer where TView : MonoBehaviour
    {
        private TView _view;

        protected TView View
        {
            get
            {
                if (_view == null)
                    _view = GetComponent<TView>();
                return _view;
            }
        }

        protected abstract override void SubscribeOnly();
        protected abstract override void UnsubscribeOnly();
    }
}