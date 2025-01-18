#if NEWTONSOFT_EXISTS
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
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

        public static async Task SaveSerializeAsync(string path, object obj, bool catchException = true, JsonSerializerSettings settings = null, CancellationTokenSource cancellationTokenSource = null)
        {
            string json;
            if (catchException)
            {
                try
                {
                    if (cancellationTokenSource != null)
                    {
                        json = await Task.Run(() => JsonConvert.SerializeObject(obj, settings), cancellationTokenSource.Token);
                        await File.WriteAllTextAsync(path, json, cancellationTokenSource.Token);
                    }
                    else
                    {
                        json = await Task.Run(() => JsonConvert.SerializeObject(obj, settings));
                        await File.WriteAllTextAsync(path, json);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
            else
            {
                if (cancellationTokenSource != null)
                {
                    json = await Task.Run(() => JsonConvert.SerializeObject(obj, settings), cancellationTokenSource.Token);
                    await File.WriteAllTextAsync(path, json, cancellationTokenSource.Token);
                }
                else
                {
                    json = await Task.Run(() => JsonConvert.SerializeObject(obj, settings));
                    await File.WriteAllTextAsync(path, json);
                }
            }
        }
        
        public static async Task<T> DeserializeFromFileAsync<T>(string path, CacheOption cacheOption = CacheOption.None, bool catchException = true, T defaultValue = default, JsonSerializerSettings settings = null, CancellationTokenSource cancellationTokenSource = null)
        {
            if (cacheOption == CacheOption.TryGetCachedValue && StaticCache<T>.CachedValues.TryGetValue(path, out var cachedValue))
                return cachedValue;
            
            if (!File.Exists(path))
                return defaultValue;

            string json;
            T value;
            if (catchException)
            {
                try
                {
                    if (cancellationTokenSource != null)
                    {
                        json = await File.ReadAllTextAsync(path, cancellationTokenSource.Token);
                        value = settings == null ? JsonConvert.DeserializeObject<T>(json) : JsonConvert.DeserializeObject<T>(json, settings);
                    }
                    else
                    {
                        json = await File.ReadAllTextAsync(path);
                        value = settings == null ? JsonConvert.DeserializeObject<T>(json) : JsonConvert.DeserializeObject<T>(json, settings);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    value = defaultValue;
                }
            }
            else
            {
                if (cancellationTokenSource != null)
                {
                    json = await File.ReadAllTextAsync(path, cancellationTokenSource.Token);
                    value = settings == null ? JsonConvert.DeserializeObject<T>(json) : JsonConvert.DeserializeObject<T>(json, settings);
                }
                else
                {
                    json = await File.ReadAllTextAsync(path);
                    value = settings == null ? JsonConvert.DeserializeObject<T>(json) : JsonConvert.DeserializeObject<T>(json, settings);
                }
            }

            if (cacheOption == CacheOption.ForceUpdateCachedValue || cacheOption == CacheOption.TryGetCachedValue && !StaticCache<T>.CachedValues.ContainsKey(path))
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