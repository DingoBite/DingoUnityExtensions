using DingoUnityExtensions.MonoBehaviours.UI.PointsGraphics.Core;
using UnityEngine;

namespace DingoUnityExtensions.MonoBehaviours.UI.PointsGraphics
{
    public static class Utils
    {
        public static (Vector2 startOffset, Vector2 endOffset) GetRadiusOffset(IClosestPointComponent c1, IClosestPointComponent c2, 
            ref Vector2 start, ref Vector2 end, float startShoulder = 0, float endShoulder = 0)
        {
            var dir = end - start;
            start -= dir.normalized * startShoulder;
            end += dir.normalized * endShoulder;

            var startOffset = -c1.GetLookClosestPoint(end, start);
            var endOffset = -c2.GetLookClosestPoint(start, end);
            return (startOffset, endOffset);
        }
        
        public static Vector2 GetRadiusOffset(IClosestPointComponent c, ref Vector2 start, ref Vector2 end, float startShoulder = 0, float endShoulder = 0)
        {
            var dir = end - start;
            start -= dir.normalized * startShoulder;
            end += dir.normalized * endShoulder;

            var offset = -c.GetLookClosestPoint(end, start);
            return offset;
        }
        
        public static ((Vector2 top, Vector2 bottom), (Vector2 top, Vector2 bottom))
            GetRadiusTouchOffset(ITouchPointComponent c1, ITouchPointComponent c2, ref Vector2 start, ref Vector2 end, float startShoulder = 0, float endShoulder = 0)
        {
            var startTouchPoint = c1.GetTouchPoint(end, start);
            var endTouchPoint = c2.GetTouchPoint(start, end);
            startTouchPoint -= startTouchPoint.normalized * startShoulder;
            endTouchPoint -= endTouchPoint.normalized * endShoulder;
            return ((-startTouchPoint, startTouchPoint), (-endTouchPoint, endTouchPoint));
        }
        
        public static (Vector2 topTouch, Vector2 bottomTouch) GetRadiusTouchOffset(ITouchPointComponent c, ref Vector2 start, ref Vector2 end, float startShoulder = 0, float endShoulder = 0)
        {
            var dir = end - start;
            start -= dir.normalized * startShoulder;
            end += dir.normalized * endShoulder;

            var offset = c.GetTouchPoint(end, start);
            return (offset, -offset);
        }
    }
}