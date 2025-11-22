using System.IO;
using UnityEditor;
using UnityEngine;

namespace DingoUnityExtensions.PrefabsCreateMenu.Editor
{
    public static class PrefabTemplatesSceneMenu
    {
        private static bool _isMenuRequested;
        private static GameObject _requestedParent;

        static PrefabTemplatesSceneMenu()
        {
#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui -= OnSceneGUI;
            SceneView.duringSceneGui += OnSceneGUI;
#else
            SceneView.onSceneGUIDelegate -= OnSceneGUI;
            SceneView.onSceneGUIDelegate += OnSceneGUI;
#endif
        }

        [MenuItem("GameObject/Create From Prefab...", false, 0)]
        public static void RequestMenu(MenuCommand command)
        {
            _requestedParent = command.context as GameObject;
            _isMenuRequested = true;
        }

        private static void OnSceneGUI(SceneView sceneView)
        {
            if (!_isMenuRequested)
                return;

            _isMenuRequested = false;

            var menu = new GenericMenu();
            var parentGameObject = _requestedParent;
            var hasPrefabs = false;

            foreach (var assetsSubPaths in PrefabsCreateMenuConfig.AssetsSubPathsToPrefabs)
            {
                PopulateMenuItems(assetsSubPaths, menu, parentGameObject, ref hasPrefabs);
            }

            if (!hasPrefabs)
            {
                EditorUtility.DisplayDialog("No prefabs", "No prefabs found in configured folders.", "OK");
                return;
            }

            menu.ShowAsContext();
        }

        private static void PopulateMenuItems(string templatesFolder, GenericMenu menu, GameObject parentGameObject, ref bool hasPrefabs)
        {
            var guids = AssetDatabase.FindAssets("t:Prefab", new[] { templatesFolder });

            if (guids == null || guids.Length == 0)
                return;

            hasPrefabs = true;

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null)
                    continue;

                var prefabName = Path.GetFileNameWithoutExtension(path);

                var menuPath = GetMenuPathRelativeToRoot(templatesFolder, path);

                var templatePathLocal = path;
                var prefabNameLocal = prefabName;
                var parentLocal = parentGameObject;
                var menuPathLocal = menuPath;

                menu.AddItem(
                    new GUIContent(menuPathLocal),
                    false,
                    () => { CreateInstanceFromPrefab(templatePathLocal, prefabNameLocal, parentLocal); });
            }
        }

        private static string GetMenuPathRelativeToRoot(string rootFolder, string assetPath)
        {
            if (assetPath.StartsWith(rootFolder))
            {
                var relativePath = assetPath.Substring(rootFolder.Length).TrimStart('/', '\\');
                var withoutExt = Path.ChangeExtension(relativePath, null);
                return withoutExt.Replace('\\', '/');
            }

            return Path.GetFileNameWithoutExtension(assetPath);
        }

        private static void CreateInstanceFromPrefab(string templatePath, string prefabName, GameObject parentGameObject)
        {
            var template = AssetDatabase.LoadAssetAtPath<GameObject>(templatePath);
            if (template == null)
            {
                Debug.LogError($"Prefab not found at path: {templatePath}");
                return;
            }

            var parentTransform = parentGameObject != null ? parentGameObject.transform : null;

            var instance = PrefabUtility.InstantiatePrefab(template, parentTransform) as GameObject;
            if (instance == null)
            {
                Debug.LogError("Failed to instantiate prefab.");
                return;
            }

            if (parentTransform == null)
            {
                instance.transform.position = GetSpawnPosition();
            }

            Undo.RegisterCreatedObjectUndo(instance, $"Create {prefabName}");
            Selection.activeGameObject = instance;
            EditorGUIUtility.PingObject(instance);
        }

        private static Vector3 GetSpawnPosition()
        {
            var sceneView = SceneView.lastActiveSceneView;
            if (sceneView != null)
            {
                var cam = sceneView.camera;
                if (cam != null)
                {
                    var ray = new Ray(cam.transform.position, cam.transform.forward);
                    const float distance = 10f;
                    return ray.GetPoint(distance);
                }

                return sceneView.pivot;
            }

            return Vector3.zero;
        }
    }
}
