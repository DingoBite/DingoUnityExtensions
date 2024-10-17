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

        public float DisableDelay => _disableDelay;
        public float EnableDelay => _enableDelay;
        public float DisableDuration => DisableDelay + _disableDuration;
        public float EnableDuration => EnableDelay + _enableDuration;
        
        public Tween MakeEnableTween(Func<float, Tween> tweenFactoryMethod) => MakeEnableTween(tweenFactoryMethod, out _);
        public Tween MakeEnableTween(Func<float, Tween> tweenFactoryMethod, float addDelay) => MakeEnableTween(tweenFactoryMethod, out _, addDelay);

        public Tween MakeEnableTween(Func<float, Tween> tweenFactoryMethod, out float fullDuration, float addDelay = 0)
        {
            var tween = tweenFactoryMethod(_enableDuration);
            var enableDelay = _enableDelay + addDelay;
            if (enableDelay > Vector2.kEpsilon)
                tween.SetDelay(enableDelay);
            if (_enableAnimationCurve != null)
                tween.SetEase(_enableAnimationCurve);
            fullDuration = enableDelay + _enableDuration;
            return tween;
        }

        public Tween MakeDisableTween(Func<float, Tween> tweenFactoryMethod) => MakeDisableTween(tweenFactoryMethod, out _);
        public Tween MakeDisableTween(Func<float, Tween> tweenFactoryMethod, float addDelay) => MakeDisableTween(tweenFactoryMethod, out _, addDelay);
        public Tween MakeDisableTween(Func<float, Tween> tweenFactoryMethod, out float fullDuration, float addDelay = 0)
        {
            var tween = tweenFactoryMethod(_disableDuration);
            var disableDelay = _disableDelay + addDelay;
            if (disableDelay > Vector2.kEpsilon)
                tween.SetDelay(disableDelay);
            if (_disableAnimationCurve != null)
                tween.SetEase(_disableAnimationCurve);
            fullDuration = disableDelay + _disableDuration;
            return tween;
        }
    }
}