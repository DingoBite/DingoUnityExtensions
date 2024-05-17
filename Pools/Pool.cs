using System.Collections.Generic;
using UnityEngine;

namespace _Project.Utils.MonoBehaviours
{
    public class Pool<T> : MonoBehaviour where T : MonoBehaviour
    {
        [SerializeField] private T _prefab;
        [SerializeField] private bool _manageActiveness = true;

        private readonly List<T> _pulledElements = new List<T>();
        private readonly Queue<T> _queue = new();
        private string ComponentName => typeof(T).Name;
        public IReadOnlyList<T> PulledElements => _pulledElements;
        
        public T PullElement()
        {
            if (_queue.TryDequeue(out var element))
            {
                if (_manageActiveness)
                    element.gameObject.SetActive(true);
                _pulledElements.Add(element);
                return element;
            }
            element = InstantiateComponent();
            if (_manageActiveness)
                element.gameObject.SetActive(true);
            _pulledElements.Add(element);
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
            for (var i = 0; i < PulledElements.Count; i++)
            {
                var element = PulledElements[i];
                if (_manageActiveness)
                    element.gameObject.SetActive(false);
                _queue.Enqueue(element);
            }
            _pulledElements.Clear();
        }
        
        private T InstantiateComponent()
        {
            var component = Instantiate(_prefab, transform);
            component.name = $"--{_queue.Count}_{ComponentName}";
            return component;
        }
    }
}