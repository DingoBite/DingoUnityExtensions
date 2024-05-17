using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace DingoUnityExtensions.Utils
{
    public static class ImageUtils
    {
        private const string MP4 = ".mp4";
        private const string WAV = ".wav";
        private const string MOV = ".mov";
        private const string WEBM = ".webm";

        public static readonly IReadOnlyList<string> VideoExtensions = new[] { MP4, WAV, MOV, WEBM };
        
        public static Texture2D LoadStreamingAssetImage(string folder, string name, string imagesFolder = "images")
        {
            var path = PathUtils.MakeAssetPath(folder, name, imagesFolder);
            if (path == null)
                return null;
            
            var imageData = File.ReadAllBytes(path);
            var t = new Texture2D(2, 2);
            t.LoadImage(imageData);
            return t;
        }
        
        public static IEnumerator LoadStreamingAssetImage_C(Action<Texture2D> callback, string folder, string name, string imagesFolder = "images", float textureCreateDelay = 0)
        {
            var path = PathUtils.MakeAssetPath(folder, name, imagesFolder);
            yield return LoadStreamingAssetImage_C(callback, textureCreateDelay, path);
        }

        public static IEnumerator LoadStreamingAssetImage_C(Action<Texture2D> callback, float textureCreateDelay, string path)
        {
            if (path == null)
            {
                callback(null);
                yield break;
            }

            path = path.Replace("\\", "/");
            var imageDataTask = Task.Run(() => File.ReadAllBytesAsync(path));
            yield return new WaitUntil(() => imageDataTask.IsCompleted);
            if (!imageDataTask.IsCompletedSuccessfully)
            {
                Debug.LogError($"Task failed or canceled with error\n{imageDataTask.Exception}");
                callback(null);
                yield break;
            }

            if (textureCreateDelay > 1e-4)
                yield return new WaitForSeconds(textureCreateDelay);

            var t = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            t.LoadImage(imageDataTask.Result);
            callback(t);
        }
    }
}