#if NEWTONSOFT_EXISTS
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using NaughtyAttributes;
using NaughtyAttributes.ColorKeyProperties;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using Utils.Inspector;

namespace DingoUnityExtensions.Configurator.Core
{
    public abstract class ConfigProvider<TConfig> : MonoBehaviour, IAppliable where TConfig : Config, new()
    {
        [SerializeField] private bool _hideConfig;
        [SerializeField] private bool _hideConfigs;
#if NAUGHTYATTRIBUTES_EXISTS
        [field: HideIf(nameof(_hideConfig))]
#endif
        [field: SerializeField] public TConfig CurrentConfig { get; private set; }
#if NAUGHTYATTRIBUTES_EXISTS
        [HideIf(nameof(_hideConfigs))]
#endif
        [SerializeField] private List<TConfig> _configs;
        [SerializeField] private bool _isApplyOnStart;
        [SerializeField] private bool _alwaysTryToCreateDirectory;
        [SerializeField] private bool _pushOnSave;
        [SerializeField] private bool _onValidateApply;
        [SerializeField] private bool _isStreamingAssets;
#if NAUGHTYATTRIBUTES_EXISTS
        [DisableIf(nameof(_isStreamingAssets))]
#endif
        [SerializeField] private string _defaultPath;
        [Space(20)]
#if NAUGHTYATTRIBUTES_EXISTS
        [Dropdown(nameof(ConfigNames))]
#endif
        [SerializeField] private string _configName;
        
        private List<string> ConfigNames
        {
            get
            {
                if (_configs == null || _configs.Count == 0)
                    return new List<string>{ "None" }; 
                return _configs.Select(c => c.ConfigName).ToList();
            }
        }
        
        private void Start()
        {
            if (_isApplyOnStart) ReloadConfig();
        }

        public void ReloadConfig()
        {
            var directory = Path.Combine(Application.streamingAssetsPath, new TConfig().FolderName);
            _configs = Load(directory);
            if (_configs.Count > 0)
            {
                var mainIndex = _configs.FindIndex(c => c.ConfigName == "main");
                if (mainIndex >= 0)
                {
                    CurrentConfig = Copy(_configs[mainIndex]);
                    Apply();
                }
            }
        }

        public async Task ReloadConfigAsync()
        {
            var directory = Path.Combine(Application.streamingAssetsPath, new TConfig().FolderName);
            _configs = await LoadAsync(directory);
            if (_configs.Count > 0)
            {
                var mainIndex = _configs.FindIndex(c => c.ConfigName == "main");
                if (mainIndex >= 0)
                {
                    CurrentConfig = Copy(_configs[mainIndex]);
                    Apply();
                }
            }
        }
        
        protected abstract TConfig DefaultConfig { get; } 
        protected abstract bool Equal(TConfig c1, TConfig c2);

        protected virtual TConfig Copy(TConfig c1) => c1 with {};

        public void SetName(string configName) => _configName = configName;
        
        public void UpdateConfig()
        {
            LoadFromFiles();
            SelectConfigFromName();
            Apply();
        }
        
