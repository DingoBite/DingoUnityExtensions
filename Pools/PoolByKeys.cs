using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;

namespace _Project.Utils.MonoBehaviours
{
    public class PoolByKeys<TKey, T> : MonoBehaviour where T : Component
    {
        [SerializeField] private SerializedDictionary<TKey, T> _prefabs;
        [SerializeField] private bool _manageActiveness = true;

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
                return element;
            }
            element = InstantiateComponent(key);
            if (_manageActiveness)
                element.gameObject.SetActive(true);
            _pulledElements.Add((key, element));
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
            component.name = $"--{queue.Count}_{key}_{ComponentName}";
            return component;
        }
    }
}