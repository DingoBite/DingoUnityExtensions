using UnityEngine;

namespace DingoUnityExtensions.Extensions
{
    public static class MeshExtensions
    {
        public static void GetWorldBoundaries(this Renderer renderer, out Vector2 min, out Vector2 max, out float height)
        {
            var bounds = renderer.bounds;
            height = bounds.max.y;
            min = bounds.min.XZ();
            max = bounds.max.XZ();
        }

        public static Vector2 GetWorldBoundariesXZSize(this Renderer renderer)
        {
            var bounds = renderer.bounds;
            var min = bounds.min.XZ();
            var max = bounds.max.XZ();
            return max - min;
        }
        
        public static void GetWorldBoundaries(this Renderer renderer, out Vector3 min, out Vector3 max)
        {
            var bounds = renderer.bounds;
            min = bounds.min;
            max = bounds.max;
        } 
    }
}