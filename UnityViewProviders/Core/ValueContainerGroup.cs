using System;
using System.Collections.Generic;
using System.Linq;

namespace DingoUnityExtensions.UnityViewProviders.Core
{
    public class ValueContainerGroup<TId, TValue>
    {
        public event Action<TId, TValue> OnValueChange;
        
        private readonly Dictionary<TId, ValueContainer<TValue>> _containers = new();
        private readonly Dictionary<TId, Action<TValue>> _eventById = new();

        public IEnumerable<ValueContainer<TValue>> GetContainers() => _containers.Values;
        public ValueContainer<TValue> GetContainer(TId id) => _containers.GetValueOrDefault(id, null);

        protected IReadOnlyDictionary<TId, ValueContainer<TValue>> Containers => _containers;

        public void Initialize<T>(IEnumerable<(TId, T)> eventContainers) where T : ValueContainer<TValue>
        {
            Clear();
            foreach (var (id, eventContainer) in eventContainers)
            {
                eventContainer.ValueChangeFromExternalSource = true;
                _containers.Add(id, eventContainer);
            }

            SubscribeOnly();
        }

        public void Initialize<T>(IEnumerable<(T, TId)> eventContainers) where T : ValueContainer<TValue>
        {
            Initialize(eventContainers.Select(p => (p.Item2, p.Item1)));
        }

        public void Initialize<T>(IEnumerable<KeyValuePair<T, TId>> eventContainers) where T : ValueContainer<TValue>
        {
            Initialize(eventContainers.Select(p => (p.Value, p.Key)));
        }

        public void Initialize<T>(IEnumerable<KeyValuePair<TId, T>> eventContainers) where T : ValueContainer<TValue>
        {
            Initialize(eventContainers.Select(p => (p.Key, p.Value)));
        }
        
        public void Initialize<T>(IReadOnlyDictionary<TId, T> eventContainers) where T : ValueContainer<TValue>
        {
            Initialize(eventContainers.Select(p => (p.Key, p.Value)));
        }
        
        public void Clear()
        {
            UnsubscribeOnly();
            _containers.Clear();
            _eventById.Clear();
        }

        protected virtual void UpdateContainerValues(TId id, TValue value)
        {
            _containers[id].UpdateValueWithoutNotify(value);
        }
        
        private void ValueChange(TId id, TValue value)
        {
            UpdateContainerValues(id, value);
            OnValueChange?.Invoke(id, value);
        }

        private void SubscribeOnly()
        {
            foreach (var (id, eventContainer) in _containers)
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
            foreach (var (id, eventContainer) in _containers.Where(p => p.Value != null))
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