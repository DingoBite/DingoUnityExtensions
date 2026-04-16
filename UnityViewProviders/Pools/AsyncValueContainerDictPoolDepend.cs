using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DingoUnityExtensions.Pools.Core;
using DingoUnityExtensions.UnityViewProviders.Core;
using UnityEngine;

namespace DingoUnityExtensions.UnityViewProviders.Pools
{
    public abstract class AsyncValueContainerDictPoolDepend<TRootValue, TKey, TValue, TValueContainer> : ValueContainer<(TRootValue v, CollectionViewSpawnOptions o)> where TValueContainer : ValueContainer<TValue>
    {
        [SerializeField] private GameObject _parent;
        [SerializeField] private TValueContainer _prefab;
        [SerializeField] private bool _fullRebuildOnChange;
        [SerializeField] private CollectionViewSpawnOptions _defaultSpawnOptions;

        private Pool<TValueContainer> _pool;
        private readonly Dictionary<TKey, ActiveEntry> _activeEntries = new();
        private readonly List<TKey> _orderedKeys = new();
        private int _entryVersion;

        public void DefaultUpdateValueWithoutNotify(TRootValue value) => UpdateValueWithoutNotify((value, CollectionViewSpawnOptions.Default));

        public IEnumerable<TValueContainer> GetOrderedActiveContainers() => _orderedKeys.Select(k => _activeEntries[k].Container);
        public IEnumerable<TRequested> GetOrderedActiveContainers<TRequested>() => _orderedKeys.Select(k => _activeEntries[k].Container).OfType<TRequested>();

        protected override void SetValueWithoutNotify((TRootValue v, CollectionViewSpawnOptions o) pair)
        {
            _pool ??= Factory(_prefab, _parent);
            ApplyValue(pair.v, ResolveSpawnOptions(pair.o));
        }

        public void Clear() => Clear(CollectionViewSpawnOptions.ImmediateFill);

        public void Clear(CollectionViewSpawnOptions spawnOptions)
        {
            base.UpdateValueWithoutNotify((default, spawnOptions));
        }

        protected virtual void SetValue(TValueContainer valueContainer, TValue value) => valueContainer.UpdateValueWithoutNotify(value);

        protected virtual UniTask OnAfterPullAsync(TKey key, TValue value, TValueContainer valueContainer, CollectionViewSpawnOptions spawnOptions) => UniTask.CompletedTask;
        protected virtual UniTask OnBeforePushAsync(TKey key, TValue value, TValueContainer valueContainer, CollectionViewSpawnOptions spawnOptions) => UniTask.CompletedTask;
        protected virtual void OnBeginRelease(TKey key, TValue value, TValueContainer valueContainer) => valueContainer.transform.SetAsLastSibling();

        protected abstract Pool<TValueContainer> Factory(TValueContainer prefab, GameObject parent);
        protected abstract int GetCount(TRootValue value);
        protected abstract TValue GetValue(TRootValue value, TKey key);
        protected abstract IOrderedEnumerable<TKey> GetOrderedKeys(TRootValue value);

