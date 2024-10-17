using UnityEngine;

namespace DingoUnityExtensions.MonoBehaviours.Singletons
{
    public class SingletonProtectedBehaviour<T> : MonoBehaviour where T : SingletonProtectedBehaviour<T>
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
                            var obj = new GameObject($"S_{typeof(T).Name}");
                            _instance = obj.AddComponent<T>();
                            SetDontDestroyOnLoad(obj);
                        }
                        else if (instances.Length == 1)
                        {
                            _instance = instances[0];
                            SetDontDestroyOnLoad(_instance.gameObject);
                        }
                        else
                        {
                            Debug.LogError($"[Singleton] Multiple instances of singleton '{typeof(T)}' found. This can lead to incorrect behavior.");
                            _instance = instances[0];
                            SetDontDestroyOnLoad(_instance.gameObject);
                            for (var i = 1; i < instances.Length; i++)
                            {
                                Destroy(instances[i].gameObject);
                            }
                        }
                    }

                    return _instance;
                }
            }
        }

        private static void SetDontDestroyOnLoad(GameObject go)
        {
            if (go.transform.parent == null) 
                DontDestroyOnLoad(go);
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