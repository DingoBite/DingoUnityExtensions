using System;

namespace DingoUnityExtensions.MonoBehaviours.PixelTransform.Movement.Core
{
    public interface IPixelMove
    {
        public event Action<IPixelMovable> Arrived;
        public bool IsActive { get; }
        public bool Tick(float deltaTime, IPixelMovable movable);
        public void Resume();
        public void Pause();
    }
}