#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
#endif

namespace DingoUnityExtensions.Utils
{
    public static class EditorRuntimeUtils
    {
        public static string OpenFileBrowser(string extensionNoDot, string title = "Open file", string directory = null)
        {
            string filePath = null;
#if UNITY_EDITOR
            filePath = EditorUtility.OpenFilePanel(title, directory ?? Application.dataPath, extensionNoDot);
#endif
            return filePath;
        } 
        
        public static string OpenFolderBrowser(string title = "Open file", string directory = null, string defaultName = "")
        {
            string folderPath = null;
#if UNITY_EDITOR
            folderPath = EditorUtility.OpenFolderPanel(title, directory ?? Application.dataPath, defaultName);
#endif
            return folderPath;
        } 
    }
}