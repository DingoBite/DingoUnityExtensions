using System;
using System.Collections;
using System.Collections.Generic;
using Bind;
using Cysharp.Threading.Tasks;
using DingoUnityExtensions.Utils;
using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_EDITOR && EMULATE_DELAYS
using Random = UnityEngine.Random;
#endif

namespace DingoUnityExtensions.ImageLoadGlobalSystem
{
    public static class ImageLoadGlobalCache
    {
        private const string LOAD_COROUTINE = "IMAGE_LOAD_GLOBAL";

        private sealed class CacheEntry
        {
            public readonly Bind<TextureLoadData> Flow = new();
            public readonly HashSet<object> Receivers = new();

            public int Generation;
        }

        private static readonly Dictionary<string, CacheEntry> Entries = new();
        private static readonly Dictionary<object, string> ReceiverToPath = new();

        public static Bind<TextureLoadData> GetOrRegister(string path)
        {
            if (!Entries.TryGetValue(path, out var entry))
            {
                entry = new CacheEntry();
                Entries.Add(path, entry);
            }

            return entry.Flow;
        }

        public static void Link(string path, object receiver, bool disableLogException = false)
        {
            if (string.IsNullOrWhiteSpace(path) || receiver == null)
                return;

            UnLink(receiver);
            var entry = GetOrCreateEntry(path);
            CleanupDeadReceivers(entry);

            entry.Receivers.Add(receiver);
            ReceiverToPath[receiver] = path;

            entry.Flow.V = entry.Flow.V;

            if (entry.Flow.V.State == ImageLoadState.None)
                StartLoad(entry, path, disableLogException);
        }

        public static void UnLink(object receiver)
        {
            if (receiver == null)
                return;

            if (!ReceiverToPath.Remove(receiver, out var path))
                return;

            if (!Entries.TryGetValue(path, out var entry))
                return;

            entry.Receivers.Remove(receiver);
            CleanupDeadReceivers(entry);

            if (entry.Receivers.Count > 0)
                return;

            entry.Generation++;
            CoroutineParent.CancelCoroutine((entry, LOAD_COROUTINE));

            if (entry.Flow.V.State == ImageLoadState.Loaded && entry.Flow.V.Texture != null)
                Object.Destroy(entry.Flow.V.Texture);

            entry.Flow.V = TextureLoadData.None;

            Entries.Remove(path);
        }

        private static CacheEntry GetOrCreateEntry(string path)
        {
            if (!Entries.TryGetValue(path, out var entry))
            {
                entry = new CacheEntry();
                Entries.Add(path, entry);
            }

            return entry;
        }

        private static void StartLoad(CacheEntry entry, string path, bool disableLogException)
        {
            entry.Generation++;
            var gen = entry.Generation;

            entry.Flow.V = new TextureLoadData(null, ImageLoadState.Loading, path);

            CoroutineParent.StartCoroutineWithCanceling((entry, LOAD_COROUTINE), LoadImageCoroutine(entry, gen, path, disableLogException));
        }

        private static IEnumerator LoadImageCoroutine(CacheEntry entry, int gen, string path, bool disableLogException)
        {
#if UNITY_EDITOR && EMULATE_DELAYS
            yield return CoroutineParent.CachedWaiter((int)(Random.value * 3f));
#endif
            yield return MultiplatformLoadUtils.LoadTexture2DAsync(path, disableLogException).AsUniTask().ToCoroutine(t =>
            {
                if (entry.Generation != gen || entry.Receivers.Count == 0)
                {
                    if (t != null)
                        Object.Destroy(t);

                    return;
                }

                if (t == null)
                    entry.Flow.V = new TextureLoadData(null, ImageLoadState.NotFound, path);
                else
                    entry.Flow.V = new TextureLoadData(t, ImageLoadState.Loaded, path);
            });
        }

        private static void CleanupDeadReceivers(CacheEntry entry)
        {
            if (entry.Receivers.Count == 0)
                return;

            List<object> dead = null;

            foreach (var r in entry.Receivers)
            {
                if (r is Object uo && uo == null)
                {
                    dead ??= new List<object>();
                    dead.Add(r);
                }
            }

            if (dead == null)
                return;

            foreach (var r in dead)
            {
                entry.Receivers.Remove(r);
                ReceiverToPath.Remove(r);
            }
        }
    }

    [Obsolete("Use Texture2DLoadHandle")]
    public class ImageLoadHandle
    {
        public const string IMAGE_LOAD_HANDLE = "IMAGE_LOAD_HANDLE";
        public string Path { get; private set; }

        private readonly Bind<TextureLoadData> _textureFlow;
        private readonly bool _disableLogException;
        public IReadonlyBind<TextureLoadData> TextureFlow => _textureFlow;

        public ImageLoadHandle(string path, bool disableLogException = false)
        {
            _disableLogException = disableLogException;
            Path = path;
            if (Path == null)
                return;

            _textureFlow = ImageLoadGlobalCache.GetOrRegister(Path);
        }

        public void LoadFor(object receiver)
        {
            if (string.IsNullOrWhiteSpace(Path))
            {
                Debug.LogErrorFormat("Cannot load image path == null/empty");
                return;
            }

            ImageLoadGlobalCache.Link(Path, receiver, _disableLogException);
        }

        public void UnloadFor(object receiver)
        {
            ImageLoadGlobalCache.UnLink(receiver);
        }
    }
}