using UnityEngine;

namespace DingoUnityExtensions.MonoBehaviours.Singletons
{
    public abstract class SingletonScriptableObject<T> : ScriptableObject where T : SingletonScriptableObject<T>
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

                    return _instance;
                }
            }
        }
    }

    public abstract class ProtectedSingletonScriptableObject<T> : ScriptableObject where T : ProtectedSingletonScriptableObject<T>
    {
        private static T _instance;
        private static readonly object _lock = new();

        protected static T Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance != null)
                        return _instance;
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

                    return _instance;
                }
            }
        }
    }
}
