using System;
using DG.Tweening;
using UnityEngine;

namespace DingoUnityExtensions.Tweens
{
    [CreateAssetMenu(menuName = "Tween Animations/ Create TweenAnimationPair", fileName = "TweenAnimationPair", order = 0)]
    public class EnableDisableTweenAnimationPair : ScriptableObject
    {
        [SerializeField] private AnimationCurve _enableAnimationCurve;
        [SerializeField] private float _enableDuration;
        [SerializeField] private float _enableDelay;
        
        [SerializeField] private AnimationCurve _disableAnimationCurve;
        [SerializeField] private float _disableDuration;
        [SerializeField] private float _disableDelay;

        public float EnableDuration => _enableDelay + _enableDuration;
        public float EnableDelay => _enableDelay;
        public float DisableDuration => _disableDelay + _disableDuration;
        public float DisableDelay => _disableDelay;

        public AnimationCurve EnableAnimationCurve => _enableAnimationCurve;
        public AnimationCurve DisableAnimationCurve => _disableAnimationCurve;
        
        public Tween MakeEnableTween(Func<float, Tween> tweenFactoryMethod) => MakeEnableTween(tweenFactoryMethod, out _);
        
        public Tween MakeEnableTween(Func<float, Tween> tweenFactoryMethod, out float fullDuration)
        {
            var tween = tweenFactoryMethod(_enableDuration);
            if (_enableDelay > Vector2.kEpsilon)
                tween.SetDelay(_enableDelay);
            if (_enableAnimationCurve != null)
                tween.SetEase(_enableAnimationCurve);
            fullDuration = _enableDelay + _enableDuration;
            return tween;
        }

        public Tween MakeDisableTween(Func<float, Tween> tweenFactoryMethod) => MakeDisableTween(tweenFactoryMethod, out _);
        public Tween MakeDisableTween(Func<float, Tween> tweenFactoryMethod, out float fullDuration)
        {
            var tween = tweenFactoryMethod(_disableDuration);
            if (_disableDelay > Vector2.kEpsilon)
                tween.SetDelay(_disableDelay);
            if (_disableAnimationCurve != null)
                tween.SetEase(_disableAnimationCurve);
            fullDuration = _disableDelay + _disableDuration;
            return tween;
        }
    }
}