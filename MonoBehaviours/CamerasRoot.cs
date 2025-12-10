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
            if (Instance == null)
                return null;

            return Instance._cameras.GetValueOrDefault(key);
        }
    }
}