        public async Task UpdateConfigAsync()
        {
            await LoadFromFilesAsync();
            SelectConfigFromName();
            Apply();
        }
#if NAUGHTYATTRIBUTES_CK_EXISTS
        [ColorKey(210, 130, 30, ColorPart.Background)]
#endif
#if NAUGHTYATTRIBUTES_EXISTS
        [Button]
#endif
        private void SelectConfigFromName()
        {
            CurrentConfig = Copy(_configs.First(c => c.ConfigName == _configName));
        }

#if NAUGHTYATTRIBUTES_CK_EXISTS
        [ColorKey(210, 130, 30, ColorPart.Background)]
#endif
#if NAUGHTYATTRIBUTES_EXISTS
        [Button]
#endif
        public void Apply()
        {
            void configurableFieldProcessor(object obj, ConfigurableFieldInfo configurableFieldInfo)
            {
                configurableFieldInfo.ConfigureObject(obj, CurrentConfig);
            }
            void configurableConfigProcessor(object obj, ConfigurableFieldInfo field, TConfig c)
            {
                field.FieldInfo.SetValue(obj, CurrentConfig);
            }
            
            ProcessConfigurables(configurableFieldProcessor, configurableConfigProcessor);
        }

#if NAUGHTYATTRIBUTES_CK_EXISTS
        [ColorKey(110, 30, 215, ColorPart.Background)]
#endif
#if NAUGHTYATTRIBUTES_EXISTS
        [Button]
#endif
        private void CollectCurrentConfig()
        {
            AppendCurrentConfig();
            CurrentConfig = GetCurrentConfigState();
        }

#if NAUGHTYATTRIBUTES_CK_EXISTS
        [ColorKey(110, 30, 215, ColorPart.Background)]
#endif
#if NAUGHTYATTRIBUTES_EXISTS
        [Button]
#endif
        private void AppendCurrentConfig()
        {
            if (Equal(CurrentConfig, DefaultConfig) || _configs.Any(c => Equal(c, CurrentConfig)) || string.IsNullOrWhiteSpace(CurrentConfig?.ConfigName))
                return;
            _configs.Add(CurrentConfig);
            CurrentConfig = Copy(CurrentConfig);
            ValidateNames();
        }

#if NAUGHTYATTRIBUTES_CK_EXISTS
        [ColorKey(110, 30, 215, ColorPart.Background)]
#endif
#if NAUGHTYATTRIBUTES_EXISTS
        [Button]
#endif
        public void PushOrEditExistConfig()
        {
            if (Equal(CurrentConfig, DefaultConfig))
                return;

            var configIndex = _configs.FindIndex(c => c.ConfigName == CurrentConfig.ConfigName);
            if (configIndex >= 0)
            {
                _configs[configIndex] = Copy(CurrentConfig);
            }
            else
            {
                _configs.Add(CurrentConfig);
                CurrentConfig = Copy(CurrentConfig);
            }
        }

#if NAUGHTYATTRIBUTES_CK_EXISTS
        [ColorKey(55, 115, 65, ColorPart.Background)]
#endif
#if NAUGHTYATTRIBUTES_EXISTS
        [Button]
#endif
        public void SaveToFiles()
        {
            string directory = null;
            if (_isStreamingAssets)
            {
                directory = Application.streamingAssetsPath;
            }
            else if (!string.IsNullOrWhiteSpace(_defaultPath) && Directory.Exists(_defaultPath))
            {
                directory = _defaultPath;
            }
            else if (!string.IsNullOrWhiteSpace(_defaultPath) && Directory.Exists(Path.Combine(Application.dataPath, _defaultPath)))
            {
                directory = Path.Combine(Application.dataPath, _defaultPath);
            }
            else
            {
#if UNITY_EDITOR
                directory = EditorUtility.OpenFolderPanel("Select save configs folder", Application.dataPath + "/Assets", null);
#endif
            }
            
            if (directory == null || !Directory.Exists(directory))
            {
                Debug.LogError($"Directory not fount {directory}");
                return;
            }
            if (_pushOnSave)
                AppendCurrentConfig();
            var rootDirectoryEntities = Directory.GetFileSystemEntries(directory);
            var isNewDirectory = _alwaysTryToCreateDirectory || rootDirectoryEntities.Length != 0;
            foreach (var config in _configs)
            {
                Save(directory, config, isNewDirectory);
            }
        }

#if NAUGHTYATTRIBUTES_CK_EXISTS
        [ColorKey(55, 115, 65, ColorPart.Background)]
#endif
#if NAUGHTYATTRIBUTES_EXISTS
        [Button]
#endif
        private void LoadFromFiles()
        {
            if (GetDirectory(out var directory))
                return;
            _configs = Load(directory);
            if (_configs.Count != 0)
                CurrentConfig = _configs.Last();
        }

        private async Task LoadFromFilesAsync()
        {
            if (GetDirectory(out var directory))
                return;
            _configs = await LoadAsync(directory);
            if (_configs.Count != 0)
                CurrentConfig = _configs.Last();
        }
        
