#if NEWTONSOFT_EXISTS
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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

    public enum CacheType
    {
        Queue,
        Stack,
        Random
    }

    public static class SerializationUtils
    {
        private static class StaticCache<T>
        {
            private const int MAX_ITERATIONS = 50;
            
            private static readonly ConcurrentDictionary<string, T> CachedValues = new();
            private static readonly ConcurrentQueue<string> Queue = new();
            private static readonly ConcurrentStack<string> Stack = new();
            private static readonly ConcurrentBag<string> Bag = new();
            
            private static int _capacity = -1;
            private static CacheType _cacheType = CacheType.Random;
            private static Action<T> _dispose;

            public static void Setup(int capacity, CacheType cacheType, Action<T> dispose)
            {
                _dispose = dispose;
                _cacheType = cacheType;
                _capacity = capacity;
            }

            public static bool TryGetCache(string key, out T value)
            {
                return CachedValues.TryGetValue(key, out value);
            }

            public static void AddCacheValue(string key, T value)
            {
                if (_capacity > 0)
                {
                    var iterations = MAX_ITERATIONS;
                    while (iterations >= 0 && CachedValues.Count >= _capacity)
                    {
                        iterations--;
                        switch (_cacheType)
                        {
                            case CacheType.Queue:
                                QueueBasedClear();
                                break;
                            case CacheType.Stack:
                                StackBasedClear();
                                break;
                            case CacheType.Random:
                                RandomBasedClear();
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                }

                CachedValues.AddOrUpdate(key, k => value, (k, v) =>
                {
                    _dispose?.Invoke(v);
                    return value;
                });
                switch (_cacheType)
                {
                    case CacheType.Queue:
                        Queue.Enqueue(key);
                        break;
                    case CacheType.Stack:
                        Stack.Push(key);
                        break;
                    case CacheType.Random:
                        Bag.Add(key);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            private static void RandomBasedClear()
            {
                if (Bag.TryTake(out var removeKey) && CachedValues.TryRemove(removeKey, out var valueToDispose))
                    _dispose?.Invoke(valueToDispose);
            }

            private static void QueueBasedClear()
            {
                if (Queue.TryDequeue(out var removeKey) && CachedValues.TryRemove(removeKey, out var valueToDispose))
                    _dispose?.Invoke(valueToDispose);
            }

            private static void StackBasedClear()
            {
                if (Stack.TryPop(out var removeKey) && CachedValues.TryRemove(removeKey, out var valueToDispose))
                    _dispose?.Invoke(valueToDispose);
            }
        }

        public static void SetupCache<T>(int capacity, CacheType cacheType, Action<T> dispose = null)
        {
            StaticCache<T>.Setup(capacity, cacheType, dispose);
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
            if (cacheOption == CacheOption.TryGetCachedValue && StaticCache<T>.TryGetCache(path, out var cachedValue))
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

            if (cacheOption == CacheOption.ForceUpdateCachedValue || cacheOption == CacheOption.TryGetCachedValue && !StaticCache<T>.TryGetCache(path, out _))
                StaticCache<T>.AddCacheValue(path, value);

            return value;
        }
        
        public static T DeserializeOrDefault<T>(string key, string json, CacheOption cacheOption = CacheOption.None, bool catchException = true, T defaultValue = default, JsonSerializerSettings settings = null)
        {
            if (cacheOption == CacheOption.TryGetCachedValue && StaticCache<T>.TryGetCache(key, out var cachedValue))
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

            if (cacheOption == CacheOption.ForceUpdateCachedValue || cacheOption == CacheOption.TryGetCachedValue && !StaticCache<T>.TryGetCache(key, out _))
                StaticCache<T>.AddCacheValue(key, value);

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