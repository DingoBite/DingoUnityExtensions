using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace DingoUnityExtensions.Utils
{
    public static class HierarchyFinder
    {
        // Resources:
        public static T FindComponentInResources<T>() =>
            Resources.FindObjectsOfTypeAll<Transform>().Select(t => t.GetComponent<T>()).FirstOrDefault();

        public static IEnumerable<T> FindComponentsInResources<T>() =>
            Resources.FindObjectsOfTypeAll<Transform>().Select(t => t.GetComponent<T>());

        public static T FindComponentInResources<T>(string objectName) where T : Component =>
            Resources.FindObjectsOfTypeAll<Transform>().Select(t => t.GetComponent<T>())
                .FirstOrDefault(c => c.name == objectName);

        public static IEnumerable<T> FindComponentsInResources<T>(string objectName) where T : Component =>
            Resources.FindObjectsOfTypeAll<Transform>().Select(t => t.GetComponent<T>())
                .Where(c => c.name == objectName);

        // Transform:
        public static T FindFromChildren<T>(this Transform transform)
        {
            var anchor = transform.FindComponentInParent<T>(true);
            if (anchor == null)
                throw new Exception("Raycast anchor without component");
            return anchor;
        }
        
        public static T FindComponentInAllScenes<T>()
        {
            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                foreach (var rootGameObject in scene.GetRootGameObjects())
                {
                    if (rootGameObject.TryGetComponent<T>(out var component)) return component;
                    var componentInChildren = rootGameObject.transform.FindComponent<T>();
                    if (componentInChildren != null) return componentInChildren;
                }
            }

            return default;
        }
        
        public static List<T> FindComponentsInAllScenes<T>()
        {
            var result = new List<T>();
            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                foreach (var rootGameObject in scene.GetRootGameObjects())
                {
                    rootGameObject.transform.FindComponents(ref result);
                }
            }
            return result;
        }

        public static T FindComponentInAllScenes<T>(string objectName)
        {
            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                foreach (var rootGameObject in scene.GetRootGameObjects())
                {
                    if (rootGameObject.name == objectName && rootGameObject.TryGetComponent<T>(out var component))
                        return component;

                    var componentInChildren = rootGameObject.transform.FindComponent<T>(objectName);
                    if (componentInChildren != null) return componentInChildren;
                }
            }

            return default;
        }

        public static T FindComponentInAllScenesByTag<T>(string tag)
        {
            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                foreach (var rootGameObject in scene.GetRootGameObjects())
                {
                    if (rootGameObject.CompareTag(tag) && rootGameObject.TryGetComponent<T>(out var component))
                        return component;

                    var componentInChildren = rootGameObject.transform.FindComponentByTag<T>(tag);
                    if (componentInChildren != null) return componentInChildren;
                }
            }

            return default;
        }

        public static List<T> FindInSceneComponents<T>()
        {
            var result = new List<T>();
            foreach (var rootGameObject in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                var components = rootGameObject.GetComponents<T>();
                if (components != null) result.AddRange(components);
                rootGameObject.transform.FindComponents(ref result);
            }

            return result;
        }

        public static T FindInSceneComponent<T>() where T : Object
        {
            var activeScene = SceneManager.GetActiveScene();
            if (!activeScene.isLoaded)
                return Object.FindAnyObjectByType<T>(FindObjectsInactive.Include);
            
            foreach (var rootGameObject in activeScene.GetRootGameObjects())
            {
                if (rootGameObject.TryGetComponent<T>(out var component)) return component;
                var componentInChildren = rootGameObject.transform.FindComponent<T>();
                if (componentInChildren != null) return componentInChildren;
            }

            return default;
        }

        public static T FindInSceneComponent<T>(string objectName)
        {
            foreach (var rootGameObject in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                if (rootGameObject.name == objectName && rootGameObject.TryGetComponent<T>(out var component))
                    return component;

                var componentInChildren = rootGameObject.transform.FindComponent<T>(objectName);
                if (componentInChildren != null) return componentInChildren;
            }

            return default;
        }

        public static T FindInSceneComponentByTag<T>(string tag)
        {
            foreach (var rootGameObject in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                if (rootGameObject.CompareTag(tag) && rootGameObject.TryGetComponent<T>(out var component))
                    return component;

                var componentInChildren = rootGameObject.transform.FindComponentByTag<T>(tag);
                if (componentInChildren != null) return componentInChildren;
            }

            return default;
        }

        /// <summary>
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="includeSelf"></param>
        /// <param name="depth"> The generation depth. Default is int.MaxValue </param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T FindComponent<T>(this Transform transform, int depth = int.MaxValue, bool includeSelf = false)
        {
            if (depth <= 0) return default;
            if (includeSelf)
            {
                var componentsInSelf = transform.GetComponent<T>();
                if (componentsInSelf != null) return componentsInSelf;
            }
            
            foreach (Transform child in transform)
            {
                if (child.TryGetComponent<T>(out var component)) return component;
                var componentInChildren = child.FindComponent<T>(depth - 1);
                if (componentInChildren != null) return componentInChildren;
            }

            return default;
        }

        public static List<T> FindComponents<T>(this Transform transform, bool includeSelf = false, int depth = int.MaxValue)
        {
            if (depth <= 0) return default;

            var result = new List<T>();
            if (includeSelf)
            {
                var componentsInSelf = transform.GetComponents<T>();
                if (componentsInSelf != null) result.AddRange(componentsInSelf);
            }

            transform.FindComponents(ref result, depth);
            return result;
        }

        public static T FindComponent<T>(this Transform transform, string objectName, bool includeSelf, int depth = int.MaxValue)
        {
            if (depth <= 0) return default;

            if (includeSelf)
            {
                var componentsInSelf = transform.GetComponent<T>();
                if (componentsInSelf != null) return componentsInSelf;
            }

            return transform.FindComponent<T>(objectName, depth);
        }
        
        public static T FindComponent<T>(this Transform transform, string objectName, int depth = int.MaxValue)
        {
            if (depth <= 0) return default;

            foreach (Transform child in transform)
            {
                if (child.name == objectName && child.TryGetComponent<T>(out var component))
                    return component;
                var componentInChildren = child.FindComponent<T>(objectName, depth - 1);
                if (componentInChildren != null) return componentInChildren;
            }

            return default;
        }

        public static T FindComponentByTag<T>(this Transform transform, string tag, int depth = int.MaxValue)
        {
            if (depth <= 0) return default;

            foreach (Transform child in transform)
            {
                if (child.CompareTag(tag) && child.TryGetComponent<T>(out var component))
                    return component;
                var componentInChildren = child.FindComponentByTag<T>(tag, depth - 1);
                if (componentInChildren != null) return componentInChildren;
            }

            return default;
        }

        private static void FindComponents<T>(this Transform transform, ref List<T> result, int depth = int.MaxValue)
        {
            if (depth <= 0) return;

            foreach (Transform child in transform)
            {
                var components = child.GetComponents<T>();
                if (components != null) result.AddRange(components);

                child.FindComponents(ref result, depth - 1);
            }
        }

        public static T FindSibling<T>(this Transform transform) where T : Component
        {
            var baseParent = transform.parent;
            return FindInSceneComponents<T>().First(v =>
            {
                var tr = v.transform;
                return tr != transform && tr.parent == baseParent;
            });
        }

        public static List<T> FindSiblings<T>(this Transform transform) where T : Component
        {
            var baseParent = transform.parent;
            return FindInSceneComponents<T>().Where(v =>
            {
                var tr = v.transform;
                return tr != transform && tr.parent == baseParent;
            }).ToList();
        }
        
        private static void DebugErrorSubstringFind(Type type, string substring, bool isSubstring = true)
        {
            var withSting = isSubstring ? "with" : "without";
            var exceptionString = $"Find typeof {type} component(s) {withSting} substring {substring} error." +
                                  $"Try to add component or rename existing object(s) according {substring}";
            throw new Exception(exceptionString);
        }
        
        public static T FindComponentWithName<T>(this Transform transform, string subString, bool isNotFoundExceptionThrow)
        {
            foreach (Transform child in transform)
            {
                if (child.name == subString && child.TryGetComponent<T>(out var component))
                    return component;
                var componentInChildren = child.FindComponentWithName<T>(subString, false);
                if (componentInChildren != null) return componentInChildren;
            }
            if (isNotFoundExceptionThrow)
                DebugErrorSubstringFind(typeof(T), subString);
            return default;
        }
        
        public static T FindComponentWithSubstring<T>(this Transform transform, string subString, bool isNotFoundExceptionThrow)
        {
            foreach (Transform child in transform)
            {
                if (child.name.Contains(subString) && child.TryGetComponent<T>(out var component))
                    return component;
                var componentInChildren = child.FindComponentWithSubstring<T>(subString, false);
                if (componentInChildren != null) return componentInChildren;
            }
            if (isNotFoundExceptionThrow)
                DebugErrorSubstringFind(typeof(T), subString);
            return default;
        }

        public static List<T> FindComponentsWithSubstring<T>(this Transform transform, string subString, bool isNotFoundExceptionThrow)
        {
            var result = new List<T>();
            transform.FindComponentsWithSubstring(ref result, subString, isNotFoundExceptionThrow);
            return result;
        }

        public static void FindComponentsWithSubstring<T>(this Transform transform, ref List<T> result, string subString, 
            bool isNotFoundExceptionThrow)
        {
            foreach (Transform child in transform)
            {
                if (child.name.Contains(subString))
                {
                    var components = child.GetComponents<T>();
                    if (components != null) result.AddRange(components);
                }
                child.FindComponentsWithSubstring(ref result, subString, false);
            }
            if (isNotFoundExceptionThrow)
                DebugErrorSubstringFind(typeof(T), subString);
        }

        public static List<T> FindComponentsWithoutSubstring<T>(this Transform transform, string subString, bool isNotFoundExceptionThrow)
        {
            var result = new List<T>();
            transform.FindComponentsWithoutSubstring(ref result, subString, isNotFoundExceptionThrow);
            return result;
        }

        public static void FindComponentsWithoutSubstring<T>(this Transform transform, ref List<T> result, string subString, bool isNotFoundExceptionThrow)
        {
            foreach (Transform child in transform)
            {
                if (!child.name.Contains(subString))
                {
                    var components = child.GetComponents<T>();
                    if (components != null) result.AddRange(components);
                }
                child.FindComponentsWithoutSubstring(ref result, subString, false);
            }
            if (isNotFoundExceptionThrow)
                DebugErrorSubstringFind(typeof(T), subString, false);
        }

        public static T FindComponentInParent<T>(this Transform transform, bool isSelfCheck = false)
        {
            if (isSelfCheck && transform.TryGetComponent<T>(out var component))
                return component;
            var parent = transform.parent;
            if (parent == null) return default;

            if (parent.TryGetComponent<T>(out component)) return component;

            var componentInParent = parent.FindComponentInParent<T>();
            return componentInParent;
        }

        // In Top Children:
        public static T FindComponentInTopChildren<T>(this Transform transform) => 
            transform.Cast<Transform>().Select(child => child.GetComponent<T>()).FirstOrDefault();

        public static List<T> FindComponentsInTopChildren<T>(this Transform transform) => 
            transform.Cast<Transform>().Select(child => child.GetComponent<T>()).ToList();
    }
}
