using System;
using System.Collections.Generic;
using System.Linq;
using DingoUnityExtensions.UnityViewProviders.Core;

namespace DingoUnityExtensions.UnityViewProviders.Toggle.Core
{
    public class ValueContainerGroup<TId, TValue>
    {
        public event Action<TId, TValue> OnValueChange;
        
        private readonly Dictionary<TId, ValueContainer<TValue>> _eventContainers = new();
        private readonly Dictionary<TId, Action<TValue>> _eventById = new();

        public IEnumerable<ValueContainer<TValue>> GetButtons() => _eventContainers.Values;
        public ValueContainer<TValue> GetButton(TId id) => _eventContainers.GetValueOrDefault(id, null);
        
        public void Initialize(IEnumerable<(TId, ValueContainer<TValue>)> eventContainers)
        {
            Clear();
            foreach (var (id, eventContainer) in eventContainers)
            {
                _eventContainers.Add(id, eventContainer);
            }

            SubscribeOnly();
        }
        
        public void Initialize(IEnumerable<KeyValuePair<TId, ValueContainer<TValue>>> eventContainers)
        {
            Clear();
            foreach (var (id, eventContainer) in eventContainers)
            {
                _eventContainers.Add(id, eventContainer);
            }

            SubscribeOnly();
        }
        
        public void Initialize(IReadOnlyDictionary<TId, ValueContainer<TValue>> eventContainers)
        {
            Clear();
            foreach (var (id, eventContainer) in eventContainers)
            {
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
        
        private void ValueChange(TId id, TValue value)
        {
            OnValueChange?.Invoke(id, value);
        }
        
        protected void SubscribeOnly()
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

        protected void UnsubscribeOnly()
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