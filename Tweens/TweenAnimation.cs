using System;
using DG.Tweening;
using UnityEngine;

namespace DingoUnityExtensions.Tweens
{
    [CreateAssetMenu(menuName = "Tween Animations/ Create TweenAnimation", fileName = "TweenAnimation", order = 0)]
    public class TweenAnimation : ScriptableObject
    {
        [SerializeField] private AnimationCurve _animationCurve;
        [SerializeField] private float _duration;
        [SerializeField] private float _delay;

        public bool IsInstant => _duration < Vector2.kEpsilon && _delay < Vector2.kEpsilon;
        
        public Tween Do(Func<float, Tween> tweenFactoryMethod)
        {
            var tween = tweenFactoryMethod(_duration);
            if (_delay > Vector2.kEpsilon)
                tween.SetDelay(_delay);
            if (_animationCurve != null)
                tween.SetEase(_animationCurve);
            return tween;
        }
    }
}