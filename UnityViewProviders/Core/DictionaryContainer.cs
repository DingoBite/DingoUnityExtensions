using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;

namespace DingoUnityExtensions.UnityViewProviders.Core
{
    public abstract class DictionaryContainer<TId, TValue> : ValueContainer<IReadOnlyDictionary<TId, TValue>>
    {
        public event Action<IReadOnlyDictionary<TId, TValue>> OnValueChange
        {
            add => _dictionaryContainerGroup.OnValueChange += value;
            remove => _dictionaryContainerGroup.OnValueChange -= value;
        } 
        
        [SerializeField] private SerializedDictionary<TId, ValueContainer<TValue>> _checkboxes;
        
        private readonly DictionaryContainerGroup<TId, TValue> _dictionaryContainerGroup = new();
        
        private bool _initialized;

        protected override void SetValueWithoutNotify(IReadOnlyDictionary<TId, TValue> value)
        {
            if (!_initialized)
                Initialize();
            _dictionaryContainerGroup.UpdateWithoutNotify(value);
        }

        protected override void OnAwake() => Initialize();

        private void Initialize()
        {
            if (_initialized)
                return;

            _dictionaryContainerGroup.Initialize(_checkboxes);
            _initialized = true;
        }
    }
}