using System;
using System.Collections.Generic;
using System.Linq;
using DingoUnityExtensions.Extensions;
using DingoUnityExtensions.MonoBehaviours.Singletons;
using DingoUnityExtensions.Tweens;
using UnityEngine;

namespace DingoUnityExtensions.MonoBehaviours
{
    public class Dict2Tree<T1, T2, TValue>
    {
        private readonly Dictionary<T1, Dictionary<T2, TValue>> _tree = new();

        public int TopCount => _tree.Count;

        public bool TryAdd(T1 t1, T2 t2, TValue value)
        {
            var keyDict = _tree.GetOrAddAndGet(t1);
            return keyDict.TryAdd(t2, value);
        }
        
        public bool TryGetValue(T1 t1, out TValue value)
        {
            value = default;
            if (!_tree.TryGetValue(t1, out var tips) || tips.Count == 0)
                return false;
            value = tips.Values.First();
            return true;
        }
        
        public bool TryGetValue(T1 t1, T2 t2, out TValue value)
        {
            value = default;
            if (!_tree.TryGetValue(t1, out var values) || values.Count == 0)
                return false;
            if (t2 == null || t2.Equals(default(T2)))
            {
                value = values.Values.First();
                return true;
            }

            return values.TryGetValue(t2, out value);
        }

        public TValue GetOrDefault(T1 t1, T2 t2)
        {
            if (TryGetValue(t1, t2, out var value))
                return value;
            return default;
        }

        public void Clear(Action<TValue> dispose = null)
        {
            if (_tree.Count > 0 && dispose != null)
            {
                foreach (var subTree in _tree.Values)
                {
                    if (subTree != null && subTree.Count > 0)
                    {
                        foreach (var value in subTree.Values)
                        {
                            dispose(value);
                        }
                    }
                }
            }
            _tree.Clear();
        }
    }
    
    public abstract class InGamePrefab : SubscribableBehaviour
    {
        [SerializeField] private string _key;
        [SerializeField] private RevealBehaviour _revealBehaviour;

        public IRevealBehaviour RevealBehaviour => _revealBehaviour;
        public string Key => _key;
        public abstract Type Type { get; }

        protected override void SubscribeOnly() { }
        protected override void UnsubscribeOnly() { }
    }
    
    public abstract class InGamePrefabsTree<TSelf, TGamePrefab> : SingletonBehaviour<TSelf> 
        where TSelf : InGamePrefabsTree<TSelf, TGamePrefab>
        where TGamePrefab : InGamePrefab
    {
        [SerializeField] private List<TGamePrefab> _inGamePrefabs;

        private readonly Dict2Tree<Type, string, TGamePrefab> _inGamePrefabsTree = new();

        public T GetPrefab<T>(string key = null) where T : TGamePrefab
        {
            if (_inGamePrefabsTree.TopCount == 0)
                PopulateTipsTree();
            return _inGamePrefabsTree.GetOrDefault(typeof(T), key) as T;
        }
        
        protected void PopulateTipsTree()
        {
            _inGamePrefabsTree.Clear();
            foreach (var gameInfoTipPrefab in _inGamePrefabs)
            {
                if (!_inGamePrefabsTree.TryAdd(gameInfoTipPrefab.Type, gameInfoTipPrefab.Key, gameInfoTipPrefab))
                    Debug.LogError($"Key error on building tree: {gameInfoTipPrefab.Key}");
            }
        }
        
        private void OnValidate() => PopulateTipsTree();
    }
}