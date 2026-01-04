using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using DingoUnityExtensions.MonoBehaviours.Singletons;
using UnityEngine;

namespace DingoUnityExtensions.MonoBehaviours
{
    public abstract class CamerasRoot<TSelf, TKey> : ProtectedSingletonBehaviour<TSelf> 
        where TSelf : CamerasRoot<TSelf, TKey> 
    {
        [SerializeField] private SerializedDictionary<TKey, Camera> _cameras;

        public static Camera GetCamera(TKey key)
        {
            var instance = GetNoCheck();
            if (instance == null)
                return null;

            return instance._cameras.GetValueOrDefault(key);
        }
    }
}