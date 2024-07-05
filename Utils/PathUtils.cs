using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace DingoUnityExtensions.Utils
{
    public static class PathUtils
    {
        private const string DP_SLASH_1 = ":/";
        private const string DP_SLASH_2 = "://";
        private const string DP_SLASH_3 = ":///";
        private const string FILE = "file";

        public enum PathPrefix
        {
            Application,
            Persistent,
            Absolute,
            StreamingAssets,
            TemporaryCache,
        }

        public static readonly IReadOnlyList<string> Extensions = new List<string> { ".jpg", ".png" };

        private static readonly Dictionary<PathPrefix, string> PrefixDictionary;

        static PathUtils()
        {
            PrefixDictionary = new()
            {
                { PathPrefix.Application, Application.dataPath },
                { PathPrefix.Persistent, Application.persistentDataPath },
                { PathPrefix.Absolute, "" },
                { PathPrefix.StreamingAssets, Application.streamingAssetsPath },
                { PathPrefix.TemporaryCache, Application.temporaryCachePath },
            };
#if UNITY_ANDROID
            var path = Application.streamingAssetsPath;
            path = AbsoluteFilePathToUri(path);

            PrefixDictionary[PathPrefix.StreamingAssets] = path;
// #elif UNITY_IOS
            // var path = $"{FILE}{DP_SLASH_3}{Application.dataPath}/AssetBundles/";
            // PrefixDictionary[PathPrefix.StreamingAssets] = path;
#endif
        }

        public static string AbsoluteFilePathToUri(string path)
        {
            if (!path.Contains(DP_SLASH_2))
            {
                if (path.Contains(DP_SLASH_2))
                    path = path.Replace(DP_SLASH_2, DP_SLASH_3);
                else if (path.Contains(DP_SLASH_1))
                    path = path.Replace(DP_SLASH_1, DP_SLASH_3);
                else
                    path = FILE + DP_SLASH_3 + path;
            }

            return path;
        }

        public static string MakeAssetPath(string folder, string name, string imagesFolder = "images")
        {
            foreach (var extension in Extensions)
            {
                var imgName = name + extension;
                var path = Path.Combine(Application.streamingAssetsPath, imagesFolder, folder, imgName);
                path = path.Replace("\\", "/");
                if (File.Exists(path))
                    return path;
            }

            return null;
        }
        
        public static string MakeImagePath(string folder, string imageName)
        {
            foreach (var extension in Extensions)
            {
                var fileName = imageName + extension;
                folder = folder.Replace("\\", "/").Replace("//", "/");
                var path = $"{folder}/{fileName}";
                if (File.Exists(path))
                    return path;
            }

            return null;
        }

        public static string GetPathPrefix(PathPrefix pathPrefix) => PrefixDictionary[pathPrefix];

        public static string MakePathWithPrefix(PathPrefix pathPrefix, string path, bool createDirectory = false)
        {
            path = path.Replace('\\', '/');
            string fullPath;
            var prefix = PrefixDictionary[pathPrefix];
            Debug.Log($"{pathPrefix} : {prefix} : {path}");
            if (path.StartsWith('/') || prefix == "")
                fullPath = prefix + path;
            else
                fullPath = prefix + '/' + path;
            if (createDirectory)
            {
                var directoryName = Path.GetDirectoryName(fullPath);
                if (!string.IsNullOrWhiteSpace(directoryName) && directoryName != "/")
                    Directory.CreateDirectory(directoryName);
            }

            return fullPath;
        }

        public static string GetPathWithoutExtensions(string path)
        {
            var dir = Path.GetDirectoryName(path);
            var fileName = Path.GetFileNameWithoutExtension(path);
            return dir + "/" + fileName;
        }
    }
}