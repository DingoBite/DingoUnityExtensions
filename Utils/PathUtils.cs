using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace DingoUnityExtensions.Utils
{
    public static class PathUtils
    {
        private const string HTTP = "http:/";
        private const string HTTPS = "https:/";
        
        private const string DP_SLASH_1 = ":/";
        private const string DP_SLASH_2 = "://";
        private const string DP_SLASH_3 = ":///";
        private const string FILE = "file";

        private const string MP4 = ".mp4";
        private const string WAV = ".wav";
        private const string MOV = ".mov";
        private const string WEBM = ".webm";

        private const string JPG = ".jpg";
        private const string PNG = ".png";
        private const string JPEG = ".jpeg";

        public enum PathPrefix
        {
            Application,
            Persistent,
            Absolute,
            StreamingAssets,
            TemporaryCache,
            HTTP,
            HTTPS,
        }

        public static readonly IReadOnlyList<string> ImageExtensions = new List<string> { JPG, PNG, JPEG };
        public static readonly IReadOnlyList<string> VideoExtensions = new[] { MP4, WAV, MOV, WEBM };

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
                { PathPrefix.HTTP, HTTP },
                { PathPrefix.HTTPS, HTTPS },
            };
#if UNITY_ANDROID
            var path = Application.streamingAssetsPath;
            path = AbsoluteFilePathToUri(path);

            PrefixDictionary[PathPrefix.StreamingAssets] = path;
#endif
        }

        public static bool IsURL(string path) => path.StartsWith(HTTP) || path.StartsWith(HTTPS);

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
        
        public static string MakeImagePath(string folder, string imageName)
        {
            if (IsURL(folder))
            {
                if (imageName.StartsWith('/') || folder.EndsWith('/'))
                    return (folder + imageName);
                return folder + '/' + imageName; 
            }
            
#if UNITY_ANDROID
            if (imageName.StartsWith('/') || folder.EndsWith('/'))
                return (folder + imageName).Replace("//", "/");
            return folder + '/' + imageName; 
#endif
            imageName = Path.GetFileNameWithoutExtension(imageName);
            foreach (var extension in ImageExtensions)
            {
                var fileName = imageName + extension;
                folder = folder.Replace("\\", "/").Replace("//", "/");
                var path = $"{folder}/{fileName}";
                if (File.Exists(path))
                    return path;
            }

            return null;
        }

        public static string MakePathWithPrefix(PathPrefix pathPrefix, string path, bool createDirectory = false, bool addEscape = false)
        {
            path = path.Replace('\\', '/');
            string fullPath;
            var prefix = PrefixDictionary[pathPrefix];
            if (addEscape)
                prefix += "//";
            if (path.StartsWith('/') || prefix == "")
                fullPath = prefix + path;
            else
                fullPath = prefix + '/' + path;
            if (pathPrefix is not PathPrefix.HTTP and not PathPrefix.HTTPS && createDirectory)
            {
#if !UNITY_ANDROID
                var directoryName = Path.GetDirectoryName(fullPath);
                if (!string.IsNullOrWhiteSpace(directoryName) && directoryName != "/")
                    Directory.CreateDirectory(directoryName);
#endif
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