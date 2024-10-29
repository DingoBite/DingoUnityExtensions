using UnityEngine;

namespace DingoUnityExtensions.MonoBehaviours.Singletons
{
    public abstract class SingletonScriptableObject<T> : ScriptableObject where T : SingletonScriptableObject<T>
    {
        private static T _instance;
        private static readonly object _lock = new();
        private static bool _applicationIsQuitting;

        static SingletonScriptableObject()
        {
            Application.quitting += OnApplicationQuit;
        }

        public static T Instance
        {
            get
            {
                if (_applicationIsQuitting)
                {
                    Debug.LogWarning($"[Singleton] Instance of '{typeof(T)}' already destroyed. Returning null.");
                    return null;
                }

                lock (_lock)
                {
                    if (_instance == null)
                    {
                        var instances = Resources.FindObjectsOfTypeAll<T>();

                        if (instances.Length > 1)
                        {
                            Debug.LogError($"[Singleton] Multiple instances of singleton '{typeof(T)}' found. This can lead to incorrect behavior.");
                            _instance = instances[0];
                        }
                        else if (instances.Length == 1)
                        {
                            _instance = instances[0];
                        }
                        else if (Application.isPlaying)
                        {
                            Debug.LogError($"[Singleton] No instances of singleton '{typeof(T)}' found. This can lead to incorrect behavior.");
                        }
                    }

                    return _instance;
                }
            }
        }

        protected virtual void OnDisable()
        {
            if (_instance == this)
            {
                _applicationIsQuitting = true;
            }
        }

        private static void OnApplicationQuit()
        {
            _applicationIsQuitting = true;
        }
    }

    public abstract class ProtectedSingletonScriptableObject<T> : ScriptableObject where T : ProtectedSingletonScriptableObject<T>
    {
        private static T _instance;
        private static readonly object _lock = new();
        private static bool _applicationIsQuitting;

        static ProtectedSingletonScriptableObject()
        {
            Application.quitting += OnApplicationQuit;
        }

        protected static T Instance
        {
            get
            {
                if (_applicationIsQuitting)
                {
                    Debug.LogWarning($"[Singleton] Instance of '{typeof(T)}' already destroyed. Returning null.");
                    return null;
                }

                lock (_lock)
                {
                    if (_instance == null)
                    {
                        var instances = Resources.FindObjectsOfTypeAll<T>();

                        if (instances.Length > 1)
                        {
                            Debug.LogError($"[Singleton] Multiple instances of singleton '{typeof(T)}' found. This can lead to incorrect behavior.");
                            _instance = instances[0];
                        }
                        else if (instances.Length == 1)
                        {
                            _instance = instances[0];
                        }
                        else if (Application.isPlaying)
                        {
                            Debug.LogError($"[Singleton] No instances of singleton '{typeof(T)}' found. This can lead to incorrect behavior.");
                        }
                    }

                    return _instance;
                }
            }
        }

        protected virtual void OnDisable()
        {
            if (_instance == this)
            {
                _applicationIsQuitting = true;
            }
        }

        private static void OnApplicationQuit()
        {
            _applicationIsQuitting = true;
        }
    }
}
