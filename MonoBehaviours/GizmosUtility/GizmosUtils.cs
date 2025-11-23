using System;
using System.Collections.Generic;
using System.Linq;
using DingoUnityExtensions.MathAndGeometry;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace DingoUnityExtensions.MonoBehaviours.GizmosUtility
{
    public static class GizmosUtils
    {
        public static event Func<object, Color, Color> EachSegmentCallback;

        public static Color WrapEachSegmentCallback(object obj, Color color, Func<Tuple<Vector3, Vector3>, Color> func) => WrapEachSegmentCallback<Tuple<Vector3, Vector3>>(obj, color, func);
        
        public static Color WrapEachSegmentCallback<T>(object obj, Color color, Func<T, Color> func)
        {
            try
            {
                var value = (T)obj;
                return func(value);
            }
            catch (Exception e)
            {
                Debug.LogWarning(e.Message);
            }
            return color;
        }
        
        public static void DrawEllipsoid(Vector3 pos, Quaternion rotation, Vector3 size, int segments, Color color)
        {
            if (color.a < Vector2.kEpsilon)
                return;
            var f = Vector3.forward;
            var u = Vector3.up;
            var r = Vector3.right;
            if (rotation != Quaternion.identity)
            {
                f = rotation * f;
                u = rotation * u;
                r = rotation * r;
            }
            
            DrawEllipse(pos, f, u, size.x, size.y, segments, color);
            DrawEllipse(pos, r, f, size.y, size.z, segments, color);
            DrawEllipse(pos, u, r, size.z, size.x, segments, color);
        }
        
        public static void DrawEllipse(Vector3 pos, Vector3 forward, Vector3 up, float radiusX, float radiusY, int segments, Color color)
        {
            if (color.a < Vector2.kEpsilon)
                return;
            var angle = 0f;
            var rot = Quaternion.LookRotation(forward, up);
            var lastPoint = Vector3.zero;
            var thisPoint = Vector3.zero;

            for (var i = 0; i < segments + 1; i++)
            {
                thisPoint.x = Mathf.Sin(Mathf.Deg2Rad * angle) * radiusX;
                thisPoint.y = Mathf.Cos(Mathf.Deg2Rad * angle) * radiusY;

                if (i > 0)
                {
                    var start = rot * lastPoint + pos;
                    var end = rot * thisPoint + pos;
                    if (EachSegmentCallback != null)
                        color = EachSegmentCallback.Invoke((start, end), color);
                    DrawSegment(start, end, color);
                }

                lastPoint = thisPoint;
                angle += 360f / segments;
            }
        }
        
        public static void DrawSegment(Vector3 from, Vector3 to, Color color)
        {
            if (color.a < Vector2.kEpsilon)
                return;
            var prevColor = Gizmos.color;
            Gizmos.color = color;
            Gizmos.DrawLine(from, to);
            Gizmos.color = prevColor;
        }

        public static void DrawSphere(Vector3 center, float radius, Color color)
        {
            if (color.a < Vector2.kEpsilon)
                return;
            var prevColor = Gizmos.color;
            Gizmos.color = color;
            Gizmos.DrawSphere(center, radius);
            Gizmos.color = prevColor;
        }

        public static void DrawCube(Vector3 center, Vector3 size, Color color)
        {
            if (color.a < Vector2.kEpsilon)
                return;
            var prevColor = Gizmos.color;
            Gizmos.color = color;
            Gizmos.DrawCube(center, size);
            Gizmos.color = prevColor;
        }

        public static void DrawMesh(Mesh mesh, Vector3 center, Vector3 forwardRotation, Vector3 scale, Color color)
        {
            if (color.a < Vector2.kEpsilon)
                return;
            var prevColor = Gizmos.color;
            Gizmos.color = color;
            var rotation = Quaternion.LookRotation(forwardRotation.normalized);
            Gizmos.DrawMesh(mesh, center, rotation, scale);
            Gizmos.color = prevColor;
        }
        
        public static void DrawMesh(Mesh mesh, Matrix4x4 trs, Color color)
        {
            if (color.a < Vector2.kEpsilon)
                return;
            var prevColor = Gizmos.color;
            Gizmos.color = color;
            Gizmos.DrawMesh(mesh, trs.GetPosition(), trs.rotation, trs.lossyScale);
            Gizmos.color = prevColor;
        }

        public static void DrawSegmentsLine(Vector3[] points, Color color, bool looped = false)
        {
            if (color.a < Vector2.kEpsilon)
                return;
            if (points.Length == 0)
                return;

            for (var i = 1; i < points.Length; i++)
            {
                var p0 = points[i - 1];
                var p1 = points[i];
                if (EachSegmentCallback != null)
                    color = EachSegmentCallback.Invoke((p0, p1), color);
                DrawSegment(p0, p1, color);
            }

            if (looped)
            {
                var pStart = points[0];
                var pEnd = points[^1];
                if (EachSegmentCallback != null)
                    color = EachSegmentCallback.Invoke((pStart, pEnd), color);
                DrawSegment(pStart, pEnd, color);
            }
        }

        public static void DrawSegmentsLine(IEnumerable<MonoBehaviour> points, Color color, bool looped = false)
        {
            if (color.a < Vector2.kEpsilon)
                return;
            var pointsArray = points.Select(p => p.transform.position).ToArray();
            DrawSegmentsLine(pointsArray, color, looped);
        }

        public static void DrawLine(Vector3 center, Vector3 forward, Color color)
        {
            if (color.a < Vector2.kEpsilon)
                return;
            var prevColor = Gizmos.color;
            Gizmos.color = color;
            Gizmos.DrawRay(center, forward * 1000);
            Gizmos.DrawRay(center, -forward * 1000);
            Gizmos.color = prevColor;
        }

        public static void DrawFrustum(Vector3 center, Quaternion rotation, float fov, float maxRange, float minRange, float aspect, Color color)
        {
            if (color.a < Vector2.kEpsilon)
                return;
            var prevColor = Gizmos.color;
            var prevMatrix = Gizmos.matrix;
            Gizmos.color = color;
            Gizmos.matrix = Matrix4x4.TRS(center, rotation, Vector3.one);

            Gizmos.DrawFrustum(Vector3.zero, fov, maxRange, minRange, aspect);

            Gizmos.matrix = prevMatrix;
            Gizmos.color = prevColor;
        }

        public static void DrawPlane(Vector3 position, Vector2 dimensions, Quaternion rotation, Color color, bool isDrawCross)
        {
            if (color.a < Vector2.kEpsilon)
                return;
            var prevColor = Gizmos.color;
            Gizmos.color = color;
            var points = GeometricUtils.GetBorderPoints(position, dimensions, rotation);

            if (isDrawCross)
            {
                var right = points[3] - points[0];
                var up = points[1] - points[0];
                DrawSegment(points[0] + 0.5f * up, points[0] + right + 0.5f * up, color * 0.7f);
                DrawSegment(points[0] + 0.5f * right, points[0] + up + 0.5f * right, color * 0.7f);
            }

            DrawSegmentsLine(points, color, true);

            Gizmos.color = prevColor;
        }
        
        public static void DrawGrid2D(Vector2 minXZ, Vector2 maxXZ, Vector2 cellSize, float gridY, Color color)
        {
            if (color.a < Vector2.kEpsilon)
                return;
            float x0 = minXZ.x, x1 = maxXZ.x;
            float z0 = minXZ.y, z1 = maxXZ.y;
            for (var x = x0; x <= x1; x += cellSize.x)
            {
                var p0 = new Vector3(x, gridY, z0);
                var p1 = new Vector3(x, gridY, z1);
                if (EachSegmentCallback != null)
                    color = EachSegmentCallback.Invoke((p0, p1), color);
                DrawSegment(p0, p1, color);
            }
            for (var z = z0; z <= z1; z += cellSize.y)
            {
                var p0 = new Vector3(x0, gridY, z);
                var p1 = new Vector3(x1, gridY, z);
                if (EachSegmentCallback != null)
                    color = EachSegmentCallback.Invoke((p0, p1), color);
                DrawSegment(p0, p1, color);
            }
        }
        
        public static void DrawGrid3D(Vector3 min, Vector3 max, Vector3 cellSize, Color color)
        {
            if (color.a < Vector2.kEpsilon || cellSize.x <= 0 || cellSize.y <= 0 || cellSize.z <= 0)
                return;
            min = new Vector3(min.x * cellSize.x, min.y * cellSize.y, min.z *= cellSize.z);
            max = new Vector3(max.x * cellSize.x, max.y * cellSize.y, max.z *= cellSize.z);
            for (var z = min.z; z <= max.z; z += cellSize.z)
            {
                for (var y = min.y; y <= max.y; y += cellSize.y)
                {
                    var p0 = new Vector3(min.x, y, z);
                    var p1 = new Vector3(max.x, y, z);
                    if (EachSegmentCallback != null)
                        color = EachSegmentCallback.Invoke((p0, p1), color);
                    DrawSegment(p0, p1, color);
                }
            }

            for (var x = min.x; x <= max.x; x += cellSize.x)
            {
                for (var y = min.y; y <= max.y; y += cellSize.y)
                {
                    var p0 = new Vector3(x, y, min.z);
                    var p1 = new Vector3(x, y, max.z);
                    if (EachSegmentCallback != null)
                        color = EachSegmentCallback.Invoke((p0, p1), color);
                    DrawSegment(p0, p1, color);
                }
            }

            for (var x = min.x; x <= max.x; x += cellSize.x)
            {
                for (var z = min.z; z <= max.z; z += cellSize.z)
                {
                    var p0 = new Vector3(x, min.y, z);
                    var p1 = new Vector3(x, max.y, z);
                    if (EachSegmentCallback != null)
                        color = EachSegmentCallback.Invoke((p0, p1), color);
                    DrawSegment(p0, p1, color);
                }
            }
        }

        public static bool TryGetSelectedObjectPosition(out Vector3 position)
        {
#if UNITY_EDITOR
            position = Selection.activeTransform != null ? Selection.activeTransform.position : Vector3.zero;
            return Selection.activeTransform != null;
#endif
            position = Vector3.zero;
            return false;
        }
        
        public static bool TryViewportToWorldPointOnPlane(Vector2 viewportPoint, Plane plane, out Vector3 position)
        {
#if UNITY_EDITOR
            var sceneView = SceneView.lastActiveSceneView;
            if (sceneView == null || sceneView.camera == null)
            {
                position = Vector3.zero;
                return false;
            }

            var cam = sceneView.camera;
            var ray = cam.ViewportPointToRay(new Vector3(viewportPoint.x, viewportPoint.y, 0f));
            if (plane.Raycast(ray, out var enter))
            {
                position = ray.GetPoint(enter);
                return true;
            }
#endif

            position = Vector3.zero;
            return false;
        }

        public static Vector2 DrawNavigationCurveArrowHandles(Vector3 from, Vector3 to, Color color)
        {
            if (color.a < Vector2.kEpsilon)
                return Vector2.zero;

            return DrawCurveArrowHandlesInternal(from, to, color, 0.22f, 0.12f);
        }

        public static Vector2 DrawCurveArrowHandlesInternal(Vector3 from, Vector3 to, Color color, float headSize, float curvature)
        {
            var delta = to - from;
            var sqrMag = delta.sqrMagnitude;
            if (sqrMag <= Mathf.Epsilon)
                return Vector2.zero;

#if UNITY_EDITOR
            var prevColor = Handles.color;
            Handles.color = color;

            var dir = delta / Mathf.Sqrt(sqrMag);
            var camForward = SceneView.currentDrawingSceneView != null ? SceneView.currentDrawingSceneView.camera.transform.forward : Vector3.forward;

            var side = Vector3.Cross(camForward, dir).normalized;
            var distance = Mathf.Sqrt(sqrMag);
            var controlOffset = side * (distance * curvature);
            var p0 = from;
            var p1 = (from + to) * 0.5f + controlOffset;
            var p2 = to;
            
            var startScreenSize = HandleUtility.GetHandleSize(p0) * headSize;
            var endScreenSize = HandleUtility.GetHandleSize(p2) * headSize;

            Handles.SphereHandleCap(controlID: 0, position: p0, rotation: Quaternion.identity, size: startScreenSize, EventType.Repaint);

            const int segments = 16;
            var prevPoint = p0;
            for (var i = 1; i <= segments; i++)
            {
                var t = i / (float)segments;
                var point = GetQuadraticPoint(p0, p1, p2, t);
                Handles.DrawLine(prevPoint, point);
                prevPoint = point;
            }

            var tangent = GetQuadraticTangent(p0, p1, p2, 1f);
            if (tangent.sqrMagnitude > Mathf.Epsilon)
            {
                var dirT = tangent.normalized;
                var tip = p2 - dirT * endScreenSize * 2f;

                var rotation = Quaternion.LookRotation(dirT, camForward);

                Handles.ConeHandleCap(controlID: 0, position: tip, rotation: rotation, size: endScreenSize, EventType.Repaint);
            }

            Handles.color = prevColor;
            return p1;
#endif
            return Vector2.zero;
        }

        private static Vector3 GetQuadraticPoint(Vector3 p0, Vector3 p1, Vector3 p2, float t)
        {
            var oneMinusT = 1f - t;
            return oneMinusT * oneMinusT * p0 + 2f * oneMinusT * t * p1 + t * t * p2;
        }

        private static Vector3 GetQuadraticTangent(Vector3 p0, Vector3 p1, Vector3 p2, float t)
        {
            return 2f * (1f - t) * (p1 - p0) + 2f * t * (p2 - p1);
        }
        public static void DrawText(Vector3 position, string text, Color color)
        {
#if UNITY_EDITOR
            if (color.a < Vector2.kEpsilon)
                return;
            var prev = Handles.color;
            GUI.color = color;
            Handles.Label(position, text);
            GUI.color = prev;
#endif
        }

        public static void DrawTextOutline(Vector3 position, string text, Color color)
        {
#if UNITY_EDITOR
            if (string.IsNullOrEmpty(text) || color.a < Vector2.kEpsilon)
                return;

            var cam = SceneView.currentDrawingSceneView != null ? SceneView.currentDrawingSceneView.camera : Camera.current;

            var size = HandleUtility.GetHandleSize(position) * 0.02f;
            if (size <= 0f)
                size = 0.01f;

            var right = cam != null ? cam.transform.right : Vector3.right;
            var up = cam != null ? cam.transform.up : Vector3.up;

            var offset = (right + up) * size;

            var outlineColor = new Color(0f, 0f, 0f, color.a);

            var prevGuiColor = GUI.color;
            var prevHandlesColor = Handles.color;

            GUI.color = outlineColor;
            Handles.Label(position + offset, text);

            GUI.color = color;
            Handles.Label(position, text);

            GUI.color = prevGuiColor;
            Handles.color = prevHandlesColor;
#endif
        }
    }
}