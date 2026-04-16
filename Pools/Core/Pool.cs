using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
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
        private readonly HashSet<T> _pulledLookup = new();
        private readonly Stack<T> _queue = new();
        private readonly HashSet<T> _queuedLookup = new();
        private readonly Action<T, bool> _setActiveOverwrite;
        private readonly Func<GameObject, T> _factory;

        private string ComponentName => typeof(T).Name;
        public IReadOnlyList<T> PulledElements => _pulledElements;
        public GameObject Parent => _parent;

        private Pool(GameObject parent, SortTransformOrderOption sortTransformOrder = SortTransformOrderOption.AsLast, bool layerFromPool = true, bool manageActiveness = true, Action<T, bool> setActiveOverwrite = null)
        {
            _setActiveOverwrite = setActiveOverwrite ?? ((behaviour, b) => behaviour.gameObject.SetActive(b));
            _manageActiveness = manageActiveness;
            _sortTransformOrder = sortTransformOrder;
            _layerFromPool = layerFromPool;
            _parent = parent;
        }

        public Pool(T prefab, GameObject parent, SortTransformOrderOption sortTransformOrder = SortTransformOrderOption.AsLast, bool layerFromPool = true, bool manageActiveness = true, Action<T, bool> setActiveOverwrite = null) : this(parent, sortTransformOrder, layerFromPool, manageActiveness, setActiveOverwrite)
        {
            _prefab = prefab;
        }

        public Pool(Func<GameObject, T> factory, GameObject parent, SortTransformOrderOption sortTransformOrder = SortTransformOrderOption.AsLast, bool layerFromPool = true, bool manageActiveness = true, Action<T, bool> setActiveOverwrite = null) : this(parent, sortTransformOrder, layerFromPool, manageActiveness, setActiveOverwrite)
        {
            _factory = factory;
        }

        public T PullElement()
        {
            while (_queue.TryPop(out var element))
            {
                if (element == null)
                    continue;
                if (!_queuedLookup.Remove(element))
                    continue;

                ActivateElement(element);
                return element;
            }

            var instantiated = InstantiateComponent();
            ActivateElement(instantiated);
            return instantiated;
        }

        private void ActivateElement(T element)
        {
            ManageActiveness(element, true);
            if (_pulledLookup.Add(element))
                _pulledElements.Add(element);
            Sort(element);
        }

        private void ManageActiveness(T component, bool value)
        {
            if (!_manageActiveness)
                return;
            _setActiveOverwrite(component, value);
        }

        public void PushElement(T element)
        {
            if (element == null)
                return;
            if (!_pulledLookup.Remove(element))
                return;

            if (_parent != null)
                element.transform.SetParent(_parent.transform);
            ManageActiveness(element, false);
            if (_queuedLookup.Add(element))
                _queue.Push(element);
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

        private void Sort(T element)
        {
            switch (_sortTransformOrder)
            {
                case SortTransformOrderOption.None: break;
                case SortTransformOrderOption.AsLast:
                    element.transform.SetAsLastSibling();
                    break;
                case SortTransformOrderOption.AsFirst:
                    element.transform.SetAsFirstSibling();
                    break;
                default: throw new ArgumentOutOfRangeException();
            }
        }
    }
}