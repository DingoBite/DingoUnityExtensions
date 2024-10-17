using DingoUnityExtensions.Utils;
using UnityEngine;

namespace DingoUnityExtensions.Extensions
{
    public static class ComponentExtensions
    {
        public static void SetLayerRecursive(this Component component, int layer) => component.transform.SetLayerRecursive(layer);
        public static void SetLayerRecursive(this GameObject gameObject, int layer) => gameObject.transform.SetLayerRecursive(layer);
        
        public static void SetLayerRecursive(this Transform transform, int layer)
        {
            foreach (var tr in transform.FindComponents<Transform>())
            {
                tr.gameObject.layer = layer;
            }
        }
    }
}