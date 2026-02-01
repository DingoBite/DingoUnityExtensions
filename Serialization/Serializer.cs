#if NEWTONSOFT_EXISTS
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace DingoUnityExtensions.Serialization
{
    public interface ISerializer
    {
        public Task<T> LoadDeserializeAsync<T>(string uri, bool catchExceptions = true, bool cache = false, bool forceReload = false, CancellationTokenSource cancellationTokenSource = null, JsonSerializerSettings jsonSerializerSettings = null);
        public Task SaveSerializeAsync(string uri, object obj, bool catchExceptions = true, CancellationTokenSource cancellationTokenSource = null, JsonSerializerSettings jsonSerializerSettings = null, bool bakFile = false);
        public Task SaveAsync(string uri, string json, bool catchExceptions = true, CancellationTokenSource cancellationTokenSource = null, bool bakFile = false);
        public void ClearAll<T>();
    }

    public class SemaphoreSerializer : ISerializer
    {
        private readonly JsonSerializerSettings _fallbackSettings;
        
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> SaveLocks = new();
        
        public SemaphoreSerializer() { }
        
        public SemaphoreSerializer(JsonSerializerSettings fallbackSettings)
        {
            _fallbackSettings = fallbackSettings;
        }

        private static SemaphoreSlim GetSaveLock(string key) => SaveLocks.GetOrAdd(key ?? string.Empty, _ => new SemaphoreSlim(1, 1));

        public async Task<T> LoadDeserializeAsync<T>(string uri, bool catchExceptions = true, bool cache = false, bool forceReload = false, CancellationTokenSource cancellationTokenSource = null, JsonSerializerSettings jsonSerializerSettings = null)
        {
            var cacheOption = CacheOption.None;
            if (forceReload)
                cacheOption = CacheOption.ForceUpdateCachedValue;
            else if (cache)
                cacheOption = CacheOption.TryGetCachedValue;

            return await SerializationUtils.DeserializeFromFileAsync<T>(uri, cacheOption, catchExceptions, settings: jsonSerializerSettings ?? _fallbackSettings, cancellationTokenSource: cancellationTokenSource);
        }

        public async Task SaveSerializeAsync(string uri, object obj, bool catchExceptions = true, CancellationTokenSource cancellationTokenSource = null, JsonSerializerSettings jsonSerializerSettings = null, bool bakFile = false)
        {
            var token = cancellationTokenSource?.Token ?? CancellationToken.None;

            var gate = GetSaveLock(uri);
            await gate.WaitAsync(token);
            try
            {
                if (bakFile)
                {
                    var tmp = uri + ".tmp";
                    var bak = uri + ".bak";
                    await SerializationUtils.SaveSerializeAsync(tmp, obj, catchExceptions, jsonSerializerSettings ?? _fallbackSettings, cancellationTokenSource: cancellationTokenSource);
                    SerializationUtils.Replace(tmp, uri, bak);
                }
                else
                {
                    await SerializationUtils.SaveSerializeAsync(uri, obj, catchExceptions, jsonSerializerSettings ?? _fallbackSettings, cancellationTokenSource: cancellationTokenSource);
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                gate.Release();
            }
        }

        public async Task SaveAsync(string uri, string json, bool catchExceptions = true, CancellationTokenSource cancellationTokenSource = null, bool bakFile = false)
        {
            var token = cancellationTokenSource?.Token ?? CancellationToken.None;

            var gate = GetSaveLock(uri);
            await gate.WaitAsync(token);
            try
            {
                if (bakFile)
                {
                    var tmp = uri + ".tmp";
                    var bak = uri + ".bak";
                    await SerializationUtils.SaveAsync(tmp, json, catchExceptions, cancellationTokenSource: cancellationTokenSource);
                    SerializationUtils.Replace(tmp, uri, bak);
                }
                else
                {
                    await SerializationUtils.SaveAsync(uri, json, catchExceptions, cancellationTokenSource: cancellationTokenSource);
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                gate.Release();
            }
        }

        public void ClearAll<T>()
        {
            SerializationUtils.ClearAll<T>();
        }
    }
}
#endif