using UnityEngine;

namespace DingoUnityExtensions.Utils
{
    public static class UIUtils
    {
        public static Vector2 WorldToCanvasSpace(Vector3 pointPosition, RectTransform parentCanvas, Camera cam)
        {
            var canvasPosition = CalculateCanvasPosition(pointPosition, parentCanvas, cam);
            return canvasPosition;
        }

        public static Vector2 WorldToCanvasSpaceClamped(Rect rect, RectTransform parentCanvas, Camera cam)
        {
            var pointPosition = new Vector3(rect.x, rect.y);
            var canvasPosition = CalculateCanvasPosition(pointPosition, parentCanvas, cam);
            var clampBounds = CalculateClampBounds(rect, Vector2.one * 0.5f);
            return ClampCanvasPosition(canvasPosition, clampBounds.min, clampBounds.max);
        }

        public static Vector2 WorldToCanvasSpaceClamped(Rect rect, Vector3 pointPosition, RectTransform parentCanvas, Camera cam)
        {
            var canvasPosition = CalculateCanvasPosition(pointPosition, parentCanvas, cam);
            var clampBounds = CalculateClampBounds(rect, Vector2.one * 0.5f);
            return ClampCanvasPosition(canvasPosition, clampBounds.min, clampBounds.max);
        }

        public static Vector2 WorldToCanvasSpaceClamped(Rect rect, Vector3 pointPosition, Vector2 pivot, RectTransform parentCanvas, Camera cam)
        {
            var canvasPosition = CalculateCanvasPosition(pointPosition, parentCanvas, cam);
            var clampBounds = CalculateClampBounds(rect, pivot);
            return ClampCanvasPosition(canvasPosition, clampBounds.min, clampBounds.max);
        }
        
        private static Vector2 CalculateCanvasPosition(Vector3 pointPosition, RectTransform parentCanvas, Camera cam)
        {
            var screenPoint = cam.WorldToScreenPoint(pointPosition);
            var sizeDelta = parentCanvas.sizeDelta;
            if (sizeDelta.magnitude < Vector2.kEpsilon)
                sizeDelta = parentCanvas.rect.size;
            return new Vector2(screenPoint.x * sizeDelta.x / Screen.width, screenPoint.y * sizeDelta.y / Screen.height);
        }

        private static Vector2 ClampCanvasPosition(Vector2 canvasPosition, Vector2 minBounds, Vector2 maxBounds)
        {
            var clampedX = Mathf.Clamp(canvasPosition.x, minBounds.x, maxBounds.x);
            var clampedY = Mathf.Clamp(canvasPosition.y, minBounds.y, maxBounds.y);
            return new Vector2(clampedX, clampedY);
        }

        private static Rect CalculateClampBounds(Rect rect, Vector2 pivot)
        {
            var w = rect.width;
            var h = rect.height;
            var minBounds = new Vector2(w * pivot.x, h * pivot.y);
            var maxBounds = new Vector2(Screen.width - w * (1 - pivot.x), Screen.height - h * (1 - pivot.y));
            return new Rect(minBounds, maxBounds - minBounds);
        }
    }
}
