using System;
using System.Collections.Generic;
using UnityEngine;

namespace DingoUnityExtensions.Pools
{
    public interface IPoolGetOnly<out T> where T : MonoBehaviour
    {
        public IReadOnlyList<T> PulledElements { get; }
        public T PullElement();
        public void Clear();
    }

    public enum SortTransformOrderOption
    {
        None,
        AsLast,
        AsFirst
    }
    
    public class Pool<T> : MonoBehaviour, IPoolGetOnly<T> where T : MonoBehaviour
    {
        [SerializeField] private T _prefab;
        [SerializeField] private bool _syncLayer = true;
        [SerializeField] private bool _manageActiveness = true;
        [SerializeField] private SortTransformOrderOption _sortTransformOrder;

        private readonly List<T> _pulledElements = new();
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
            if (_syncLayer)
                component.gameObject.layer = gameObject.layer;
            component.name = $"--{_pulledElements.Count}_{ComponentName}";
            return component;
        }

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