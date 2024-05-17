using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DingoUnityExtensions.Extensions
{
    public static class UnityExtensions
    {
        public static void SetActive(this IEnumerable<GameObject> gameObjects, bool value)
        {
            foreach (var go in gameObjects.Where(g => g != null))
            {
                go.SetActive(value);
            }
        }
        
        public static void SetGameObjectsActive(this IEnumerable<Component> gameObjects, bool value)
        {
            foreach (var go in gameObjects.Where(g => g != null))
            {
                go.gameObject.SetActive(value);
            }
        }
    }
}