        private bool GetDirectory(out string directory)
        {
            directory = null;
            if (_isStreamingAssets)
            {
                directory = Application.streamingAssetsPath;
            }
            else if (!string.IsNullOrWhiteSpace(_defaultPath) && Directory.Exists(_defaultPath))
            {
                directory = _defaultPath;
            }
            else if (!string.IsNullOrWhiteSpace(_defaultPath) && Directory.Exists(Path.Combine(Application.dataPath, _defaultPath)))
            {
                directory = Path.Combine(Application.dataPath, _defaultPath);
            }
            else
            {
#if UNITY_EDITOR
                directory = EditorUtility.OpenFolderPanel("Select save configs folder", Application.dataPath + "/Assets", null);
#endif
            }

            if (directory == null || !Directory.Exists(directory))
            {
                Debug.LogError($"Directory not fount {directory}");
                return true;
            }

            directory += $"/{DefaultConfig.FolderName}";
            return false;
        }

#if NAUGHTYATTRIBUTES_CK_EXISTS
        [ColorKey(150, 150, 150, ColorPart.Background)]
#endif
#if NAUGHTYATTRIBUTES_EXISTS
        [Button]
#endif
        private void SelfCopyAll()
        {
            CurrentConfig = Copy(CurrentConfig);
            for (var i = 0; i < _configs.Count; i++)
            {
                var config = _configs[i];
                _configs[i] = Copy(config);
            }
        }

        
        private void Save(string rootPath, TConfig config, bool isNewDirectory)
        {
            string directoryPath;
            if (isNewDirectory)
            {
                directoryPath = Path.Combine(rootPath, config.FolderName);
                Directory.CreateDirectory(directoryPath);
            }
            else
            {
                directoryPath = rootPath;
            }

            var data = JsonConvert.SerializeObject(config);
            var filePath = Path.Combine(directoryPath, $"{config.ConfigName}.json");
            File.WriteAllText(filePath, data);
        }

