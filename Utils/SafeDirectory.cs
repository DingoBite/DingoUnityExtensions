using System;
using System.IO;
using UnityEngine;

namespace DingoUnityExtensions.Utils
{
    public static class SafeDirectory
    {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_WSA
        private static readonly StringComparison PathCmp = StringComparison.OrdinalIgnoreCase;
#else
        private static readonly StringComparison PathCmp = StringComparison.Ordinal;
#endif

        public static void Delete(string path, string basePath, bool recursive = false)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Path is empty.", nameof(path));

            basePath = basePath.NormalizePath();
            var full = path.NormalizePath();
            
            if (!IsUnderBase(full, basePath))
                throw new UnauthorizedAccessException($"Refusing to delete outside of persistentDataPath.\nBase: {basePath}\nRequested: {full}");

            if (!Directory.Exists(full))
                return;

            Directory.Delete(full, recursive);
        }

        private static bool IsUnderBase(string fullPath, string basePath)
        {
            return fullPath.StartsWith(basePath, PathCmp);
        }
    }
}