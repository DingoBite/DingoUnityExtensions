using System;
using System.Collections.Generic;
using System.Linq;

namespace DingoUnityExtensions.UnityViewProviders.Core
{
    public class ValueContainerGroup<TId, TValue>
    {
        public event Action<TId, TValue> OnValueChange;
        
        private readonly Dictionary<TId, ValueContainer<TValue>> _eventContainers = new();
        private readonly Dictionary<TId, Action<TValue>> _eventById = new();

        public IEnumerable<ValueContainer<TValue>> GetContainers() => _eventContainers.Values;
        public ValueContainer<TValue> GetContainer(TId id) => _eventContainers.GetValueOrDefault(id, null);

        protected IReadOnlyDictionary<TId, ValueContainer<TValue>> EventContainers => _eventContainers;
        
        public void Initialize(IEnumerable<(TId, ValueContainer<TValue>)> eventContainers)
        {
            Clear();
            foreach (var (id, eventContainer) in eventContainers)
            {
                eventContainer.ValueChangeFromExternalSource = true;
                _eventContainers.Add(id, eventContainer);
            }

            SubscribeOnly();
        }
        
        public void Initialize(IEnumerable<KeyValuePair<TId, ValueContainer<TValue>>> eventContainers)
        {
            Clear();
            foreach (var (id, eventContainer) in eventContainers)
            {
                eventContainer.ValueChangeFromExternalSource = true;
                _eventContainers.Add(id, eventContainer);
            }

            SubscribeOnly();
        }
        
        public void Initialize(IReadOnlyDictionary<TId, ValueContainer<TValue>> eventContainers)
        {
            Clear();
            foreach (var (id, eventContainer) in eventContainers)
            {
                eventContainer.ValueChangeFromExternalSource = true;
                _eventContainers.Add(id, eventContainer);
            }

            SubscribeOnly();
        }
        
        public void Clear()
        {
            UnsubscribeOnly();
            _eventContainers.Clear();
            _eventById.Clear();
        }

        protected virtual void UpdateContainerValues(TId id, TValue value)
        {
            _eventContainers[id].UpdateValueWithoutNotify(value);
        }
        
        private void ValueChange(TId id, TValue value)
        {
            UpdateContainerValues(id, value);
            OnValueChange?.Invoke(id, value);
        }

        private void SubscribeOnly()
        {
            foreach (var (id, eventContainer) in _eventContainers)
            {
                if (!_eventById.TryGetValue(id, out var e))
                {
                    e = v => ValueChange(id, v);
                    _eventById[id] = e;
                }
                eventContainer.OnValueChange -= e;
                eventContainer.OnValueChange += e;
            }
        }

        private void UnsubscribeOnly()
        {
            foreach (var (id, eventContainer) in _eventContainers.Where(p => p.Value != null))
            {
                if (!_eventById.TryGetValue(id, out var e))
                {
                    continue;
                }
                eventContainer.OnValueChange -= e;
            }
        }
    }
}