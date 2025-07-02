using UnityEngine;

namespace DingoUnityExtensions.Utils
{
    public static class UIUtils
    {
        public static Vector2 WorldToCanvasSpaceClamped(
            Vector3 worldPoint,
            RectTransform rectToClampPos,
            RectTransform parentRect,
            Camera worldCamera, Camera uiCamera)
        {
            // World → screen
            Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(worldCamera, worldPoint);

            // Screen → local (anchored) in parent
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect, screenPos, uiCamera, out var localPos);

            // Clamp so the rect stays fully inside parent
            float halfW = rectToClampPos.rect.width  * rectToClampPos.pivot.x;
            float halfH = rectToClampPos.rect.height * rectToClampPos.pivot.y;

            float minX = -parentRect.rect.width  * parentRect.pivot.x + halfW;
            float maxX =  parentRect.rect.width  * (1f - parentRect.pivot.x) - (rectToClampPos.rect.width  - halfW);
            float minY = -parentRect.rect.height * parentRect.pivot.y + halfH;
            float maxY =  parentRect.rect.height * (1f - parentRect.pivot.y) - (rectToClampPos.rect.height - halfH);

            localPos.x = Mathf.Clamp(localPos.x, minX, maxX);
            localPos.y = Mathf.Clamp(localPos.y, minY, maxY);

            return localPos; // assign to rectToClampPos.anchoredPosition afterwards
        }
    }
}
