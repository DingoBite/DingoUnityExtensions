using UnityEngine;

namespace DingoUnityExtensions.MonoBehaviours.Singletons
{
    public abstract class SingletonScriptableObject<T> : ScriptableObject where T : SingletonScriptableObject<T>
    {
        public const string S_PREFIX = "S_";
        private static T _instance;
        private static readonly object Lock = new();
        
        public static T GetNoCheck()
        {
            if (_instance == null)
                _instance = Resources.Load<T>(S_PREFIX + typeof(T).Name);
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
                    _instance = Resources.Load<T>(S_PREFIX + typeof(T).Name);
                    if (_instance == null)
                        Debug.LogError($"[Singleton] No instances of singleton '{typeof(T)}' found. This can lead to incorrect behavior.");
                    return _instance;
                }
            }
        }
    }

    public abstract class ProtectedSingletonScriptableObject<T> : ScriptableObject where T : ProtectedSingletonScriptableObject<T>
    {
        public const string S_PREFIX = "S_";
        private static T _instance;
        private static readonly object Lock = new();
        
        public static T GetNoCheck()
        {
            if (_instance == null)
                _instance = Resources.Load<T>(S_PREFIX + typeof(T).Name);
            return _instance;
        }
        
        protected static T Instance
        {
            get
            {
                lock (Lock)
                {
                    if (_instance != null)
                        return _instance;
                    _instance = Resources.Load<T>(S_PREFIX + typeof(T).Name);
                    if (_instance == null)
                        Debug.LogError($"[Singleton] No instances of singleton '{typeof(T)}' found. This can lead to incorrect behavior.");
                    return _instance;
                }
            }
        }
    }
}
