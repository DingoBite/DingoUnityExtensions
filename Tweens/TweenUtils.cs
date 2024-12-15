using DG.Tweening;
using UnityEngine;

namespace DingoUnityExtensions.Tweens
{
    public static class TweenUtils
    {
        public delegate Tween Factory(float duration);
        
        public static Tween MakeTween(Factory tweenFactoryMethod, out float fullDuration, float addDelay, float duration, float delay, AnimationCurve animationCurve)
        {
            var tween = tweenFactoryMethod(duration);
            var enableDelay = delay + addDelay;
            if (enableDelay > Vector2.kEpsilon)
                tween.SetDelay(enableDelay);
            if (animationCurve != null)
                tween.SetEase(animationCurve);
            fullDuration = enableDelay + duration;
            return tween;
        }
    }
}