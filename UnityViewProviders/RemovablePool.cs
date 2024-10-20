using System.Collections.Generic;
using System.Linq;
using DingoUnityExtensions.Extensions;
using DingoUnityExtensions.Pools;
using DingoUnityExtensions.UnityViewProviders.Core;
using DingoUnityExtensions.UnityViewProviders.Toggle.Core;
using UnityEngine;

namespace DingoUnityExtensions.UnityViewProviders
{
    public interface IRemovableContainer
    {
        public EventContainer DeleteButton { get; }
    }
    
    public abstract class RemovablePool<TContainer, TValue> : ValueContainer<List<TValue>> where TContainer : ValueContainer<TValue>, IRemovableContainer
    {
        [SerializeField] private Pool<TContainer> _pool;
        [SerializeField] private EventContainer _addButton;
        
        private readonly ButtonContainerGroup<int> _deleteButtons = new();
        
        protected override void OnAwake()
        {
            _deleteButtons.OnClick += DeleteButton;
            _addButton.SafeSubscribe(AddElement);
        }

        protected abstract TValue DefaultValueFactory();

        protected override void SetValueWithoutNotify(List<TValue> value)
        {
            _deleteButtons.Clear();
            if (value.Count != _pool.PulledElements.Count)
            {
                _pool.Clear();
                foreach (var altisJoint in value)
                {
                    var view = _pool.PullElement();
                    view.UpdateValueWithoutNotify(altisJoint);
                    view.SafeSubscribe(SomeValueChanged);
                }
            }
            
            _deleteButtons.Initialize(_pool.PulledElements.Select((e, i) => (i, e.DeleteButton)).ToList());
        }

        private void DeleteButton(int index)
        {
            if (Value == null)
                return;

            Value.RemoveAt(index);
            SetValueWithNotify(Value);
        }

        private void AddElement()
        {
            Value ??= new List<TValue>();
            Value.Add(DefaultValueFactory());
            SetValueWithNotify(Value);
        }

        private void SomeValueChanged(TValue _) => ValueChangeInvoke(_pool.PulledElements.Select(e => e.Value).ToList());
    }
}