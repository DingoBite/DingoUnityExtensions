using System;
using System.Collections.Generic;
using DingoUnityExtensions.Extensions;
using DingoUnityExtensions.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DingoUnityExtensions.Pools.Core
{
    public class Pool<T> : IPoolGetOnly<T>, IEnumerableContainer<T> where T : MonoBehaviour
    {
        private readonly GameObject _parent;
        private readonly T _prefab;
        private readonly bool _manageActiveness;
        private readonly SortTransformOrderOption _sortTransformOrder;
        private readonly bool _layerFromPool;

        private readonly List<T> _pulledElements = new();
        private readonly Queue<T> _queue = new();
        private readonly Action<T, bool> _setActiveOverwrite;

        private string ComponentName => typeof(T).Name;
        public IReadOnlyList<T> PulledElements => _pulledElements;
        public IEnumerable<T> ComponentElements => _pulledElements;

        public Pool(T prefab, GameObject parent, SortTransformOrderOption sortTransformOrder = SortTransformOrderOption.AsLast, bool layerFromPool = true, bool manageActiveness = true, Action<T, bool> setActiveOverwrite = null)
        {
            _setActiveOverwrite = setActiveOverwrite ?? ((behaviour, b) => behaviour.gameObject.SetActive(b));
            _manageActiveness = manageActiveness;
            _prefab = prefab;
            _sortTransformOrder = sortTransformOrder;
            _layerFromPool = layerFromPool;
            _parent = parent;
        }
        
        public T PullElement()
        {
            if (_queue.TryDequeue(out var element))
            {
                ManageActiveness(element, true);
                _pulledElements.Add(element);
                Sort(element);
                return element;
            }
            element = InstantiateComponent();
            ManageActiveness(element, true);
            _pulledElements.Add(element);
            Sort(element);
            return element;
        }

        private void ManageActiveness(T component, bool value)
        {
            if (!_manageActiveness)
                return;
            _setActiveOverwrite(component, value);
        }
        
        public void PushElement(T element)
        {
            ManageActiveness(element, false);
            _queue.Enqueue(element);
            _pulledElements.Remove(element);
        }

        public void Clear()
        {
            for (var i = PulledElements.Count - 1; i >= 0; i--)
            {
                var element = PulledElements[i];
                ManageActiveness(element, false);
                _queue.Enqueue(element);
            }
            _pulledElements.Clear();
        }
        
        private T InstantiateComponent()
        {
            var component = Object.Instantiate(_prefab, _parent.transform);
            if (_layerFromPool)
                component.gameObject.SetLayerRecursive(_parent.layer);
            component.name = $"--{_pulledElements.Count}_{ComponentName}";
            OnInstantiate(component);
            return component;
        }

        protected virtual void OnInstantiate(T component){}
        
        private void Sort(T element)
        {
            switch (_sortTransformOrder)
            {
                case SortTransformOrderOption.None:
                    break;
                case SortTransformOrderOption.AsLast:
                    element.transform.SetAsLastSibling();
                    break;
                case SortTransformOrderOption.AsFirst:
                    element.transform.SetAsFirstSibling();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}