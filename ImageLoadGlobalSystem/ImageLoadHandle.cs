using System;
using System.Collections;
using System.Collections.Generic;
using Bind;
using Cysharp.Threading.Tasks;
using DingoUnityExtensions.Utils;
using UnityEngine;
using UnityEngine.Networking;
using Object = UnityEngine.Object;

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
            if (!DependObjects.TryGetValue(path, out var hash))
            {
                hash = new HashSet<object>();
                DependObjects.Add(path, hash);
            }

            DependPath.TryAdd(obj, path);

            hash.Add(obj);
        }

        public static void UnLink(object obj)
        {
            if (!DependPath.TryGetValue(obj, out var path))
                return;
            var v = TextureFlows[path].V;
            DependPath.Remove(obj);
            if (DependObjects.TryGetValue(path, out var dependObjects))
                dependObjects.Remove(obj);
            if (dependObjects.Count != 0)
                return;
            
            if (v.State == ImageLoadState.Loaded && v.Texture != null)
                Object.Destroy(v.Texture);
            else if (v.State == ImageLoadState.Loading)
                CoroutineParent.CancelCoroutine(obj);
            
            TextureFlows[path].V = new TextureLoadData(null, ImageLoadState.None, path);
        }
    }
    
    public class ImageLoadHandle
    {
        public string Path { get; private set; }

        private readonly Bind<TextureLoadData> _textureFlowWrapper = new();
        private Bind<TextureLoadData> _textureFlow;
        
        public IReadonlyBind<TextureLoadData> TextureFlow => _textureFlowWrapper;

        public ImageLoadHandle()
        {
        }
        
        public ImageLoadHandle(string path)
        {
            Path = path;
            _textureFlow = ImageLoadGlobalCache.GetOrRegister(Path);
            _textureFlow.SafeSubscribe(ChangeData);
        }
        
        public void LoadPath(object receiver)
        {
            if (Path == null)
            {
                Debug.LogErrorFormat($"Cannot load image path == null");
                return;
            }
            CoroutineParent.StartCoroutineWithCanceling(receiver, () => LoadImageCoroutine(_textureFlow, receiver, Path));
        }
        
        public void ChangePath(object receiver, string path)
        {
            if (path == Path)
                return;
            
            Unload(receiver);
            _textureFlow.UnSubscribe(ChangeData);
            _textureFlow = ImageLoadGlobalCache.GetOrRegister(path);
            _textureFlow.SafeSubscribe(ChangeData);
            Path = path;
        }
        
        public void Unload(object receiver)
        {
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
            var prevTexture = bind.V.Texture;
            if (prevTexture != null)
                ImageLoadGlobalCache.UnLink(receiver);
            
            yield return MultiplatformLoadUtils.LoadTexture2DAsync(path)
                .AsUniTask()
                .ToCoroutine(t =>
                {
                    if (t == null)
                        bind.V = new TextureLoadData(null, ImageLoadState.NotFound, path);
                    else
                        bind.V = new TextureLoadData(t, ImageLoadState.Loaded, path);
                });
        }
        
        private void ChangeData(TextureLoadData textureLoadData) => _textureFlowWrapper.V = textureLoadData;
    }
}