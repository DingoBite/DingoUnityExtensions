using Unity.Collections;
using UnityEngine;

namespace DingoUnityExtensions.Extensions
{
    public static class CameraExtensions
    {
        public static bool IsObjectVisible(this Camera c, Renderer renderer)
        {
            return GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(c), renderer.bounds);
        }

        public static bool IsObjectFullyVisible(this Camera c, Renderer renderer)
        {
            var bounds = renderer.bounds;
            var points = new NativeArray<Vector3>(8, Allocator.Temp);
            var ext = bounds.extents;
            var center = bounds.center;

            points[0] = center + new Vector3(-ext.x, -ext.y, -ext.z);
            points[1] = center + new Vector3(ext.x, -ext.y, -ext.z);
            points[2] = center + new Vector3(ext.x, -ext.y, ext.z);
            points[3] = center + new Vector3(-ext.x, -ext.y, ext.z);
            points[4] = center + new Vector3(-ext.x, ext.y, -ext.z);
            points[5] = center + new Vector3(ext.x, ext.y, -ext.z);
            points[6] = center + new Vector3(ext.x, ext.y, ext.z);
            points[7] = center + new Vector3(-ext.x, ext.y, ext.z);

            foreach (var p in points)
            {
                var vp = c.WorldToViewportPoint(p);
                var visible = vp is { z: > 0, x: >= 0 and <= 1, y: >= 0 and <= 1 };
                if (!visible)
                    return false;
            }
            return true;
        }
        
        public static Matrix4x4 GetMatrixForPerspective(this Camera c)
        {
            var prevState = c.orthographic;
            c.orthographic = false;
            c.ResetProjectionMatrix();
            var cameraProjectionMatrix = c.projectionMatrix;
            c.orthographic = prevState;
            return cameraProjectionMatrix;
        }
        
        public  static Matrix4x4 GetMatrixForOrthographic(this Camera c)
        {
            var prevState = c.orthographic;
            c.orthographic = true;
            c.ResetProjectionMatrix();
            var cameraProjectionMatrix = c.projectionMatrix;
            c.orthographic = prevState;
            return cameraProjectionMatrix;
        }
    }
}