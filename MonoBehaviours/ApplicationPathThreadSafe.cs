using DingoUnityExtensions.MonoBehaviours.Singletons;
using UnityEngine;

namespace DingoUnityExtensions.MonoBehaviours
{
    public class ApplicationPathThreadSafe : ProtectedSingletonBehaviour<ApplicationPathThreadSafe>
    {
        public static string PersistentDataPath { get; private set; }
        public static string DataPath { get; private set; }
        public static string StreamingAssets { get; private set; }
        public static string Temp { get; private set; }
        
        private void Awake()
        {
            PersistentDataPath = Application.persistentDataPath;
            DataPath = Application.dataPath;
            StreamingAssets = Application.streamingAssetsPath;
            Temp = Application.temporaryCachePath;
        }
    }
}