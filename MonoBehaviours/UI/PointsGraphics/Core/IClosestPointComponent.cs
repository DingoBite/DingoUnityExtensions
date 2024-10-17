using UnityEngine;

namespace DingoUnityExtensions.MonoBehaviours.UI.PointsGraphics.Core
{
    public interface IClosestPointComponent
    {
        public Vector2 GetLookClosestPoint(Vector2 p1, Vector2 p2);
    }
}