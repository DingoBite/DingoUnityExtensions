using UnityEngine;

namespace DingoUnityExtensions.MonoBehaviours.Singletons
{
    public abstract class SingletonBehaviour<T> : MonoBehaviour where T : SingletonBehaviour<T>
    {
        private static T _instance;
        private static readonly object _lock = new();

        public static T Instance
        {
            get
            {
                lock (_lock)
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
        private static readonly object _lock = new();

        protected static T Instance
        {
            get
            {
                lock (_lock)
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