using System;
using System.Collections.Generic;
using UnityEngine;

namespace DingoUnityExtensions.UnityViewProviders.Core
{
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
        [SerializeField] private bool _isSelectable;

        private bool _selected;
        
        public sealed override bool Interactable
        {
            get => gameObject.activeInHierarchy && _isInteractable;
            set
            {
                _isInteractable = value;
                if (_blocker != null)
                    _blocker.gameObject.SetActive(!value);
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

        public sealed override Type ValueType => typeof(void);

        public event Action OnEvent;

        protected virtual void EventInvoke() => OnEvent?.Invoke();
        protected abstract void OnSetInteractable(bool value);
        protected virtual void OnSelected(bool value) { }

        protected virtual void Validate() {}
        
        private void OnValidate()
        {
            Interactable = _isInteractable;
            Validate();
        }
        
        public override void SetActiveContainer(bool value)
        {
            gameObject.SetActive(value);
        }
    }
}