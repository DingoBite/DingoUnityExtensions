using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace DingoUnityExtensions.Utils
{
    public static class MultiplatformLoadUtils
    {
        public static async Task<T> LoadDeserializedAsync<T>(string path, Func<string, T> deserialization, T defaultValue = default)
        {
            try
            {
                if (PathUtils.IsURL(path))
                {
                    var strData = await GetDataFromRequest(path);
                    return await Task.Run(() => deserialization(strData));
                }
                
#if UNITY_ANDROID
                path = PathUtils.AbsoluteFilePathToUri(path);
                var data = await GetDataFromRequest(path);
#else
                var data = await File.ReadAllTextAsync(path);
#endif
                var value = await Task.Run(() => deserialization(data));
                return value;
            }
            catch (Exception e)
            {
                Debug.LogError(path);
                Debug.LogException(e);
                return defaultValue;
            }
        }

        public static async Task<string> LoadSerializedStringAsync(string path)
        {
            try
            {
                if (PathUtils.IsURL(path))
                    return await GetDataFromRequest(path);
                
#if UNITY_ANDROID
                path = PathUtils.AbsoluteFilePathToUri(path);
                var data = await GetDataFromRequest(path);
#else
                var data = await File.ReadAllTextAsync(path);
#endif
                return data;
            }
            catch (Exception e)
            {
                Debug.LogError(path);
                Debug.LogException(e);
                return null;
            }
        }

        public static async Task<Texture2D> LoadTexture2DAsync(string path, bool disableLogException = false, CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (PathUtils.IsURL(path))
                    return await GetTextureFromRequest(path, cancellationToken);
                
#if UNITY_ANDROID
                path = PathUtils.AbsoluteFilePathToUri(path);
                var result = await GetTextureFromRequest(path, cancellationToken);
#else
                var thumbnailImageData = await File.ReadAllBytesAsync(path, cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();
                var result = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                result.LoadImage(thumbnailImageData);
#endif
                return result;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return null;
            }
            catch (Exception e)
            {
                if (!disableLogException)
                {
                    Debug.LogError(path);
                    Debug.LogException(e);
                }
                return null;
            }
        }

        private static async Task<Texture2D> GetTextureFromRequest(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var unityWebRequest = UnityWebRequestTexture.GetTexture(path);
            using var cancellationRegistration = cancellationToken.Register(() => unityWebRequest.Abort());
            unityWebRequest.timeout = 5;
            var loadingRequest = await unityWebRequest.SendWebRequest();
            cancellationToken.ThrowIfCancellationRequested();
            Texture2D result;
            if (loadingRequest.result == UnityWebRequest.Result.Success)
                result = DownloadHandlerTexture.GetContent(unityWebRequest);
            else
                result = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            return result;
        }

        private static async Task<string> GetDataFromRequest(string path)
        {
            using var unityWebRequest = UnityWebRequest.Get(path);
            unityWebRequest.timeout = 5;
            var loadingRequest = await unityWebRequest.SendWebRequest();
            var data = loadingRequest.downloadHandler.text;
            return data;
        }

    }
}
