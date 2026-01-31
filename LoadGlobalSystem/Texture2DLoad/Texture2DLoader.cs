using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Object = UnityEngine.Object;

namespace DingoUnityExtensions.LoadGlobalSystem.Texture2DLoad
{
    public readonly struct Texture2DLoadInfo : IEquatable<Texture2DLoadInfo>
    {
        public readonly bool Readable;
        public readonly bool MipmapChain;
        public readonly bool LinearColorSpace;

#if UNITY_6000_0_OR_NEWER
        public readonly int MipmapCount;
#endif

        public Texture2DLoadInfo(bool readable = false, bool mipmapChain = false, bool linearColorSpace = false
#if UNITY_6000_0_OR_NEWER
            , int mipmapCount = 0
#endif
        )
        {
            Readable = readable;
            MipmapChain = mipmapChain;
            LinearColorSpace = linearColorSpace;
#if UNITY_6000_0_OR_NEWER
            MipmapCount = mipmapCount;
#endif
        }

        public bool Equals(Texture2DLoadInfo other)
        {
#if UNITY_6000_0_OR_NEWER
            return Readable == other.Readable && MipmapChain == other.MipmapChain && LinearColorSpace == other.LinearColorSpace && MipmapCount == other.MipmapCount;
#else
            return Readable == other.Readable &&
                   MipmapChain == other.MipmapChain &&
                   LinearColorSpace == other.LinearColorSpace;
#endif
        }

        public override bool Equals(object obj) => obj is Texture2DLoadInfo other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                int h = 17;
                h = (h * 31) ^ Readable.GetHashCode();
                h = (h * 31) ^ MipmapChain.GetHashCode();
                h = (h * 31) ^ LinearColorSpace.GetHashCode();
#if UNITY_6000_0_OR_NEWER
                h = (h * 31) ^ MipmapCount.GetHashCode();
#endif
                return h;
            }
        }
    }

    public readonly struct Texture2DCacheKey : IEquatable<Texture2DCacheKey>
    {
        public readonly string Path;
        public readonly Texture2DLoadInfo Info;

        public Texture2DCacheKey(string path, Texture2DLoadInfo info)
        {
            Path = path;
            Info = info;
        }

        public bool Equals(Texture2DCacheKey other) => string.Equals(Path, other.Path, StringComparison.Ordinal) && Info.Equals(other.Info);

        public override bool Equals(object obj) => obj is Texture2DCacheKey other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                int h = 17;
                h = (h * 31) ^ (Path != null ? StringComparer.Ordinal.GetHashCode(Path) : 0);
                h = (h * 31) ^ Info.GetHashCode();
                return h;
            }
        }
    }

    public sealed class Texture2DKeyFactory : ICacheKeyFactory<Texture2DCacheKey, Texture2DLoadInfo>
    {
        public Texture2DCacheKey CreateKey(string path, Texture2DLoadInfo info) => new(path, info);
    }

    public sealed class Texture2DLoader : IAssetLoader<Texture2D, Texture2DLoadInfo>
    {
        public async UniTask<Texture2D> LoadAsync(string path, Texture2DLoadInfo info, CancellationToken ct)
        {
            var uri = ResolveToUri(path);

            using var uwr = CreateRequest(uri, info);
            var op = uwr.SendWebRequest();
            await op.ToUniTask(cancellationToken: ct);

            if (uwr.result == UnityWebRequest.Result.Success)
                return DownloadHandlerTexture.GetContent(uwr);

            if (uwr.result == UnityWebRequest.Result.ProtocolError && uwr.responseCode == 404)
                return null;

            if (uwr.result == UnityWebRequest.Result.ConnectionError)
                throw new Exception($"Texture load failed (connection error): {uwr.error}");

            if (uwr.result == UnityWebRequest.Result.ProtocolError)
                throw new Exception($"Texture load failed (HTTP {(int)uwr.responseCode}): {uwr.error}");

            throw new Exception($"Texture load failed ({uwr.result}): {uwr.error}");
        }

        private static UnityWebRequest CreateRequest(Uri uri, Texture2DLoadInfo info)
        {
#if UNITY_6000_0_OR_NEWER
            var p = DownloadedTextureParams.Default;
            p.readable = info.Readable;
            p.mipmapChain = info.MipmapChain;
            p.linearColorSpace = info.LinearColorSpace;
            if (info.MipmapCount > 0)
                p.mipmapCount = info.MipmapCount;

            return UnityWebRequestTexture.GetTexture(uri, p);
#else
            return UnityWebRequestTexture.GetTexture(uri, nonReadable: !info.Readable);
#endif
        }

        private static Uri ResolveToUri(string path)
        {
            if (Uri.TryCreate(path, UriKind.Absolute, out var uri))
                return uri;

            var fullPath = System.IO.Path.GetFullPath(path);
            return new Uri(fullPath);
        }
    }

    public sealed class Texture2DReleaser : IAssetReleaser<Texture2D, Texture2DLoadInfo>
    {
        public void Release(Texture2D asset, string path, Texture2DLoadInfo info)
        {
            if (asset != null)
                Object.Destroy(asset);
        }
    }

    public static class Texture2DGlobal
    {
        public static readonly GlobalAssetCache<Texture2DCacheKey, Texture2D, Texture2DLoadInfo> Cache = new(new Texture2DKeyFactory(), new Texture2DLoader(), new Texture2DReleaser(), null, UnityReceiverLiveness.IsUnityAlive);
    }
}