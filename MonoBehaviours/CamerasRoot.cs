using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using DingoUnityExtensions.MonoBehaviours.Singletons;
using UnityEngine;

namespace DingoUnityExtensions.MonoBehaviours
{
    public abstract class CamerasRoot<TSelf, TEnum> : ProtectedSingletonBehaviour<TSelf> 
        where TSelf : CamerasRoot<TSelf, TEnum> 
    {
        [SerializeField] private SerializedDictionary<TEnum, Camera> _cameras;

        public static Camera GetCamera(TEnum key)
        {
            if (Instance == null)
                return null;

            return Instance._cameras.GetValueOrDefault(key);
        }
    }
}