        private List<TConfig> Load(string rootPath)
        {
            var configs = new List<TConfig>();
            var files = Directory.GetFiles(rootPath, "*.json");
            foreach (var file in files)
            {
                var fileData = File.ReadAllText(file);
                try
                {
                    var config = JsonConvert.DeserializeObject<TConfig>(fileData);
                    configs.Add(config);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            if (configs.Count == 0)
            {
                rootPath = Path.Combine(rootPath, new TConfig().FolderName);
                if (Directory.Exists(rootPath))
                    configs = Load(rootPath);
            }
            return configs;
        }

        private async Task<List<TConfig>> LoadAsync(string rootPath)
        {
            var configs = new List<TConfig>();
            var files = Directory.GetFiles(rootPath, "*.json");
            foreach (var file in files)
            {
                var fileData = await File.ReadAllTextAsync(file);
                try
                {
                    var config = JsonConvert.DeserializeObject<TConfig>(fileData);
                    configs.Add(config);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            if (configs.Count == 0)
            {
                rootPath = Path.Combine(rootPath, new TConfig().FolderName);
                if (Directory.Exists(rootPath))
                    configs = Load(rootPath);
            }
            return configs;
        }
        
        private TConfig GetCurrentConfigState()
        {
            var config = new TConfig();
            void configurableFieldProcessor(object obj, ConfigurableFieldInfo configurableFieldInfo)
            {
                configurableFieldInfo.SetStateFromObject(obj, config);
            }

            void configurableConfigProcessor(object obj, ConfigurableFieldInfo field, TConfig c)
            {
                config = c;
            }
            
            ProcessConfigurables(configurableFieldProcessor, configurableConfigProcessor);
            return config;
        }

        private void ProcessConfigurables(Action<object, ConfigurableFieldInfo> configurableFieldProcessor, Action<object, ConfigurableFieldInfo, TConfig> configurableConfigProcessor)
        {
            var configurables = HierarchyFinder.FindInSceneComponents<IConfigurable>();
            var configurableTypes = GetConfigurableTypes();
            foreach (var configurable in configurables)
            {
                foreach (var configurableType in configurableTypes)
                {
                    try
                    {

                        var obj = Convert.ChangeType(configurable, configurableType.Type);
                        foreach (var configurableFieldInfo in configurableType.ConfigurableFields)
                        {
                            var value = configurableFieldInfo.FieldInfo != null ? configurableFieldInfo.FieldInfo.GetValue(configurable) : configurableFieldInfo.PropertyInfo.GetValue(configurable);
                            if (value is TConfig c)
                                configurableConfigProcessor(configurable, configurableFieldInfo, c);
                            else
                                configurableFieldProcessor(obj, configurableFieldInfo);
                        }
                        if (configurable is IConfigurable.Appliable configurableWithApply)
                            configurableWithApply.ApplyConfig();
                        break;
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning(e);
                    }
                }
            }
        }
        
        private class ConfigurableFieldInfo
        {
            public readonly FieldInfo FieldInfo;
            public readonly PropertyInfo PropertyInfo;
            public readonly string ConfigFieldName;

            public ConfigurableFieldInfo(FieldInfo fieldInfo, string configFieldName)
            {
                FieldInfo = fieldInfo;
                ConfigFieldName = configFieldName;
            }
            
            public ConfigurableFieldInfo(PropertyInfo propertyInfo, string configFieldName)
            {
                PropertyInfo = propertyInfo;
                ConfigFieldName = configFieldName;
            }
            
            public void ConfigureObject(object o, TConfig c)
            {
                var value = GetConfigValue(c);
                if (FieldInfo != null)
                    FieldInfo.SetValue(o, value);
                else
                    PropertyInfo.SetValue(o, value);
            }

            public void SetStateFromObject(object o, TConfig c)
            {
                var value = FieldInfo == null ? PropertyInfo.GetValue(o) : FieldInfo.GetValue(o);
                SetConfigField(c, value);
            }

            private object GetConfigValue(object c)
            {
                var type = typeof(TConfig);
                var pathElements = ConfigFieldName.Split("/");
                if (pathElements.Length is 0 or 1)
                    return typeof(TConfig).GetField(ConfigFieldName).GetValue(c);

                object value = null;
                try
                {
                    foreach (var pathElement in pathElements)
                    {
                        var obj = value ?? c;
                        var fieldInfo = type.GetField(pathElement);
                        type = fieldInfo.FieldType;
                        value = fieldInfo.GetValue(obj);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Cannot Find Field in Path: {ConfigFieldName}");
                    Debug.LogException(e);
                    return null;
                }

                return value;
            }
            
            private void SetConfigField(object obj, object value)
            {
                var type = typeof(TConfig);
                var pathElements = ConfigFieldName.Split("/");
                if (pathElements.Length is 0 or 1)
                {
                    typeof(TConfig).GetField(ConfigFieldName).SetValue(obj, value);
                    return;
                }

                object newObj = null;
                FieldInfo fieldInfo = null;
                try
                {
                    for (var i = 0; i < pathElements.Length - 1; i++)
                    {
                        var pathElement = pathElements[i];
                        var o = newObj ?? obj;
                        var prevFieldInfo = fieldInfo;
                        fieldInfo = type.GetField(pathElement);
                        type = fieldInfo.FieldType;
                        newObj = fieldInfo.GetValue(o);
                        if (prevFieldInfo != null)
                            prevFieldInfo.SetValue(o, newObj);
                    }

                    fieldInfo = type.GetField(pathElements[^1]);
                    fieldInfo.SetValue(newObj, value);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Cannot Find Field in Path: {ConfigFieldName}");
                    Debug.LogException(e);
                }
            }
        }
        
        private class ConfigurableTypeInfo
        {
            public readonly Type Type;
            public readonly int Priority;
            public readonly IReadOnlyList<ConfigurableFieldInfo> ConfigurableFields;

            public ConfigurableTypeInfo(Type type, int priority)
            {
                Type = type;
                Priority = priority;
                var configurableFields = Type.GetRuntimeFields()
                    .Where(f => f.GetCustomAttribute<ConfigurableAttribute>() != null)
                    .Select(f => (f, f.GetCustomAttribute<ConfigurableAttribute>()));
                var configurableProperties = Type.GetRuntimeProperties()
                    .Where(f => f.GetCustomAttribute<ConfigurableAttribute>() != null)
                    .Select(f => (f, f.GetCustomAttribute<ConfigurableAttribute>()));
                
                ConfigurableFields = configurableFields
                    .Select(v => new ConfigurableFieldInfo(v.f, v.Item2.ConfigFieldName))
                    .Concat(
                        configurableProperties
                            .Select(v => new ConfigurableFieldInfo(v.f, v.Item2.ConfigFieldName))
                    )
                    .ToList();
            }
        }
        
        private List<ConfigurableTypeInfo> GetConfigurableTypes()
        {
            var classes = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t =>
                {
                    var attribute = t.GetCustomAttribute<ConfigurableEntityAttribute>();
                    return attribute != null && attribute.ConfigType == typeof(TConfig);
                })
                .Select(t => new ConfigurableTypeInfo(t, t.GetCustomAttribute<ConfigurableEntityAttribute>().Priority))
                .ToList();
            classes.Sort((c1, c2) => c2.Priority.CompareTo(c1.Priority));
            return classes;
        }

        private void ValidateNames()
        {
            for (var i = 0; i < _configs.Count; i++)
            {
                var config1 = _configs[i];
                var k = 0;
                for (var j = i + 1; j < _configs.Count; j++)
                {
                    var config2 = _configs[j];
                    if (config2.ConfigName == config1.ConfigName)
                    {
                        config2.ConfigName += "_" + k;
                        k++;
                    }
                }
            }
        }

        private void OnValidate()
        {
            if (_configs == null)
                _configs = new List<TConfig>();
            ValidateNames();
            if (_onValidateApply)
                Apply();
        }
    }
}
#endif
