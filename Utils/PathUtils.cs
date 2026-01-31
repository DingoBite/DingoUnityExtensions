using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace DingoUnityExtensions.Utils
{
    public static class PathUtils
    {
        private const string HTTP = "http://";
        private const string HTTPS = "https://";

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

        public static bool IsURL(string path) => path.StartsWith(HTTP, StringComparison.Ordinal) || path.StartsWith(HTTPS, StringComparison.Ordinal);

        public static string AbsoluteFilePathToUri(string path)
        {
            if (string.IsNullOrEmpty(path))
                return string.Empty;

            if (Uri.TryCreate(path, UriKind.Absolute, out var uri))
                return uri.AbsoluteUri;

            return new Uri(path).AbsoluteUri;
        }

        public static string MakeImagePath(string folder, string imageName)
        {
            if (string.IsNullOrEmpty(folder) || string.IsNullOrEmpty(imageName))
                return null;

            if (IsURL(folder))
                return JoinWithSlash(folder, imageName);

#if UNITY_ANDROID
            return JoinWithSlash(folder, imageName);
#endif
            folder = NormalizePath(folder);
            imageName = Path.GetFileNameWithoutExtension(imageName);

            foreach (var ext in ImageExtensions)
            {
                var path = JoinWithSlash(folder, imageName + ext);
                if (File.Exists(path))
                    return path;
            }

            return null;
        }

        public static string GetRootPathFromPrefix(PathPrefix pathPrefix) => PrefixDictionary[pathPrefix];

        public static string MakePathWithPrefix(PathPrefix pathPrefix, string path, bool createDirectory = false, bool addEscape = false)
        {
            path = (path ?? string.Empty).Replace('\\', '/');

            var prefix = PrefixDictionary[pathPrefix] ?? string.Empty;
            if (addEscape)
                prefix += "//";

            var fullPath = string.IsNullOrEmpty(prefix) ? path : JoinWithSlash(prefix, path);

            if (pathPrefix is not PathPrefix.HTTP and not PathPrefix.HTTPS && createDirectory)
            {
#if !UNITY_ANDROID
                var dir = Path.GetDirectoryName(fullPath);
                if (!string.IsNullOrWhiteSpace(dir) && dir != "/")
                    Directory.CreateDirectory(dir);
#endif
            }

            return fullPath;
        }

        public static string NormalizePath(this string path)
        {
            if (string.IsNullOrEmpty(path))
                return string.Empty;

            path = path.Replace('\\', '/');

            var schemeIdx = path.IndexOf("://", System.StringComparison.Ordinal);
            if (schemeIdx < 0)
                return CollapseDoubleSlashes(path);

            var head = path[..(schemeIdx + 3)];
            var tail = path[(schemeIdx + 3)..];
            return head + CollapseDoubleSlashes(tail);
        }

        public static string GetPathWithoutExtensions(string path)
        {
            var dir = Path.GetDirectoryName(path);
            var fileName = Path.GetFileNameWithoutExtension(path);
            return dir + "/" + fileName;
        }

        private static string CollapseDoubleSlashes(string s)
        {
            var length = s.Length;
            var buffer = length <= 256 ? stackalloc char[length] : new char[length];
            var w = 0;
            var lastSlash = false;

            for (var i = 0; i < length; i++)
            {
                var c = s[i];
                if (c == '/')
                {
                    if (lastSlash)
                        continue;
                    lastSlash = true;
                }
                else
                {
                    lastSlash = false;
                }

                buffer[w++] = c;
            }

            return new string(buffer[..w]);
        }

        private static string JoinWithSlash(string left, string right)
        {
            if (string.IsNullOrEmpty(left))
                return right ?? string.Empty;
            if (string.IsNullOrEmpty(right))
                return left;

            var lEnds = left.EndsWith("/", System.StringComparison.Ordinal);
            var rStarts = right.StartsWith("/", System.StringComparison.Ordinal);

            if (lEnds && rStarts)
                return left + right.Substring(1);
            if (!lEnds && !rStarts)
                return left + "/" + right;
            return left + right;
        }
    }
}