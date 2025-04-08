using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using DingoUnityExtensions.Extensions;
using UnityEngine;

namespace DingoUnityExtensions.Pools.Core
{
    public class PoolByKeysBehaviour<TKey, T> : MonoBehaviour where T : Component
    {
        [SerializeField] private SerializedDictionary<TKey, T> _prefabs;
        [SerializeField] private bool _manageActiveness = true;
        [SerializeField] private SortTransformOrderOption _sortTransformOrder;
        [SerializeField] private bool _layerFromPool = true;
        
        private readonly List<(TKey, T)> _pulledElements = new();

        private readonly Dictionary<TKey, Queue<T>> _queues = new();
        private string ComponentName => typeof(T).Name;
        
        public IReadOnlyList<(TKey, T)> PulledElements => _pulledElements;

        public bool TryPoolElement(TKey key, out T element)
        {
            element = default;
            if (!_prefabs.TryGetValue(key, out var prefab))
                return false;
            element = PullElement(key);
            return true;
        }
        
        public T PullElement(TKey key)
        {
            if (!_queues.TryGetValue(key, out var queue))
            {
                queue = new Queue<T>();
                _queues.Add(key, queue);
            }
            
            if (queue.TryDequeue(out var element))
            {
                if (_manageActiveness)
                    element.gameObject.SetActive(true);
                _pulledElements.Add((key, element));
                Sort(element);
                return element;
            }
            element = InstantiateComponent(key);
            if (_manageActiveness)
                element.gameObject.SetActive(true);
            _pulledElements.Add((key, element));
            Sort(element);
            return element;
        }

        public void PushElement(T element, TKey key)
        {
            if (!_queues.TryGetValue(key, out var queue))
            {
                queue = new Queue<T>();
                _queues.Add(key, queue);
            }
            if (_manageActiveness)
                element.gameObject.SetActive(false);
            queue.Enqueue(element);
            _pulledElements.Remove((key, element));
        }
        
        public void Clear()
        {
            for (var i = 0; i < PulledElements.Count; i++)
            {
                var (key, element) = PulledElements[i];
                PushElement(element, key);
            }
        }
        
        private T InstantiateComponent(TKey key)
        {
            var prefab = _prefabs[key];
            var queue = _queues[key];
            var component = Instantiate(prefab, transform);
            if (_layerFromPool)
                component.gameObject.SetLayerRecursive(gameObject.layer);
            component.name = $"--{queue.Count}_{key}_{ComponentName}";
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