using UnityEngine;

namespace DingoUnityExtensions.Utils
{
    public static class UIUtils
    {
        public static Vector2 WorldToCanvasSpaceClamped(Vector3 worldPoint, RectTransform rectToClampPos, RectTransform parentRect, Camera worldCamera, Camera uiCamera)
        {
            var screenPos = RectTransformUtility.WorldToScreenPoint(worldCamera, worldPoint);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, screenPos, uiCamera, out var localPos);

            var halfW = rectToClampPos.rect.width * rectToClampPos.pivot.x;
            var halfH = rectToClampPos.rect.height * rectToClampPos.pivot.y;

            var minX = -parentRect.rect.width * parentRect.pivot.x + halfW;
            var maxX = parentRect.rect.width * (1f - parentRect.pivot.x) - (rectToClampPos.rect.width - halfW);
            var minY = -parentRect.rect.height * parentRect.pivot.y + halfH;
            var maxY = parentRect.rect.height * (1f - parentRect.pivot.y) - (rectToClampPos.rect.height - halfH);

            localPos.x = Mathf.Clamp(localPos.x, minX, maxX);
            localPos.y = Mathf.Clamp(localPos.y, minY, maxY);

            return localPos;
        }
    }
}