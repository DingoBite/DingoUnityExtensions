using System;
using UnityEngine;

namespace DingoUnityExtensions.UnityViewProviders.Pools
{
    [Serializable]
    public struct CollectionViewSpawnOptions
    {
        [SerializeField] private bool _immediate;
        [SerializeField] private bool _fullRebuild;

        public bool Immediate => _immediate;
        public bool FullRebuild => _fullRebuild;

        public CollectionViewSpawnOptions(bool immediate = false, bool fullRebuild = false)
        {
            _immediate = immediate;
            _fullRebuild = fullRebuild;
        }

        public static CollectionViewSpawnOptions Default => default;
        public static CollectionViewSpawnOptions ImmediateFill => new(immediate: true);
        public static CollectionViewSpawnOptions ImmediateRebuild => new(immediate: true, fullRebuild: true);
    }
}
