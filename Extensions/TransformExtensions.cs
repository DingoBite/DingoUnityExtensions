using UnityEngine;

namespace DingoUnityExtensions.Extensions
{
    public static class TransformExtensions
    {
        public static void SetXZPos(this Transform transform, Vector2 xzVector)
        {
            var pos = transform.position;
            pos.x = xzVector.x;
            pos.z = xzVector.y;
            transform.localPosition = pos;
        }
        
        
        public static void FromMatrix(this Transform transform, Matrix4x4 matrix)
        {
            transform.localScale = matrix.ExtractScale();
            transform.localRotation = matrix.ExtractRotation();
            transform.localPosition = matrix.ExtractPosition();
        }
        
        public static (Vector3 position, Quaternion rotation, Vector3 localScale) FromMatrix(this Matrix4x4 matrix)
        {
            var localScale = matrix.ExtractScale();
            var rotation = matrix.ExtractRotation();
            var position = matrix.ExtractPosition();
            return (position, rotation, localScale);
        }
        
        public static Quaternion ExtractRotation(this Matrix4x4 matrix)
        {
            var rotation = Quaternion.LookRotation(
                matrix.GetColumn(2),
                matrix.GetColumn(1)
            );
 
            return rotation;
        }
 
        public static Vector3 ExtractPosition(this Matrix4x4 matrix)
        {
            return matrix.GetColumn(3);
        }
 
        public static Vector3 ExtractScale(this Matrix4x4 matrix)
        {
            var scale = new Vector3(
                matrix.GetColumn(0).magnitude,
                matrix.GetColumn(1).magnitude,
                matrix.GetColumn(2).magnitude
            );
            return scale;
        }
    }
}