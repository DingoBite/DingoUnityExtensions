using UnityEngine;

namespace DingoUnityExtensions.MonoBehaviours.Singletons
{
    public abstract class SingletonBehaviour<T> : MonoBehaviour where T : SingletonBehaviour<T>
    {
        private static T _instance;
        private static readonly object _lock = new();
        private static bool _applicationIsQuitting;

        public static T Instance
        {
            get
            {
                // if (_applicationIsQuitting)
                // {
                //     Debug.LogWarning($"[Singleton] Instance '{typeof(T)}' already destroyed. Returning null.");
                //     return null;
                // }

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

        protected virtual void OnDestroy()
        {
            if (_instance == this)
            {
                _applicationIsQuitting = true;
            }
        }

        protected virtual void OnApplicationQuit()
        {
            _applicationIsQuitting = true;
        }
    }

    public abstract class ProtectedSingletonBehaviour<T> : MonoBehaviour where T : ProtectedSingletonBehaviour<T>
    {
        private static T _instance;
        private static readonly object _lock = new();
        private static bool _applicationIsQuitting;

        protected static T Instance
        {
            get
            {
                if (_applicationIsQuitting)
                {
                    Debug.LogWarning($"[Singleton] Instance '{typeof(T)}' already destroyed. Returning null.");
                    return null;
                }

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

        protected virtual void OnDestroy()
        {
            if (_instance == this)
            {
                _applicationIsQuitting = true;
            }
        }

        protected virtual void OnApplicationQuit()
        {
            _applicationIsQuitting = true;
        }
    }
}