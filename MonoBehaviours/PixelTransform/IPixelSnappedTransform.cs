using UnityEngine;

namespace DingoUnityExtensions.MonoBehaviours.PixelTransform
{
    public interface IPixelSnappedTransform
    {
        void SetupPixelPerfectCamera(Camera c);
    }
}