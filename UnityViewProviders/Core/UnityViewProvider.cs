using System;
using System.Collections.Generic;
using DingoUnityExtensions.MonoBehaviours;
using NaughtyAttributes;
using UnityEngine;

namespace DingoUnityExtensions.UnityViewProviders.Core
{
    public abstract class ContainerBase : SubscribableBehaviour
    {
        public virtual void SetDefaultView() {}
        protected override void SubscribeOnly() { }
        protected override void UnsubscribeOnly() { }
        public virtual bool Interactable { get; set; }
    }

    public abstract class ValueContainer<TValue> : ContainerBase
    {
        public abstract class List : ValueContainer<TValue>
        {
            [SerializeField] private List<ValueContainer<TValue>> _stack = new ();
            [SerializeField] private bool _manageActiveness;

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

        private bool _awaked;
        
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

                OnSetInteractable(value);
            }
        }

        private void Awake()
        {
            if (_awaked)
                return;
            _awaked = true;
            OnAwake();
        }

        protected virtual void OnAwake() {}

        public event Action<TValue> OnValueChange;
        public event Action OnAnyChange;

        private bool _disabledValueChanged;
        private bool _updateOnEnable;
        
        public TValue Value { get; protected set; }

        protected virtual void PreviousValueFree(TValue previousData){}

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

        public void UpdateValueWithoutNotify(TValue value)
        {
            if (!_awaked)
                Awake();
            
            if (!_updateOnEnable && (!gameObject.activeInHierarchy || !enabled))
            {
                PreviousValueFree(Value);
                Value = value;
                _disabledValueChanged = true;
                return;
            }
            if (_updateOnEnable && _disabledValueChanged)
            {
                _disabledValueChanged = false;
                _updateOnEnable = false;
            }
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
        
        protected virtual void Validate() {}

        protected override void OnEnable()
        {
            base.OnEnable();
            if (!_disabledValueChanged)
                return;
            _updateOnEnable = true;
            UpdateValueWithoutNotify(Value);
        }
        
        public virtual void SetActiveContainer(bool value)
        {
            gameObject.SetActive(value);
        }
    }

    public abstract class EventContainer : ContainerBase
    {
        public class List : EventContainer
        {
            [SerializeField] private List<EventContainer> _stack = new();
            [SerializeField] private bool _manageActiveness;

            protected override void OnSetInteractable(bool value)
            {
                foreach (var container in _stack)
                {
                    container.Interactable = value;
                }
            }

            protected override void SubscribeOnly()
            {
                foreach (var container in _stack)
                {
                    container.OnEvent += EventInvoke;
                }
            }

            protected override void UnsubscribeOnly()
            {
                foreach (var container in _stack)
                {
                    container.OnEvent -= EventInvoke;
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
                    if (container.gameObject == gameObject)
                        continue;
                    container.SetActiveContainer(false);
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
        
        [Tooltip("Can be null")] 
        [SerializeField]
        private Transform _blocker;

        [SerializeField] private bool _isInteractable = true;

        public sealed override bool Interactable
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

        protected virtual void Validate() {}
        
        private void OnValidate()
        {
            Interactable = _isInteractable;
            Validate();
        }
        
        public virtual void SetActiveContainer(bool value)
        {
            gameObject.SetActive(value);
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