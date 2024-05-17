using System;
using System.Collections.Generic;
using UnityEngine;

namespace DingoUnityExtensions.MathAndGeometry
{
    public static class GeometricUtils
    {
        public static Vector3 GetProjection(Vector3 point, Vector3 pointOnLine, Vector3 direction)
        {
            var d = direction;
            var p = pointOnLine;

            var t = Vector3.Dot(point - p, d) / Vector3.Dot(d, d);
            var projection = p + t * d;
            return projection;
        }

        /// <summary>
        ///     Order:
        ///     0 - Bottom Left.
        ///     1 - Top Left.
        ///     2 - Top Right.
        ///     3 - Bottom Right.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="dimensions"></param>
        /// <param name="rotation"></param>
        /// <returns></returns>
        public static Vector3[] GetBorderPoints(Vector3 position, Vector2 dimensions, Quaternion rotation)
        {
            var right = rotation * Vector3.right;
            var up = rotation * Vector3.up;

            var halfWidth = right * dimensions.x * 0.5f;
            var halfHeight = up * dimensions.y * 0.5f;

            var topLeft = position - halfWidth + halfHeight;
            var topRight = position + halfWidth + halfHeight;
            var bottomLeft = position - halfWidth - halfHeight;
            var bottomRight = position + halfWidth - halfHeight;

            var points = new[]
            {
                bottomLeft,
                topLeft,
                topRight,
                bottomRight
            };
            return points;
        }

        public static Vector3 GetPointFromUV(Vector2 uvPoint, Vector3 origin, Vector3 horizontalDir, Vector3 verticalDir)
        {
            return origin + uvPoint.x * horizontalDir + uvPoint.y * verticalDir;
        }

        public static Vector3 GetPointFromUV(Vector2 uvPoint, IReadOnlyList<Vector3> planeBorders)
        {
            var origin = planeBorders[0];
            var horizontalDir = planeBorders[3] - planeBorders[0];
            var verticalDir = planeBorders[1] - planeBorders[0];
            return GetPointFromUV(uvPoint, origin, horizontalDir, verticalDir);
        }

        public static Vector2 GetUVFromProjectedPoint(Vector3 point, Vector3 origin, Vector3 horizontalDir, Vector3 verticalDir)
        {
            var localPoint = point - origin;
            var u = Vector3.Dot(localPoint, horizontalDir) / Vector3.Dot(horizontalDir, horizontalDir);
            var v = Vector3.Dot(localPoint, verticalDir) / Vector3.Dot(verticalDir, verticalDir);
            return new Vector2(u, v);
        }

        public static Vector2 GetUVFromProjectedPoint(Vector3 point, IReadOnlyList<Vector3> planeBorders)
        {
            var origin = planeBorders[0];
            var horizontalDir = planeBorders[3] - planeBorders[0];
            var verticalDir = planeBorders[1] - planeBorders[0];
            return GetUVFromProjectedPoint(point, origin, horizontalDir, verticalDir);
        }

        public static bool GetProjection(Vector3 position, Vector3 frustumPosition, Plane frustumNearPlane, out Vector3 projectedPoint)
        {
            var direction = position - frustumPosition;
            projectedPoint = Vector3.zero;
            var ray = new Ray(frustumPosition, direction.normalized);
            if (!frustumNearPlane.Raycast(ray, out var enter))
                return false;
            projectedPoint = ray.GetPoint(enter);
            return true;
        }

