using System;
using UnityEngine;

namespace DingoUnityExtensions.Extensions
{
    public static class Vector2Extensions
    {
        private const double ReverseHalf = 1d / 180;
        private const double ReversePI = 1 / Math.PI;
        
        public static Vector2 ToPercentPoint(this Vector2 point, Vector2 minPoint, Vector2 maxPoint) =>
            (point - minPoint) / (maxPoint - minPoint);

        public static Vector2 ToPercentMercator(this Vector2 point) => new((point.x + 180) / 360, point.y / 90);
        
        public static Vector3 ForwardTopIndependent(this Transform transform) => transform.forward.XZ().XZ().normalized;
        public static Vector3 RightTopIndependent(this Transform transform) => transform.right.XZ().XZ().normalized;
        
        public static Vector3 XZ(this Vector2 vector) => new Vector3(vector.x, 0, vector.y);

        public static Vector2 Project(this Vector2 vector, Vector2 onNormal)
        {
            var result = Vector3.Project(vector.XZ(), onNormal.XZ());
            return result.XZ();
        }

        public static Vector2 ToRad(this Vector2 v)
        {
            v.x = (float)(v.x * ReverseHalf * Math.PI);
            v.y = (float)(v.y * ReverseHalf * Math.PI);
            return v;
        }

        public static Vector2 FromRad(this Vector2 v)
        {
            v.x = (float)(v.x * 180 * ReversePI);
            v.y = (float)(v.y * 180 * ReversePI);
            return v;
        }

        public static int ComarerLD(Vector2 v1, Vector2 v2, float epsilon = Vector3.kEpsilon)
        {
            if (Vector2.Distance(v1, v2) < epsilon)
                return 0;

            if (Math.Abs(v1.y - v2.y) > epsilon && Math.Abs(v1.x - v2.x) > epsilon) // Y !eq, X !eq, Y comp
                return v1.y.CompareTo(v2.y);

            if (Math.Abs(v1.y - v2.y) < epsilon) // Y eq, X comp
                return v1.x.CompareTo(v2.x);
            
            // X eq, Y comp
            return v1.y.CompareTo(v2.y);
        }

        public static bool Under(this Vector2Int v1, Vector2Int v2, bool isEq = false)
        {
            return isEq ? v1.x <= v2.x && v1.y <= v2.y : v1.x < v2.x && v1.y < v2.y;
        }
        
        public static bool Over(this Vector2Int v1, Vector2Int v2, bool isEq = false)
        {
            return isEq ? v1.x >= v2.x && v1.y >= v2.y : v1.x > v2.x && v1.y > v2.y;
        }
    }
}