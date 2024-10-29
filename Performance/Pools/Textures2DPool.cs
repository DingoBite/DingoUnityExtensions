using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DingoUnityExtensions.Performance.Pools
{
    public static class Textures2DPool
    {
        private class Texture2DWrapper : IDisposable
        {
            public (int, int, TextureFormat) Key { get; private set; }
            public readonly ulong Id;
            public readonly Texture2D Texture;
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

            public void ReinitializeKey((int, int, TextureFormat) key)
            {
                lock (_lock)
                {
                    Key = key;
                }
            }

            private Texture2DWrapper(ulong id, (int, int, TextureFormat) key, Texture2D texture)
            {
                Key = key;
                Texture = texture;
                Id = id;
            }

            public static Texture2DWrapper Wrap(ulong id, Texture2D texture)
            {
                return new Texture2DWrapper(id, (texture.width, texture.height, texture.format), texture);
            }

            public static Texture2DWrapper WrapThreadSafe(ulong id, int width, int height, Texture2D texture, TextureFormat format)
            {
                return new Texture2DWrapper(id, (width, height, format), texture);
            }
            
            public static Texture2DWrapper WrapThreadSafe(ulong id, (int, int, TextureFormat) key, Texture2D texture)
            {
                return new Texture2DWrapper(id, key, texture);
            }

            public void Dispose()
            {
                Object.Destroy(Texture);
            }

            public override string ToString() => _lockFlags.ToString();
        }

        private static ulong _lastId;
        private static readonly ConcurrentDictionary<ulong, Texture2DWrapper> Pooled = new();
        private static readonly ConcurrentDictionary<(int, int, TextureFormat), ConcurrentStack<Texture2DWrapper>> Free = new();

        /// <summary>
        /// Not thread safe
        /// </summary>
        /// <returns></returns>
        public static Texture2D PullTexture(int width, int height, TextureFormat textureFormat, out ulong id, ushort flag = 0, Func<Texture2D, (int, int, TextureFormat)> imageLoadKeyFactory = null)
        {
            var keyTuple = (width, height, textureFormat);
            Texture2DWrapper wrapper;
            if (Free.TryGetValue(keyTuple, out var wrappers))
            {
                while (wrappers.Count > 0)
                {
                    if (!wrappers.TryPop(out wrapper))
                        continue;
                    if (!wrapper.IsUnLocked)
                        continue;
                    id = wrapper.Id;
                    wrapper.LockByFlag(flag);
                    if (imageLoadKeyFactory != null)
                        keyTuple = imageLoadKeyFactory.Invoke(wrapper.Texture);

                    wrapper.ReinitializeKey(keyTuple);
                    return wrapper.Texture;
                }
            }

            Free[keyTuple] = new ConcurrentStack<Texture2DWrapper>();
            id = ++_lastId;
            var texture = new Texture2D(width, height, textureFormat, false);
            if (imageLoadKeyFactory != null)
                keyTuple = imageLoadKeyFactory(texture);
            wrapper = Texture2DWrapper.WrapThreadSafe(id, keyTuple, texture);
            wrapper.LockByFlag(flag);
            Pooled[id] = wrapper;

            return wrapper.Texture;
        }

        public static void PushTextureThreadSafe(ulong id, ushort flag = 0)
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
                    Free[wrapper.Key].Push(wrapper);
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
            sb.AppendLine($"{nameof(Textures2DPool)}: {Pooled.Count}");
            sb.AppendLine($"{Free.ToArray().SelectMany(c => c.Value.Where(e => e.IsUnLocked)).Count()} / {Pooled.Count}");
            return sb.ToString();
        }

        public static void Dispose()
        {
            foreach (var (key, wrapper) in Pooled)
            {
                wrapper.Dispose();
            }

            _lastId = 0;
            Pooled.Clear();
            Free.Clear();
        }
    }
}