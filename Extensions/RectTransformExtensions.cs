using UnityEngine;

namespace DingoUnityExtensions.Extensions
{
    public static class RectTransformExtensions
    {
        public static bool IsPointInRectTransform(this RectTransform rectTransform, Vector2 point, Camera camera)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, point, camera, out var lp);
            return rectTransform.rect.Contains(lp);
        }
        
        public static bool IsPointInRectTransform(this RectTransform rectTransform, Vector2 point)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, point, Camera.main, out var lp);
            return rectTransform.rect.Contains(lp);
        }
        
        public static Rect GetWorldRect(this RectTransform rectTransform)
        {
            var localRect = rectTransform.rect;

            return new Rect
            {
                min = rectTransform.TransformPoint(localRect.min),
                max = rectTransform.TransformPoint(localRect.max)
            };
        }

        public static bool WorldOverlaps(this RectTransform rect1, RectTransform rect2)
        {
            var worldRect1 = GetWorldRect(rect1);
            var worldRect2 = GetWorldRect(rect2);
            return worldRect1.Overlaps(worldRect2);
        }
        
        public static bool WorldOverlaps(this RectTransform rect1, RectTransform rect2, out Rect worldRect1, out Rect worldRect2)
        {
            worldRect1 = GetWorldRect(rect1);
            worldRect2 = GetWorldRect(rect2);
            return worldRect1.Overlaps(worldRect2);
        }
        
        public static float GetNormalizedHeightRelativeDistance(this RectTransform rect1, RectTransform rect2, out bool isOverlap)
        {
            isOverlap = rect1.WorldOverlaps(rect2, out var worldRect1, out var worldRect2);
            var worldRect2Height = worldRect2.y + worldRect2.height * 0.5f - worldRect1.y - worldRect1.height * 0.5f;
            // Debug.Log(worldRect2Height);
            // return worldRect2Height;
            var maxDist = worldRect2.height + worldRect1.height;
            var distance = worldRect1.height * 0.5f - worldRect1.y;
            var normalizedDist = worldRect2Height / maxDist;
            return normalizedDist;
        }

        public static float GetNormalizedHeightRelativeDistance(this RectTransform rect1, RectTransform rect2) =>
            GetNormalizedHeightRelativeDistance(rect1, rect2, out _);
    }
}