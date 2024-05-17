using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace DingoUnityExtensions.Utils
{
    public static class PathUtils
    {
        public enum PathPrefix
        {
            Application,
            Persistent,
            Absolute,
            StreamingAssets,
            TemporaryCache,
        }
        
        public static readonly IReadOnlyList<string> Extensions = new List<string> {".jpg", ".png"};

        private static readonly Dictionary<PathPrefix, string> PrefixDictionary = new()
        {
            { PathPrefix.Application, Application.dataPath},
            { PathPrefix.Persistent, Application.persistentDataPath},
            { PathPrefix.Absolute, ""},
            { PathPrefix.StreamingAssets, Application.streamingAssetsPath},
            { PathPrefix.TemporaryCache, Application.temporaryCachePath},
        };

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

        public static string GetPathPrefix(PathPrefix pathPrefix) => PrefixDictionary[pathPrefix];
        
        public static string MakePathWithPrefix(PathPrefix pathPrefix, string path, bool createDirectory = false)
        {
            path = path.Replace('\\', '/');
            string fullPath;
            var prefix = PrefixDictionary[pathPrefix];
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