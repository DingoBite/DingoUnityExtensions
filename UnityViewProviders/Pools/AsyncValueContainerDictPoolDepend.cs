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
        private readonly Dictionary<TKey, PendingEntry> _pendingEntries = new();
        private readonly List<TKey> _orderedKeys = new();

        private CollectionViewSpawnOptions _pendingSpawnOptions;
        private bool _hasPendingSpawnOptions;
        private int _spawnVersion;

        public void DefaultUpdateValueWithoutNotify(TRootValue value) => UpdateValueWithoutNotify((value, CollectionViewSpawnOptions.Default));
        
        protected override void SetValueWithoutNotify((TRootValue v, CollectionViewSpawnOptions o) pair)
        {
            _pendingSpawnOptions = pair.o;
            _hasPendingSpawnOptions = true;
            _pool ??= Factory(_prefab, _parent);
            ApplyValue(pair.v, _pendingSpawnOptions);
        }

        public void Clear() => Clear(CollectionViewSpawnOptions.ImmediateFill);

        public void Clear(CollectionViewSpawnOptions spawnOptions)
        {
            _pendingSpawnOptions = spawnOptions;
            _hasPendingSpawnOptions = true;
            base.UpdateValueWithoutNotify(default);
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
            var resolvedOptions = ResolveSpawnOptions(spawnOptions);

            if (value == null)
            {
                ReleaseAll(resolvedOptions);
                return;
            }

            var count = GetCount(value);
            if (count == 0)
            {
                ReleaseAll(resolvedOptions);
                return;
            }

            if (resolvedOptions.FullRebuild)
                ReleaseAll(resolvedOptions);

            var orderedKeys = GetOrderedKeys(value).ToList();
            _orderedKeys.Clear();
            _orderedKeys.AddRange(orderedKeys);

            var nextKeys = new HashSet<TKey>(orderedKeys);

            if (_activeEntries.Count > 0)
            {
                var removedActiveKeys = _activeEntries.Keys.Where(k => !nextKeys.Contains(k)).ToList();
                foreach (var removedKey in removedActiveKeys)
                {
                    if (!_activeEntries.Remove(removedKey, out var activeEntry))
                        continue;

                    BeginRelease(removedKey, activeEntry.Value, activeEntry.Container, resolvedOptions);
                }
            }

            if (_pendingEntries.Count > 0)
            {
                var removedPendingKeys = _pendingEntries.Keys.Where(k => !nextKeys.Contains(k)).ToList();
                foreach (var removedKey in removedPendingKeys)
                {
                    _pendingEntries.Remove(removedKey);
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

                if (_pendingEntries.TryGetValue(key, out var pendingEntry))
                {
                    pendingEntry.Order = order;
                    pendingEntry.Value = subValue;
                    pendingEntry.SpawnOptions = resolvedOptions;
                    continue;
                }

                var version = ++_spawnVersion;
                _pendingEntries[key] = new PendingEntry
                {
                    Version = version,
                    Order = order,
                    Value = subValue,
                    SpawnOptions = resolvedOptions,
                };
                _ = EnsureEntryAsync(key, version);
            }

            ApplyActiveOrder();
        }

        private void ReleaseAll(CollectionViewSpawnOptions spawnOptions)
        {
            _orderedKeys.Clear();
            _pendingEntries.Clear();

            if (_activeEntries.Count == 0)
                return;

            var activeEntries = _activeEntries.ToList();
            _activeEntries.Clear();

            if (spawnOptions.Immediate)
            {
                _pool?.Clear();
                return;
            }

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

        private async UniTask EnsureEntryAsync(TKey key, int version)
        {
            TValueContainer valueContainer = null;
            try
            {
                if (!_pendingEntries.TryGetValue(key, out var pendingAtStart) || pendingAtStart.Version != version)
                    return;

                valueContainer = pendingAtStart.SpawnOptions.Immediate
                    ? _pool.PullElement()
                    : await _pool.PullElementAsync();

                if (this == null)
                {
                    if (valueContainer != null)
                        _pool.PushElement(valueContainer);
                    return;
                }

                if (!_pendingEntries.TryGetValue(key, out var pendingEntry) || pendingEntry.Version != version)
                {
                    if (valueContainer != null)
                        _pool.PushElement(valueContainer);
                    return;
                }

                _pendingEntries.Remove(key);
                SetValue(valueContainer, pendingEntry.Value);
                _activeEntries[key] = new ActiveEntry
                {
                    Order = pendingEntry.Order,
                    Value = pendingEntry.Value,
                    Container = valueContainer,
                };

                await OnAfterPullAsync(key, pendingEntry.Value, valueContainer, pendingEntry.SpawnOptions);

                if (this == null)
                    return;
                if (!_activeEntries.TryGetValue(key, out var activeEntry) || !ReferenceEquals(activeEntry.Container, valueContainer))
                    return;

                ApplyActiveOrder();
            }
            catch (Exception e)
            {
                Debug.LogException(e, this);
                if (valueContainer != null)
                    _pool.PushElement(valueContainer);
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

            try
            {
                if (spawnOptions.Immediate)
                    _pool.PushElement(valueContainer);
                else
                    await _pool.PushElementAsync(valueContainer);
            }
            catch (Exception e)
            {
                Debug.LogException(e, this);
            }
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

        private CollectionViewSpawnOptions ConsumeSpawnOptions()
        {
            if (!_hasPendingSpawnOptions)
                return _defaultSpawnOptions;

            var spawnOptions = _pendingSpawnOptions;
            _hasPendingSpawnOptions = false;
            _pendingSpawnOptions = default;
            return spawnOptions;
        }

        private CollectionViewSpawnOptions ResolveSpawnOptions(CollectionViewSpawnOptions spawnOptions)
        {
            return new CollectionViewSpawnOptions(
                immediate: spawnOptions.Immediate,
                fullRebuild: _fullRebuildOnChange || spawnOptions.FullRebuild);
        }

        private sealed class ActiveEntry
        {
            public int Order;
            public TValue Value;
            public TValueContainer Container;
        }

        private sealed class PendingEntry
        {
            public int Version;
            public int Order;
            public TValue Value;
            public CollectionViewSpawnOptions SpawnOptions;
        }
    }
}
