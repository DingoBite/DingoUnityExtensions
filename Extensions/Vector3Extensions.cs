using UnityEngine;

namespace DingoUnityExtensions.Extensions
{
    public static class Vector3Extensions
    {
        public static bool IsLessByComponents(this Vector3 baseVector, Vector3 vector) =>
            baseVector.x < vector.x && baseVector.y < vector.y && baseVector.z < vector.z;
        
        public static bool IsMoreByComponents(this Vector3 baseVector, Vector3 vector) =>
            baseVector.x > vector.x && baseVector.y > vector.y && baseVector.z > vector.z;

        public static Vector2 XZ(this Vector3 vector) => new Vector2(vector.x, vector.z);
    }
}