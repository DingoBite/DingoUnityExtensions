using System.Linq;
using UnityEngine;

namespace DingoUnityExtensions.MonoBehaviours.Singletons
{
    public abstract class SingletonBehaviour<T> : MonoBehaviour where T : SingletonBehaviour<T>
    {
        private static T _instance;
        private static readonly object Lock = new();

        public static T GetNoCheck()
        {
            if (_instance == null)
                _instance = FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None)?.FirstOrDefault();
            return _instance;
        }
        
        public static T Instance
        {
            get
            {
                lock (Lock)
                {
                    if (_instance != null)
                        return _instance;
                    var instances = FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None);

                    if (instances.Length == 0)
                    {
                        Debug.LogError($"[Singleton] No instances of singleton '{typeof(T)}' found. This can lead to incorrect behavior.");
                    }
                    else if (instances.Length == 1)
                    {
                        _instance = instances[0];
                    }
                    else
                    {
                        Debug.LogError($"[Singleton] Multiple instances of singleton '{typeof(T)}' found. This can lead to incorrect behavior.");
                        _instance = instances[0];
                        for (var i = 1; i < instances.Length; i++)
                        {
                            instances[i].gameObject.SetActive(false);
                        }
                    }

                    return _instance;
                }
            }
        }
    }

    public abstract class ProtectedSingletonBehaviour<T> : MonoBehaviour where T : ProtectedSingletonBehaviour<T>
    {
        private static T _instance;
        private static readonly object Lock = new();

        public static T GetNoCheck()
        {
            if (_instance == null)
                _instance = FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None)?.FirstOrDefault();
            return _instance;
        }

        protected static T Instance
        {
            get
            {
                lock (Lock)
                {
                    if (_instance == null)
                    {
                        var instances = FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None);

                        if (instances.Length == 0)
                        {
                            Debug.LogError($"[Singleton] No instances of singleton '{typeof(T)}' found. This can lead to incorrect behavior.");
                        }
                        else if (instances.Length == 1)
                        {
                            _instance = instances[0];
                        }
                        else
                        {
                            Debug.LogError($"[Singleton] Multiple instances of singleton '{typeof(T)}' found. This can lead to incorrect behavior.");
                            _instance = instances[0];
                            for (var i = 1; i < instances.Length; i++)
                            {
                                instances[i].gameObject.SetActive(false);
                            }
                        }
                    }

                    return _instance;
                }
            }
        }
    }
}