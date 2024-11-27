using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AYellowpaper.SerializedCollections;
using DingoUnityExtensions.MonoBehaviours.Singletons;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace DingoUnityExtensions.GlobalProjectConfiguration
{
    internal class ScriptableObjectsRootSettings : ProtectedSingletonScriptableObject<ScriptableObjectsRootSettings>
    {
        private const string SETTINGS_PATH = "Assets/Editor/" + nameof(GlobalProjectConfiguration) + "/" + nameof(ScriptableObjectsRootSettings) + ".asset";

        [SerializeField] private SerializedDictionary<string, ScriptableObject> _configs;

        public static ScriptableObject GetConfigsByKey(string key)
        {
            if (Instance == null)
            {
                Debug.LogWarning(new NullReferenceException($"Cannot find {nameof(ScriptableObjectsRootSettings)} in {nameof(GetConfigsByKey)}({key}) request"));
                return default;
            }
            var config = Instance._configs.GetValueOrDefault(key);
            if (config == null)
            {
                Debug.LogException(new NullReferenceException($"Cannot find any config with key {key}"));
#if UNITY_EDITOR
                if (!Instance._configs.TryAdd(key, default))
                {
                    GetSerializedSettings().ApplyModifiedProperties();
                    AssetDatabase.SaveAssets();
                }
#endif
            }
            return config;
        }

        public static T GetTypedConfigByKey<T>(string key) where T : ScriptableObject
        {
            var config = GetConfigsByKey(key);
            if (config != null && config is T stringConfiguratorConfig)
                return stringConfiguratorConfig;
            return null;
        }

#if UNITY_EDITOR
        internal static ScriptableObjectsRootSettings GetOrCreateSettings()
        {
            var settings = AssetDatabase.LoadAssetAtPath<ScriptableObjectsRootSettings>(SETTINGS_PATH);
            if (settings == null && SETTINGS_PATH != null)
            {
                var directoryName = Path.GetDirectoryName(SETTINGS_PATH);
                if (directoryName != null)
                    Directory.CreateDirectory(directoryName);
                settings = CreateInstance<ScriptableObjectsRootSettings>();
                DefineDefaultValues(settings);
                AssetDatabase.CreateAsset(settings, SETTINGS_PATH);
                AssetDatabase.SaveAssets();
            }

            return settings;
        }

        public static bool TryAddKey(string key)
        {
            if (Instance == null)
            {
                Debug.LogWarning(new NullReferenceException($"Cannot find {nameof(ScriptableObjectsRootSettings)} in {nameof(TryAddKey)}({key}) request"));
                return false;
            }

            if (!Instance._configs.TryAdd(key, default))
                return false;
            GetSerializedSettings().ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
            return true;
        }
        
        public static SerializedObject GetSerializedSettings()
        {
            return new SerializedObject(GetOrCreateSettings());
        }
#endif

        public static void DefineDefaultValues(ScriptableObjectsRootSettings settingsRoot)
        {
            settingsRoot._configs = new SerializedDictionary<string, ScriptableObject>();
        }
        
        internal static class Menu
        {
            private const string CONFIG_LABEL = "Scriptable Objects Configs";
#if UNITY_EDITOR
            private static IReadOnlyDictionary<string, (string label, int order)> Keywords => new Dictionary<string, (string label, int order)>
            {
                { nameof(_configs), ("Config", 0) }
            };
            
            [SettingsProvider]
            public static SettingsProvider CreateProvider()
            {
                var provider = new SettingsProvider($"Project/{CONFIG_LABEL}", SettingsScope.Project)
                {
                    label = CONFIG_LABEL,
                    guiHandler = (searchContext) =>
                    {
                        var settings = GetSerializedSettings();
                        BuildGUI(settings, searchContext);
                        settings.ApplyModifiedPropertiesWithoutUndo();
                    },
                    keywords = Keywords.OrderBy(p => p.Value.order).Select(p => p.Value.label)
                };

                return provider;
            }

            private static void BuildGUI(SerializedObject settings, string searchContext)
            {
                EditorGUILayout.PropertyField(settings.FindProperty(nameof(_configs)), new GUIContent(Keywords[nameof(_configs)].label));
            }
#endif
        }
    }
}