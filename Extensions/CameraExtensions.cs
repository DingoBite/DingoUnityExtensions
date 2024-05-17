using UnityEngine;

namespace DingoUnityExtensions.Extensions
{
    public static class CameraExtensions
    {
        public static bool IsObjectVisible(this Camera c, Renderer renderer)
        {
            return GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(c), renderer.bounds);
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