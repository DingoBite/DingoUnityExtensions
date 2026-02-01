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
            private const int MAX_EVICT_ATTEMPTS = 256;

            private readonly struct FileStamp : IEquatable<FileStamp>
            {
                public readonly long Length;
                public readonly long LastWriteTicksUtc;

                public FileStamp(long length, long lastWriteTicksUtc)
                {
                    Length = length;
                    LastWriteTicksUtc = lastWriteTicksUtc;
                }

                public bool Equals(FileStamp other) =>
                    Length == other.Length && LastWriteTicksUtc == other.LastWriteTicksUtc;

                public override bool Equals(object obj) => obj is FileStamp other && Equals(other);
                public override int GetHashCode() => HashCode.Combine(Length, LastWriteTicksUtc);
            }

            private readonly struct CacheEntry
            {
                public readonly T Value;
                public readonly bool HasStamp;
                public readonly FileStamp Stamp;

                public CacheEntry(T value)
                {
                    Value = value;
                    HasStamp = false;
                    Stamp = default;
                }

                public CacheEntry(T value, FileStamp stamp)
                {
                    Value = value;
                    HasStamp = true;
                    Stamp = stamp;
                }
            }

            private static readonly ConcurrentDictionary<string, CacheEntry> CachedValues = new();

            private static readonly ConcurrentDictionary<string, byte> EvictionKeys = new();

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
                if (CachedValues.TryGetValue(key, out var entry))
                {
                    value = entry.Value;
                    return true;
                }

                value = default;
                return false;
            }
            
            public static bool TryGetCacheIfFileUnchanged(string fullPathKey, out T value)
            {
                value = default;

                if (!CachedValues.TryGetValue(fullPathKey, out var entry))
                    return false;

                if (!entry.HasStamp)
                {
                    value = entry.Value;
                    return true;
                }

                if (!TryGetFileStamp(fullPathKey, out var current))
                {
                    Remove(fullPathKey);
                    return false;
                }

                if (!entry.Stamp.Equals(current))
                {
                    Remove(fullPathKey);
                    return false;
                }

                value = entry.Value;
                return true;
            }

            public static void AddOrUpdate(string key, T value)
            {
                AddOrUpdateInternal(key, new CacheEntry(value));
            }

            public static void AddOrUpdateFromFile(string fullPathKey, T value)
            {
                if (TryGetFileStamp(fullPathKey, out var stamp))
                    AddOrUpdateInternal(fullPathKey, new CacheEntry(value, stamp));
                else
                    AddOrUpdateInternal(fullPathKey, new CacheEntry(value));
            }

            private static void AddOrUpdateInternal(string key, CacheEntry entry)
            {
                EnsureCapacity();

                CachedValues.AddOrUpdate(key, _ => entry, (_, old) =>
                {
                    _dispose?.Invoke(old.Value);
                    return entry;
                });

                if (EvictionKeys.TryAdd(key, 0))
                {
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
                        default: throw new ArgumentOutOfRangeException();
                    }
                }
            }

            private static void EnsureCapacity()
            {
                if (_capacity <= 0)
                    return;

                var attempts = MAX_EVICT_ATTEMPTS;
                while (attempts-- > 0 && CachedValues.Count >= _capacity)
                {
                    if (!TryEvictOne())
                        break;
                }
            }

            private static bool TryEvictOne()
            {
                string key;

                switch (_cacheType)
                {
                    case CacheType.Queue:
                        while (Queue.TryDequeue(out key))
                        {
                            if (Remove(key))
                                return true;
                        }

                        return false;

                    case CacheType.Stack:
                        while (Stack.TryPop(out key))
                        {
                            if (Remove(key))
                                return true;
                        }

                        return false;

                    case CacheType.Random:
                        while (Bag.TryTake(out key))
                        {
                            if (Remove(key))
                                return true;
                        }

                        return false;

                    default: throw new ArgumentOutOfRangeException();
                }
            }

            private static bool Remove(string key)
            {
                if (!CachedValues.TryRemove(key, out var removed))
                    return false;

                EvictionKeys.TryRemove(key, out _);

                _dispose?.Invoke(removed.Value);
                return true;
            }

            private static bool TryGetFileStamp(string fullPath, out FileStamp stamp)
            {
                stamp = default;
                try
                {
                    var fi = new FileInfo(fullPath);
                    if (!fi.Exists)
                        return false;

                    stamp = new FileStamp(fi.Length, fi.LastWriteTimeUtc.Ticks);
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            public static void ClearAll()
            {
                Bag.Clear();
                Stack.Clear();
                Queue.Clear();
                EvictionKeys.Clear();

                foreach (var kv in CachedValues)
                {
                    _dispose?.Invoke(kv.Value.Value);
                }

                CachedValues.Clear();
            }
        }

        public static void SetupCache<T>(int capacity, CacheType cacheType, Action<T> dispose = null)
        {
            StaticCache<T>.Setup(capacity, cacheType, dispose);
        }

        public static async Task SaveAsync(string path, string json, bool catchException = true, CancellationTokenSource cancellationTokenSource = null)
        {
            var fullPath = NormalizePathKey(path);
            var token = cancellationTokenSource?.Token ?? CancellationToken.None;

            if (catchException)
            {
                try
                {
                    await File.WriteAllTextAsync(fullPath, json, token);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
            else
            {
                await File.WriteAllTextAsync(fullPath, json, token);
            }
        }

        public static async Task SaveSerializeAsync(string path, object obj, bool catchException = true, JsonSerializerSettings settings = null, CancellationTokenSource cancellationTokenSource = null)
        {
            var fullPath = NormalizePathKey(path);
            var token = cancellationTokenSource?.Token ?? CancellationToken.None;

            if (catchException)
            {
                try
                {
                    var json = await Task.Run(() => JsonConvert.SerializeObject(obj, settings), token);
                    await File.WriteAllTextAsync(fullPath, json, token);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
            else
            {
                var json = await Task.Run(() => JsonConvert.SerializeObject(obj, settings), token);
                await File.WriteAllTextAsync(fullPath, json, token);
            }
        }

        public static async Task<T> DeserializeFromFileAsync<T>(string path, CacheOption cacheOption = CacheOption.None, bool catchException = true, T defaultValue = default, JsonSerializerSettings settings = null, CancellationTokenSource cancellationTokenSource = null)
        {
            var fullPath = NormalizePathKey(path);
            var token = cancellationTokenSource?.Token ?? CancellationToken.None;

            if (cacheOption == CacheOption.TryGetCachedValue && StaticCache<T>.TryGetCacheIfFileUnchanged(fullPath, out var cached))
                return cached;

            if (!File.Exists(fullPath))
                return defaultValue;

            string json;
            T value;

            if (catchException)
            {
                try
                {
                    json = await File.ReadAllTextAsync(fullPath, token);
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
                json = await File.ReadAllTextAsync(fullPath, token);
                value = settings == null ? JsonConvert.DeserializeObject<T>(json) : JsonConvert.DeserializeObject<T>(json, settings);
            }

            if (cacheOption is CacheOption.ForceUpdateCachedValue or CacheOption.TryGetCachedValue)
                StaticCache<T>.AddOrUpdateFromFile(fullPath, value);

            return value;
        }
        
        public static void Replace(string path1, string path2, string bak)
        {
            var p1 = NormalizePathKey(path1);
            var p2 = NormalizePathKey(path2);
            var pb = NormalizePathKey(bak);

            if (File.Exists(p1))
            {
                try
                {
                    File.Replace(p2, p1, pb, true);
                }
                catch
                {
                    if (File.Exists(p2))
                    {
                        if (File.Exists(p1))
                            File.Delete(p1);
                        File.Move(p2, p1);
                    }
                }
            }
            else
            {
                File.Move(p2, p1);
            }
        }

        public static T DeserializeOrDefault<T>(string key, string json, CacheOption cacheOption = CacheOption.None, bool catchException = true, T defaultValue = default, JsonSerializerSettings settings = null)
        {
            if (cacheOption == CacheOption.TryGetCachedValue && StaticCache<T>.TryGetCache(key, out var cached))
                return cached;

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

            if (cacheOption == CacheOption.ForceUpdateCachedValue || (cacheOption == CacheOption.TryGetCachedValue && !StaticCache<T>.TryGetCache(key, out _)))
            {
                StaticCache<T>.AddOrUpdate(key, value);
            }

            return value;
        }

        public static T DeserializeOrDefault<T>(string json, bool catchException = true, T defaultValue = default, JsonSerializerSettings settings = null)
        {
            if (catchException)
            {
                try
                {
                    return settings == null ? JsonConvert.DeserializeObject<T>(json) : JsonConvert.DeserializeObject<T>(json, settings);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    return defaultValue;
                }
            }

            return settings == null ? JsonConvert.DeserializeObject<T>(json) : JsonConvert.DeserializeObject<T>(json, settings);
        }

        public static void ClearAll<T>() => StaticCache<T>.ClearAll();

        private static string NormalizePathKey(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return string.Empty;

            if (Uri.TryCreate(path, UriKind.Absolute, out var uri) && !uri.IsFile)
                return path;

            try
            {
                return Path.GetFullPath(path);
            }
            catch
            {
                return path;
            }
        }
    }
}
#endif