        /// <summary>
        /// 0 - Near.
        /// 1 - Far.
        /// 2 - Left.
        /// 3 - Right.
        /// 4 - Top.
        /// 5 - Bottom.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="near"></param>
        /// <param name="far"></param>
        /// <param name="aspect"></param>
        /// <param name="fov"></param>
        /// <param name="forward"></param>
        /// <param name="up"></param>
        /// <returns></returns>
        public static Plane[] GetFrustumPlanes(Vector3 position, float near, float far, float aspect, float fov, Vector3 forward, Vector3 up)
        {
            var halfHeight = near * Mathf.Tan(fov * Mathf.Deg2Rad * 0.5f);
            var halfWidth = halfHeight * aspect;

            var right = Vector3.Cross(forward, up);

            var nearCenter = position + forward * near;
            var farCenter = position + forward * far;

            var corners = new Vector3[8];
            corners[0] = nearCenter - halfHeight * up - halfWidth * right; // near bottom left
            corners[1] = nearCenter + halfHeight * up - halfWidth * right; // near top left
            corners[2] = nearCenter + halfHeight * up + halfWidth * right; // near top right
            corners[3] = nearCenter - halfHeight * up + halfWidth * right; // near bottom right

            corners[4] = farCenter - halfHeight * far / near * up - halfWidth * far / near * right; // far bottom left
            corners[5] = farCenter + halfHeight * far / near * up - halfWidth * far / near * right; // far top left
            corners[6] = farCenter + halfHeight * far / near * up + halfWidth * far / near * right; // far top right
            corners[7] = farCenter - halfHeight * far / near * up + halfWidth * far / near * right; // far bottom right

            var planes = new Plane[6];
            planes[0] = new Plane(corners[0], corners[1], corners[2]); // Near
            planes[1] = new Plane(corners[4], corners[7], corners[6]); // Far
            planes[2] = new Plane(corners[0], corners[4], corners[5]); // Left
            planes[3] = new Plane(corners[3], corners[2], corners[6]); // Right
            planes[4] = new Plane(corners[1], corners[5], corners[6]); // Top
            planes[5] = new Plane(corners[0], corners[3], corners[7]); // Bottom
            return planes;
        }

        public static bool IsPointInFrustum(Vector3 point, float near, float far, float aspect, float fov, Vector3 forward, Vector3 position, Vector3 up)
        {
            var planes = GetFrustumPlanes(position, near, far, aspect, fov, forward, up);
            return IsPointInFrustum(point, planes);
        }
        
        public static bool IsPointInFrustum(Vector3 point, IReadOnlyList<Plane> planes)
        {
            if (planes.Count != 6)
                throw new ArgumentException($"Planes count {planes.Count} != 6");
            for (var i = 0; i < 6; i++)
            {
                if (!planes[i].GetSide(point))
                    return false;
            }

            return true;
        }

        public static float MinDistanceToFrustum(Vector3 point, IReadOnlyList<Plane> planes)
        {
            if (planes.Count != 6)
                throw new ArgumentException($"Planes count {planes.Count} != 6");
            var minDistance = float.MaxValue;
            for (var i = 0; i < 6; i++)
            {
                var distance = System.Math.Abs(planes[i].GetDistanceToPoint(point));
                if (distance < minDistance)
                    minDistance = distance;
            }
            return minDistance;
        }
        
        public static float MinDistanceToFrustum(Vector3 point, float near, float far, float aspect, float fov, Vector3 forward, Vector3 position, Vector3 up)
        {
            var planes = GetFrustumPlanes(position, near, far, aspect, fov, forward, up);
            return MinDistanceToFrustum(point, planes);
        }
        
        public static float MaxDistanceToFrustum(Vector3 point, IReadOnlyList<Plane> planes)
        {
            if (planes.Count != 6)
                throw new ArgumentException($"Planes count {planes.Count} != 6");
            var maxDistance = float.MinValue;
            for (var i = 0; i < 6; i++)
            {
                var distance = System.Math.Abs(planes[i].GetDistanceToPoint(point));
                if (distance > maxDistance)
                    maxDistance = distance;
            }
            return maxDistance;
        }
        
        public static float MaxDistanceToFrustum(Vector3 point, float near, float far, float aspect, float fov, Vector3 forward, Vector3 position, Vector3 up)
        {
            var planes = GetFrustumPlanes(position, near, far, aspect, fov, forward, up);
            return MaxDistanceToFrustum(point, planes);
        }
    }
}