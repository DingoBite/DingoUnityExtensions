#if UNITASK_EXISTS
using System;
using System.IO;
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
#if UNITY_ANDROID
                var unityWebRequest = UnityWebRequest.Get(path);
                unityWebRequest.timeout = 5;
                var loadingRequest = await unityWebRequest.SendWebRequest();
                var data = loadingRequest.downloadHandler.text;
#else
                var data = await File.ReadAllTextAsync(path);
#endif
                var value = defaultValue;
                value = await Task.Run(() => deserialization(data));
                return value;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return defaultValue;
            }
        }

        public static async Task<Texture2D> LoadTexture2DAsync(string path)
        {
            try
            {
#if UNITY_ANDROID
                 path = PathUtils.AbsoluteFilePathToUri(path);
                 using var unityWebRequest = UnityWebRequestTexture.GetTexture(path);
                 unityWebRequest.timeout = 5;
                 await unityWebRequest.SendWebRequest();
                 var loadingRequest = await UnityWebRequest.Get(path).SendWebRequest();
                 Texture2D result;
                 if (loadingRequest.result == UnityWebRequest.Result.Success)
                     result = DownloadHandlerTexture.GetContent(unityWebRequest);
                 else
                     result = new Texture2D(2, 2, TextureFormat.RGBA32, false);
#else
                var result = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                var thumbnailImageData = await File.ReadAllBytesAsync(path);
                result.LoadImage(thumbnailImageData);
#endif
                return result;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return null;
            }
        }
    }
}
#endif
