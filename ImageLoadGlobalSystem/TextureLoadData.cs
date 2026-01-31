using UnityEngine;

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
}