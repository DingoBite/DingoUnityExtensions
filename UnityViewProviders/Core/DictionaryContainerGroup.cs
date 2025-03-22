using System;
using System.Collections.Generic;

namespace DingoUnityExtensions.UnityViewProviders.Core
{
    public class DictionaryContainerGroup<TId, TValue> : ValueContainerGroup<TId, TValue>
    {
        public event Action<IReadOnlyDictionary<TId, TValue>> OnDictValueChange;

        private readonly Dictionary<TId, TValue> _values = new();

        public IReadOnlyDictionary<TId, TValue> Values => _values;
        
        public void UpdateWithoutNotify(IReadOnlyDictionary<TId, TValue> dictionary)
        {
            foreach (var (key, value) in dictionary)
            {
                _values[key] = value;
            }
            foreach (var (key, container) in Containers)
            {
                container.UpdateValueWithoutNotify(_values[key]);
            }
        }
        
        protected override void UpdateContainerValues(TId id, TValue value)
        {
            UpdateContainerValuesWithoutNotify(id, value);
            OnDictValueChange?.Invoke(_values);
        }
        
        public void UpdateContainerValuesWithoutNotify(TId id, TValue value)
        {
            foreach (var (key, container) in Containers)
            {
                if (key.Equals(id))
                    container.UpdateValueWithoutNotify(value);
                _values[key] = container.Value;
            }
        }
    }
}