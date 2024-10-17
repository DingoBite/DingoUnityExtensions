#if NEWTONSOFT_EXISTS
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace DingoUnityExtensions.Serialization
{
    public enum CacheOption
    {
        None,
        TryGetCachedValue,
        ForceUpdateCachedValue,
    }

    public static class SerializationUtils
    {
        private static class StaticCache<T>
        {
            public static readonly ConcurrentDictionary<string, T> CachedValues = new();
        }
        
        public static async Task<T> DeserializeFromFile<T>(string path, CacheOption cacheOption = CacheOption.None, bool catchException = true, T defaultValue = default, JsonSerializerSettings settings = null)
        {
            if (cacheOption == CacheOption.TryGetCachedValue && StaticCache<T>.CachedValues.TryGetValue(path, out var cachedValue))
                return cachedValue;
            
            if (!File.Exists(path))
                return defaultValue;

            string json;
            T value;
            var saveCache = false;
            if (catchException)
            {
                try
                {
                    json = await File.ReadAllTextAsync(path);
                    value = settings == null ? JsonConvert.DeserializeObject<T>(json) : JsonConvert.DeserializeObject<T>(json, settings);
                    saveCache = true;
                }
                catch (Exception e)
                {
                    Debug.LogError($"Cannot load and deserialize at: {path}");
                    Debug.LogException(e);
                    value = defaultValue;
                    saveCache = false;
                }
            }
            else
            {
                json = await File.ReadAllTextAsync(path);
                value = settings == null ? JsonConvert.DeserializeObject<T>(json) : JsonConvert.DeserializeObject<T>(json, settings);
                saveCache = true;
            }

            if (saveCache && (cacheOption == CacheOption.ForceUpdateCachedValue || cacheOption == CacheOption.TryGetCachedValue && !StaticCache<T>.CachedValues.ContainsKey(path)))
                StaticCache<T>.CachedValues.AddOrUpdate(path, value, (k, v) => value);

            return value;
        }
        
        public static T DeserializeOrDefault<T>(string key, string json, CacheOption cacheOption = CacheOption.None, bool catchException = true, T defaultValue = default, JsonSerializerSettings settings = null)
        {
            if (cacheOption == CacheOption.TryGetCachedValue && StaticCache<T>.CachedValues.TryGetValue(key, out var cachedValue))
                return cachedValue;
            
            T value;
            if (catchException)
            {
                try
                {
                    value = settings == null ? JsonConvert.DeserializeObject<T>(json) : JsonConvert.DeserializeObject<T>(json, settings);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    value = defaultValue;
                }
            }
            else
            {
                value = settings == null ? JsonConvert.DeserializeObject<T>(json) : JsonConvert.DeserializeObject<T>(json, settings);
            }

            if (cacheOption == CacheOption.ForceUpdateCachedValue || cacheOption == CacheOption.TryGetCachedValue && !StaticCache<T>.CachedValues.ContainsKey(key))
                StaticCache<T>.CachedValues.AddOrUpdate(key, value, (k, v) => value);

            return value;
        }
        
        public static T DeserializeOrDefault<T>(string json, bool catchException = true, T defaultValue = default, JsonSerializerSettings settings = null)
        {
            T value;
            if (catchException)
            {
                try
                {
                    value = settings == null ? JsonConvert.DeserializeObject<T>(json) : JsonConvert.DeserializeObject<T>(json, settings);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    value = defaultValue;
                }
            }
            else
            {
                value = settings == null ? JsonConvert.DeserializeObject<T>(json) : JsonConvert.DeserializeObject<T>(json, settings);
            }
            
            return value;
        }
    }
}

#endif