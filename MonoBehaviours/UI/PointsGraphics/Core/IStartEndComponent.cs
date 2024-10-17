using UnityEngine;

namespace DingoUnityExtensions.MonoBehaviours.UI.PointsGraphics.Core
{
    public interface IStartEndComponent
    {
        public void SetPoints(Vector2 start, Vector2 end);
    }
}