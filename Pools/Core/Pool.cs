using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DingoUnityExtensions.Extensions;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DingoUnityExtensions.Pools.Core
{
    public class Pool<T> : IPoolGetOnly<T> where T : MonoBehaviour
    {
        private readonly GameObject _parent;
        private readonly T _prefab;
        private readonly bool _manageActiveness;
        private readonly SortTransformOrderOption _sortTransformOrder;
        private readonly bool _layerFromPool;

        private readonly List<T> _pulledElements = new();
        private readonly Queue<T> _queue = new();
        private readonly Action<T, bool> _setActiveOverwrite;
        private readonly Func<GameObject, T> _factory;
        private readonly Func<GameObject, Task<T>> _asyncFactory;

        private string ComponentName => typeof(T).Name;
        public IReadOnlyList<T> PulledElements => _pulledElements;
        public GameObject Parent => _parent;

        private Pool(GameObject parent,
            SortTransformOrderOption sortTransformOrder = SortTransformOrderOption.AsLast,
            bool layerFromPool = true, 
            bool manageActiveness = true, 
            Action<T, bool> setActiveOverwrite = null)
        {
            _setActiveOverwrite = setActiveOverwrite ?? ((behaviour, b) => behaviour.gameObject.SetActive(b));
            _manageActiveness = manageActiveness;
            _sortTransformOrder = sortTransformOrder;
            _layerFromPool = layerFromPool;
            _parent = parent;
        }
        
        public Pool(T prefab, GameObject parent, SortTransformOrderOption sortTransformOrder = SortTransformOrderOption.AsLast, bool layerFromPool = true, bool manageActiveness = true, Action<T, bool> setActiveOverwrite = null) 
            : this(parent, sortTransformOrder, layerFromPool, manageActiveness, setActiveOverwrite)
        {
            _prefab = prefab;
        }
        public Pool(Func<GameObject, T> factory, GameObject parent, SortTransformOrderOption sortTransformOrder = SortTransformOrderOption.AsLast, bool layerFromPool = true, bool manageActiveness = true, Action<T, bool> setActiveOverwrite = null) 
            : this(parent, sortTransformOrder, layerFromPool, manageActiveness, setActiveOverwrite)
        {
            _factory = factory;
        }
        
        public Pool(Func<GameObject, Task<T>> asyncFactory, GameObject parent, SortTransformOrderOption sortTransformOrder = SortTransformOrderOption.AsLast, bool layerFromPool = true, bool manageActiveness = true, Action<T, bool> setActiveOverwrite = null) 
            : this(parent, sortTransformOrder, layerFromPool, manageActiveness, setActiveOverwrite)
        {
            _asyncFactory = asyncFactory;
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
        
        public async Task<T> PullElementAsync()
        {
            if (_asyncFactory == null)
                return PullElement();
            if (_queue.TryDequeue(out var element))
            {
                ManageActiveness(element, true);
                _pulledElements.Add(element);
                Sort(element);
                return element;
            }
            element = await InstantiateComponentAsync();
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
            if (_parent != null)
                element.transform.SetParent(_parent.transform);
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
            T component;
            if (_factory != null)
                component = _factory(_parent);
            else if (_parent == null)
                component = Object.Instantiate(_prefab);
            else
                component = Object.Instantiate(_prefab, _parent.transform);
            if (_layerFromPool && _parent != null)
                component.gameObject.SetLayerRecursive(_parent.layer);
            component.name = $"--{_pulledElements.Count}_{ComponentName}";
            return component;
        }
        
        private async Task<T> InstantiateComponentAsync()
        {
            var component = await _asyncFactory(_parent);
            if (_layerFromPool && _parent != null)
                component.gameObject.SetLayerRecursive(_parent.layer);
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