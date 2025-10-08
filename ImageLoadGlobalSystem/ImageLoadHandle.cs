using System;
using System.Collections;
using System.Collections.Generic;
using Bind;
using Cysharp.Threading.Tasks;
using DingoUnityExtensions.Utils;
using UnityEngine;
using UnityEngine.Networking;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace DingoUnityExtensions.ImageLoadGlobalSystem
{
    public enum ImageLoadState
    {
        None,
        Loading,
        NotFound,
        Loaded,
    }

    public struct TextureLoadData
    {
        public readonly string Path;
        public readonly Texture2D Texture;
        public readonly ImageLoadState State;

        public TextureLoadData(Texture2D texture, ImageLoadState state, string path)
        {
            Texture = texture;
            State = state;
            Path = path;
        }

        public static TextureLoadData None => new(null, ImageLoadState.None, null);
        public static TextureLoadData NotFound => new(null, ImageLoadState.NotFound, null);
    }

    public static class ImageLoadGlobalCache
    {
        private static readonly Dictionary<string, Bind<TextureLoadData>> TextureFlows = new();
        private static readonly Dictionary<string, HashSet<object>> DependObjects = new();
        private static readonly Dictionary<object, string> DependPath = new();
        
        public static Bind<TextureLoadData> GetOrRegister(string path)
        {
            if (!TextureFlows.TryGetValue(path, out var bind))
            {
                bind = new Bind<TextureLoadData>();
                TextureFlows.Add(path, bind);
            }

            return bind;
        }

        public static void Link(string path, object obj)
        {
            if (!DependObjects.TryGetValue(path, out var dependObjects))
            {
                dependObjects = new HashSet<object>();
                DependObjects.Add(path, dependObjects);
            }

            DependPath.TryAdd(obj, path);
            dependObjects.Add(obj);
        }

        public static void UnLink(object obj)
        {
            if (!DependPath.Remove(obj, out var path))
                return;
            var textureFlow = TextureFlows[path];
            if (DependObjects.TryGetValue(path, out var dependObjects))
                dependObjects.Remove(obj);
            if (dependObjects != null && dependObjects.Count != 0)
                return;
            
            if (textureFlow.V.State == ImageLoadState.Loaded && textureFlow.V.Texture != null)
                Object.Destroy(textureFlow.V.Texture);

            textureFlow.V = TextureLoadData.None;
        }
    }
    
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
                Debug.LogErrorFormat($"Cannot load image path == null");
                return;
            }

            UnloadFor(receiver);
            CoroutineParent.StartCoroutineWithCanceling((receiver, IMAGE_LOAD_HANDLE), LoadImageCoroutine(_textureFlow, receiver, Path));
        }

        public void UnloadFor(object receiver)
        {
            CoroutineParent.CancelCoroutine((receiver, IMAGE_LOAD_HANDLE));
            ImageLoadGlobalCache.UnLink(receiver);
        }
        
        private IEnumerator LoadImageCoroutine(IValueContainer<TextureLoadData> bind, object receiver, string path)
        {
            ImageLoadGlobalCache.Link(path, receiver);
            if (path == Path && bind.V.State != ImageLoadState.None)
            {
                bind.V = bind.V;
                yield break;
            }

            bind.V = new TextureLoadData(null, ImageLoadState.Loading, path);
#if UNITY_EDITOR && EMULATE_DELAYS
            yield return CoroutineParent.CachedWaiter((int)(Random.value * 3f));
#endif
            yield return MultiplatformLoadUtils.LoadTexture2DAsync(path, _disableLogException)
                .AsUniTask()
                .ToCoroutine(t =>
                {
                    if (t == null)
                        bind.V = new TextureLoadData(null, ImageLoadState.NotFound, path);
                    else
                        bind.V = new TextureLoadData(t, ImageLoadState.Loaded, path);
                });
        }
    }
}