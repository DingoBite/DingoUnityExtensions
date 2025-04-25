using System;
using System.Collections.Generic;
using DingoUnityExtensions.Extensions;
using DingoUnityExtensions.Generic;
using UnityEngine;

namespace DingoUnityExtensions.Pools.Core
{
    public class PoolBehaviour<T> : MonoBehaviour, IPoolGetOnly<T>, IEnumerableContainer<T> where T : MonoBehaviour
    {
        [SerializeField] private T _prefab;
        [SerializeField] private bool _manageActiveness = true;
        [SerializeField] private SortTransformOrderOption _sortTransformOrder;
        [SerializeField] private bool _layerFromPool = true;
        
        private readonly List<T> _pulledElements = new();
        private readonly Queue<T> _queue = new();
        private string ComponentName => typeof(T).Name;
        public IReadOnlyList<T> PulledElements => _pulledElements;
        public IEnumerable<T> ComponentElements => _pulledElements;

        public T PullElement()
        {
            if (_queue.TryDequeue(out var element))
            {
                if (_manageActiveness)
                    element.gameObject.SetActive(true);
                _pulledElements.Add(element);
                Sort(element);
                return element;
            }
            element = InstantiateComponent();
            if (_manageActiveness)
                element.gameObject.SetActive(true);
            _pulledElements.Add(element);
            Sort(element);
            return element;
        }

        public void PushElement(T element)
        {
            if (_manageActiveness)
                element.gameObject.SetActive(false);
            _queue.Enqueue(element);
            _pulledElements.Remove(element);
        }

        public void Clear()
        {
            for (var i = PulledElements.Count - 1; i >= 0; i--)
            {
                var element = PulledElements[i];
                PushElement(element);
            }
        }
        
        private T InstantiateComponent()
        {
            var component = Instantiate(_prefab, transform);
            if (_layerFromPool)
                component.gameObject.SetLayerRecursive(gameObject.layer);
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