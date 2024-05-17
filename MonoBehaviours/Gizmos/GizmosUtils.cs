using System.Collections.Generic;
using System.Linq;
using DingoUnityExtensions.MathAndGeometry;
using DingoUnityExtensions.Utils;
using UnityEngine;

namespace DingoUnityExtensions.MonoBehaviours.Gizmos
{
    public static class GizmosUtils
    {
        public static void DrawEllipsoid(Vector3 pos, Quaternion rotation, Vector3 size, int segments, Color color, float duration = 0)
        {
            var f = Vector3.forward;
            var u = Vector3.up;
            var r = Vector3.right;
            if (rotation != Quaternion.identity)
            {
                f = rotation * f;
                u = rotation * u;
                r = rotation * r;
            }
            
            DrawEllipse(pos, f, u, size.x, size.y, segments, color, duration);
            DrawEllipse(pos, r, f, size.y, size.z, segments, color, duration);
            DrawEllipse(pos, u, r, size.z, size.x, segments, color, duration);
        }
        
        public static void DrawEllipse(Vector3 pos, Vector3 forward, Vector3 up, float radiusX, float radiusY, int segments, Color color, float duration = 0)
        {
            float angle = 0f;
            Quaternion rot = Quaternion.LookRotation(forward, up);
            Vector3 lastPoint = Vector3.zero;
            Vector3 thisPoint = Vector3.zero;

            for (int i = 0; i < segments + 1; i++)
            {
                thisPoint.x = Mathf.Sin(Mathf.Deg2Rad * angle) * radiusX;
                thisPoint.y = Mathf.Cos(Mathf.Deg2Rad * angle) * radiusY;

                if (i > 0)
                {
                    Debug.DrawLine(rot * lastPoint + pos, rot * thisPoint + pos, color, duration);
                }

                lastPoint = thisPoint;
                angle += 360f / segments;
            }
        }
        
        public static void DrawSegment(Vector3 from, Vector3 to, Color color)
        {
            var prevColor = UnityEngine.Gizmos.color;
            UnityEngine.Gizmos.color = color;
            UnityEngine.Gizmos.DrawLine(from, to);
            UnityEngine.Gizmos.color = prevColor;
        }

        public static void DrawSphere(Vector3 center, float radius, Color color)
        {
            var prevColor = UnityEngine.Gizmos.color;
            UnityEngine.Gizmos.color = color;
            UnityEngine.Gizmos.DrawSphere(center, radius);
            UnityEngine.Gizmos.color = prevColor;
        }

        public static void DrawCube(Vector3 center, Vector3 size, Color color)
        {
            var prevColor = UnityEngine.Gizmos.color;
            UnityEngine.Gizmos.color = color;
            UnityEngine.Gizmos.DrawCube(center, size);
            UnityEngine.Gizmos.color = prevColor;
        }

        public static void DrawMesh(Mesh mesh, Vector3 center, Vector3 forwardRotation, Vector3 scale, Color color)
        {
            var prevColor = UnityEngine.Gizmos.color;
            UnityEngine.Gizmos.color = color;
            var rotation = Quaternion.LookRotation(forwardRotation.normalized);
            UnityEngine.Gizmos.DrawMesh(mesh, center, rotation, scale);
            UnityEngine.Gizmos.color = prevColor;
        }

        public static void DrawSegmentsLine(Vector3[] points, Color color, bool looped = false)
        {
            if (points.Length == 0)
                return;

            var prevColor = UnityEngine.Gizmos.color;
            UnityEngine.Gizmos.color = color;
            for (var i = 1; i < points.Length; i++)
            {
                var p0 = points[i - 1];
                var p1 = points[i];
                UnityEngine.Gizmos.DrawLine(p0, p1);
            }

            if (looped)
            {
                var pStart = points[0];
                var pEnd = points[^1];
                UnityEngine.Gizmos.DrawLine(pEnd, pStart);
            }
            UnityEngine.Gizmos.color = prevColor;
        }

        public static void DrawSegmentsLine(IEnumerable<MonoBehaviour> points, Color color, bool looped = false)
        {
            var pointsArray = points.Select(p => p.transform.position).ToArray();
            DrawSegmentsLine(pointsArray, color, looped);
        }

        public static void DrawLine(Vector3 center, Vector3 forward, Color color)
        {
            var prevColor = UnityEngine.Gizmos.color;
            UnityEngine.Gizmos.color = color;
            UnityEngine.Gizmos.DrawRay(center, forward * 1000);
            UnityEngine.Gizmos.DrawRay(center, -forward * 1000);
            UnityEngine.Gizmos.color = prevColor;
        }

        public static void DrawFrustum(Vector3 center, Quaternion rotation, float fov, float maxRange, float minRange, float aspect, Color color)
        {
            var prevColor = UnityEngine.Gizmos.color;
            var prevMatrix = UnityEngine.Gizmos.matrix;
            UnityEngine.Gizmos.color = color;
            UnityEngine.Gizmos.matrix = Matrix4x4.TRS(center, rotation, Vector3.one);

            UnityEngine.Gizmos.DrawFrustum(Vector3.zero, fov, maxRange, minRange, aspect);

            UnityEngine.Gizmos.matrix = prevMatrix;
            UnityEngine.Gizmos.color = prevColor;
        }

        public static void DrawPlane(Vector3 position, Vector2 dimensions, Quaternion rotation, Color color, bool isDrawCross)
        {
            var prevColor = UnityEngine.Gizmos.color;
            UnityEngine.Gizmos.color = color;
            var points = GeometricUtils.GetBorderPoints(position, dimensions, rotation);

            if (isDrawCross)
            {
                var right = points[3] - points[0];
                var up = points[1] - points[0];
                DrawSegment(points[0] + 0.5f * up, points[0] + right + 0.5f * up, color * 0.7f);
                DrawSegment(points[0] + 0.5f * right, points[0] + up + 0.5f * right, color * 0.7f);
            }

            DrawSegmentsLine(points, color, true);

            UnityEngine.Gizmos.color = prevColor;
        }
    }
}