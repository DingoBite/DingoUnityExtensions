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
        private readonly Dictionary<T, int> _elementVersions = new();
        private readonly Action<T, bool> _setActiveOverwrite;
        private readonly Func<GameObject, T> _factory;
        private readonly Func<GameObject, UniTask<T>> _asyncFactory;
        private readonly Func<T, UniTask> _asyncPushOverwrite;

        private string ComponentName => typeof(T).Name;
        public IReadOnlyList<T> PulledElements => _pulledElements;
        public GameObject Parent => _parent;

        private Pool(GameObject parent,
            SortTransformOrderOption sortTransformOrder = SortTransformOrderOption.AsLast,
            bool layerFromPool = true,
            bool manageActiveness = true,
            Action<T, bool> setActiveOverwrite = null,
            Func<T, UniTask> asyncPushOverwrite = null)
        {
            _setActiveOverwrite = setActiveOverwrite ?? ((behaviour, b) => behaviour.gameObject.SetActive(b));
            _manageActiveness = manageActiveness;
            _sortTransformOrder = sortTransformOrder;
            _layerFromPool = layerFromPool;
            _parent = parent;
            _asyncPushOverwrite = asyncPushOverwrite;
        }
        
        public Pool(T prefab, GameObject parent, SortTransformOrderOption sortTransformOrder = SortTransformOrderOption.AsLast, bool layerFromPool = true, bool manageActiveness = true, Action<T, bool> setActiveOverwrite = null, Func<T, UniTask> asyncPushOverwrite = null)
            : this(parent, sortTransformOrder, layerFromPool, manageActiveness, setActiveOverwrite, asyncPushOverwrite)
        {
            _prefab = prefab;
        }

        public Pool(Func<GameObject, T> factory, GameObject parent, SortTransformOrderOption sortTransformOrder = SortTransformOrderOption.AsLast, bool layerFromPool = true, bool manageActiveness = true, Action<T, bool> setActiveOverwrite = null, Func<T, UniTask> asyncPushOverwrite = null)
            : this(parent, sortTransformOrder, layerFromPool, manageActiveness, setActiveOverwrite, asyncPushOverwrite)
        {
            _factory = factory;
        }
        
        public Pool(Func<GameObject, UniTask<T>> asyncFactory, GameObject parent, SortTransformOrderOption sortTransformOrder = SortTransformOrderOption.AsLast, bool layerFromPool = true, bool manageActiveness = true, Action<T, bool> setActiveOverwrite = null, Func<T, UniTask> asyncPushOverwrite = null)
            : this(parent, sortTransformOrder, layerFromPool, manageActiveness, setActiveOverwrite, asyncPushOverwrite)
        {
            _asyncFactory = asyncFactory;
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
        
        public async UniTask<T> PullElementAsync()
        {
            if (_asyncFactory == null)
                return PullElement();

            while (_queue.TryPop(out var element))
            {
                if (element == null)
                    continue;
                if (!_queuedLookup.Remove(element))
                    continue;

                ActivateElement(element);
                return element;
            }

            var instantiated = await InstantiateComponentAsync();
            ActivateElement(instantiated);
            return instantiated;
        }

        private void ActivateElement(T element)
        {
            RememberVersion(element);
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

        public async UniTask PushElementAsync(T element)
        {
            if (element == null)
                return;
            if (!_pulledLookup.Contains(element))
                return;
            if (_asyncPushOverwrite == null)
            {
                PushElement(element);
                return;
            }

            var version = GetVersion(element);
            await _asyncPushOverwrite(element);

            if (GetVersion(element) != version)
                return;
            if (!_pulledLookup.Contains(element))
                return;

            PushElement(element);
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
        
        private async UniTask<T> InstantiateComponentAsync()
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

        private void RememberVersion(T element)
        {
            if (element == null)
                return;

            _elementVersions.TryGetValue(element, out var version);
            _elementVersions[element] = version + 1;
        }

        private int GetVersion(T element)
        {
            if (element == null)
                return 0;
            return _elementVersions.GetValueOrDefault(element);
        }
    }
}
