using UnityEngine;

namespace DingoUnityExtensions.MonoBehaviours.UI.PointsGraphics.Core
{
    public interface ITouchPointComponent
    {
        public Vector2 GetTouchPoint(Vector2 p1, Vector2 p2);
    }
}