using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Collections;
using UnityEngine;

namespace DingoUnityExtensions.Performance.Pools
{
    public static class NativeArrayPool<T> where T : struct
    {
        private class NativeArrayWrapper : IDisposable
        {
            public readonly ulong Id;
            public NativeArray<T> NativeArray;
            private long _lockFlags;
            private readonly object _lock = new();
            
            public bool IsUnLocked => _lockFlags == 0;

            public void LockByFlag(ushort flag)
            {
                lock (_lock)
                {
                    _lockFlags |= (uint)(1 << flag);
                }
            }

            public bool UnlockByFlag(ushort flag)
            {
                lock (_lock)
                {
                    var prevLocked = _lockFlags;
                    _lockFlags &= ~(uint)(1 << flag);
                    return prevLocked != 0 && _lockFlags == 0;
                }
            }

            public void Unlock()
            {
                lock (_lock)
                {
                    _lockFlags = 0;
                }
            }

            public NativeArrayWrapper(ulong id, NativeArray<T> nativeArray)
            {
                Id = id;
                NativeArray = nativeArray;
            }
            
            public void Dispose()
            {
                if (NativeArray.IsCreated)
                {
                    NativeArray.Dispose();
                }
            }

            public override string ToString() => _lockFlags.ToString();
        }

        private static ulong _lastId;
        private static readonly ConcurrentDictionary<ulong, NativeArrayWrapper> Pooled = new();
        private static readonly ConcurrentDictionary<int, ConcurrentStack<NativeArrayWrapper>> Free = new();
        
        public static NativeArray<T> PullNativeArray(int length, out ulong id, ushort flag = 0)
        {
            NativeArrayWrapper wrapper;
            if (Free.TryGetValue(length, out var wrappers))
            {
                while (wrappers.Count > 0)
                {
                    if (!wrappers.TryPop(out wrapper))
                        continue;
                    if (!wrapper.IsUnLocked)
                        continue;
                    id = wrapper.Id;
                    wrapper.LockByFlag(flag);
                    return wrapper.NativeArray;
                }
            }
            Free[length] = new ConcurrentStack<NativeArrayWrapper>();
            id = ++_lastId;
            var nativeArray = new NativeArray<T>(length, Allocator.Persistent);
            wrapper = new NativeArrayWrapper(id, nativeArray);
            wrapper.LockByFlag(flag);
            Pooled[id] = wrapper;
            return wrapper.NativeArray;
        }

        public static void PushNativeArray(ulong id, ushort flag = 0)
        {
            if (id == 0)
                return;

            if (!Pooled.TryGetValue(id, out var wrapper))
            {
                Debug.LogException(new KeyNotFoundException($"{nameof(id)}: {id}"));
            }
            else
            {
                var isUnlocked = wrapper.UnlockByFlag(flag);
                if (isUnlocked)
                    Free[wrapper.NativeArray.Length].Push(wrapper);
            }
        }

        public static void Lock(ulong id, ushort flag = 0)
        {
            if (!Pooled.TryGetValue(id, out var wrapper))
                return;
            wrapper.LockByFlag(flag);
        }

        public static void UnlockAll()
        {
            foreach (var (key, wrapper) in Pooled)
            {
                wrapper.Unlock();
            }
        }
        public static string GetToStringData()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"{nameof(NativeArrayPool<T>)}<{typeof(T).Name}>: {Pooled.Count}");
            sb.AppendLine($"{Free.Values.Sum(e => e.Count)} / {Pooled.Count}");

            return sb.ToString();
        }
    }
}