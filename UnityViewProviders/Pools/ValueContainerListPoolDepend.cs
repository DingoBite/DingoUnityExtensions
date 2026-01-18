using System.Collections.Generic;
using System.Linq;
using DingoUnityExtensions.Pools.Core;
using DingoUnityExtensions.UnityViewProviders.Core;
using UnityEngine;

namespace DingoUnityExtensions.UnityViewProviders.Pools
{
    public abstract class ValueContainerListPoolDepend<TRootValue, TValue, TValueContainer> : ValueContainer<TRootValue> where TValueContainer : ValueContainer<TValue>
    {
        [SerializeField] private GameObject _parent;
        [SerializeField] private TValueContainer _prefab;
        [SerializeField] private bool _fullRebuildOnChange;
        
        private Pool<TValueContainer> _pool;

        protected override void SetValueWithoutNotify(TRootValue value)
        {
            _pool ??= Factory(_prefab, _parent);
            if (value == null)
            {
                _pool.Clear();
                return;
            }
            
            var count = GetCount(value);
            if (count == 0)
            {
                _pool.Clear();
                return;
            }
            
            if (_fullRebuildOnChange)
                _pool.Clear();

            for (var i = 0; i < count; i++)
            {
                var subValue = GetValue(value, i);
                var valueContainer = i < _pool.PulledElements.Count ? _pool.PulledElements[i] : _pool.PullElement();
                SetValue(valueContainer, subValue);
            }

            if (count < _pool.PulledElements.Count)
            {
                var pulledElements = _pool.PulledElements.ToList();
                for (var i = count; i < pulledElements.Count; i++)
                {
                    _pool.PushElement(pulledElements[i]);
                }
            }
        }

        protected virtual void SetValue(TValueContainer valueContainer, TValue value) => valueContainer.UpdateValueWithoutNotify(value);

        protected abstract Pool<TValueContainer> Factory(TValueContainer prefab, GameObject parent);
        protected abstract int GetCount(TRootValue value);
        protected abstract TValue GetValue(TRootValue value, int index);
    }
}