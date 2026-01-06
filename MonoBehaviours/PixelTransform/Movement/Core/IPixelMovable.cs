using UnityEngine;

namespace DingoUnityExtensions.MonoBehaviours.PixelTransform.Movement.Core
{
    public interface IPixelMovable
    {
        public Vector2 PositionPx { get; set; }
        public Vector2Int PositionPxRounded { get; }

        public void SetPositionPx(Vector2Int px);
        public void SnapPosition();
    }
}