        private void ApplyValue(TRootValue value, CollectionViewSpawnOptions spawnOptions)
        {
            if (value == null)
            {
                ReleaseAll(spawnOptions);
                return;
            }

            var count = GetCount(value);
            if (count == 0)
            {
                ReleaseAll(spawnOptions);
                return;
            }

            if (spawnOptions.FullRebuild)
                ReleaseAll(spawnOptions);

            var orderedKeys = GetOrderedKeys(value).ToList();
            _orderedKeys.Clear();
            _orderedKeys.AddRange(orderedKeys);

            var nextKeys = new HashSet<TKey>(orderedKeys);
            if (_activeEntries.Count > 0)
            {
                var removedKeys = _activeEntries.Keys.Where(k => !nextKeys.Contains(k)).ToList();
                foreach (var removedKey in removedKeys)
                {
                    if (_activeEntries.Remove(removedKey, out var activeEntry))
                        BeginRelease(removedKey, activeEntry.Value, activeEntry.Container, spawnOptions);
                }
            }

            for (var order = 0; order < orderedKeys.Count; order++)
            {
                var key = orderedKeys[order];
                var subValue = GetValue(value, key);

                if (_activeEntries.TryGetValue(key, out var activeEntry))
                {
                    activeEntry.Order = order;
                    activeEntry.Value = subValue;
                    SetValue(activeEntry.Container, subValue);
                    continue;
                }

                var valueContainer = _pool.PullElement();
                SetValue(valueContainer, subValue);

                var version = ++_entryVersion;
                _activeEntries[key] = new ActiveEntry
                {
                    Version = version,
                    Order = order,
                    Value = subValue,
                    Container = valueContainer,
                };

                _ = FinalizePullAsync(key, version, subValue, valueContainer, spawnOptions);
            }

            ApplyActiveOrder();
        }

        private void ReleaseAll(CollectionViewSpawnOptions spawnOptions)
        {
            _orderedKeys.Clear();
            if (_activeEntries.Count == 0)
                return;

            var activeEntries = _activeEntries.ToList();
            _activeEntries.Clear();

            foreach (var pair in activeEntries)
            {
                BeginRelease(pair.Key, pair.Value.Value, pair.Value.Container, spawnOptions);
            }
        }

        private void BeginRelease(TKey key, TValue value, TValueContainer valueContainer, CollectionViewSpawnOptions spawnOptions)
        {
            if (valueContainer == null)
                return;

            OnBeginRelease(key, value, valueContainer);
            _ = ReleaseEntryAsync(key, value, valueContainer, spawnOptions);
        }

        private async UniTask FinalizePullAsync(TKey key, int version, TValue value, TValueContainer valueContainer, CollectionViewSpawnOptions spawnOptions)
        {
            try
            {
                await OnAfterPullAsync(key, value, valueContainer, spawnOptions);

                if (this == null)
                    return;
                if (!_activeEntries.TryGetValue(key, out var activeEntry))
                    return;
                if (activeEntry.Version != version)
                    return;
                if (!ReferenceEquals(activeEntry.Container, valueContainer))
                    return;

                ApplyActiveOrder();
            }
            catch (Exception e)
            {
                Debug.LogException(e, this);
            }
        }

        private async UniTask ReleaseEntryAsync(TKey key, TValue value, TValueContainer valueContainer, CollectionViewSpawnOptions spawnOptions)
        {
            try
            {
                await OnBeforePushAsync(key, value, valueContainer, spawnOptions);
            }
            catch (Exception e)
            {
                Debug.LogException(e, this);
            }

            if (this == null || valueContainer == null)
                return;

            _pool.PushElement(valueContainer);
        }

        private void ApplyActiveOrder()
        {
            var siblingIndex = 0;
            for (var i = 0; i < _orderedKeys.Count; i++)
            {
                var key = _orderedKeys[i];
                if (!_activeEntries.TryGetValue(key, out var activeEntry) || activeEntry.Container == null)
                    continue;

                activeEntry.Order = siblingIndex;
                activeEntry.Container.transform.SetSiblingIndex(siblingIndex);
                siblingIndex++;
            }
        }

        private CollectionViewSpawnOptions ResolveSpawnOptions(CollectionViewSpawnOptions spawnOptions)
        {
            var useDefaultOptions = spawnOptions.Equals(default(CollectionViewSpawnOptions));
            var sourceOptions = useDefaultOptions ? _defaultSpawnOptions : spawnOptions;
            return new CollectionViewSpawnOptions(immediate: sourceOptions.Immediate, fullRebuild: _fullRebuildOnChange || sourceOptions.FullRebuild);
        }

        private sealed class ActiveEntry
        {
            public int Version;
            public int Order;
            public TValue Value;
            public TValueContainer Container;
        }
    }
}