using DingoUnityExtensions.MonoBehaviours.Singletons;
using DingoUnityExtensions.Pools;
using DingoUnityExtensions.Pools.Core;
using UnityEngine;

namespace DingoUnityExtensions.DevView
{
    public class DevDrawer : SingletonBehaviour<DevDrawer>
    {
        [SerializeField] private DevDrawerViewPool _drawerViewPool;

        public PoolBehaviour<DevDrawerView> DevDrawerViewPool => _drawerViewPool;
    }
}