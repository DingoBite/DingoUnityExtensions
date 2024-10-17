using DingoUnityExtensions.MonoBehaviours.Singletons;
using DingoUnityExtensions.Pools;
using UnityEngine;

namespace DingoUnityExtensions.DevView
{
    public class DevDrawer : SingletonBehaviour<DevDrawer>
    {
        [SerializeField] private DevDrawerViewPool _drawerViewPool;

        public Pool<DevDrawerView> DevDrawerViewPool => _drawerViewPool;
    }
}