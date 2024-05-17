using System;
using DG.Tweening;
using DingoUnityExtensions.Extensions;
using UnityEngine;

namespace DingoUnityExtensions.Tweens
{
    [Serializable]
    public class TransformSimpleBakeAnimation
    {
        [SerializeField] private EnableDisableTweenAnimationPair _moveTweenAnimation;
        [SerializeField] private EnableDisableTweenAnimationPair _rotationTweenAnimation;
        [SerializeField] private EnableDisableTweenAnimationPair _scaleTweenAnimation;
        [SerializeField] private EnableDisableTweenAnimationPair _fadeTweenAnimation;

        [SerializeField] private bool _isFade = true;
        [SerializeField] private float _enableDelay;
        [SerializeField] private float _disableDelay;
        
        [SerializeField] private Matrix4x4 _defaultTransform;
        [SerializeField] private Matrix4x4 _targetTransform;
        [SerializeField] private Matrix4x4 _disableTransform;
        
        private readonly TweenList _tweenList = new();

        public void SetDefaultValues(Transform transform, CanvasGroup canvasGroup)
        {
            _tweenList.Kill();
            transform.FromMatrix(_defaultTransform);
            if (_isFade)
                canvasGroup.alpha = 0;
        }

        public void SetTargetValues(Transform transform, CanvasGroup canvasGroup)
        {
            transform.FromMatrix(_targetTransform);
            if (_isFade)
                canvasGroup.alpha = 1;
        }

        public void SetDisableValues(Transform transform, CanvasGroup canvasGroup)
        {
            transform.FromMatrix(_disableTransform);
            if (_isFade)
                canvasGroup.alpha = 0;
        }
        
        public void Enable(Transform transform, CanvasGroup canvasGroup)
        {
            _tweenList.Kill();
            var (position, rotation, scale) = _targetTransform.FromMatrix();
            MakeEnableAndAdd(_moveTweenAnimation, d => transform.DOLocalMove(position, d));
            MakeEnableAndAdd(_rotationTweenAnimation, d => transform.DOLocalRotate(rotation.eulerAngles, d));
            MakeEnableAndAdd(_scaleTweenAnimation, d => transform.DOScale(scale, d));
            if (_isFade)
                MakeEnableAndAdd(_fadeTweenAnimation, d => canvasGroup.DOFade(1, d));
            _tweenList.Play();
        }

        public void Disable(Transform transform, CanvasGroup canvasGroup, Action onDisableCompletely = null)
        {
            _tweenList.Kill();
            var (position, rotation, scale) = _disableTransform.FromMatrix();
            MakeDisableAndAdd(_moveTweenAnimation, d => transform.DOLocalMove(position, d));
            MakeDisableAndAdd(_rotationTweenAnimation, d => transform.DOLocalRotate(rotation.eulerAngles, d));
            MakeDisableAndAdd(_scaleTweenAnimation, d => transform.DOScale(scale, d));
            if (_isFade)
                MakeDisableAndAdd(_fadeTweenAnimation, d => canvasGroup.DOFade(0, d));
            _tweenList.Play(onDisableCompletely);
        }
        
        public void BakeDefaultValues(Transform transform) => _defaultTransform = Matrix4x4.TRS(transform.localPosition, transform.localRotation, transform.localScale);
        public void BakeTargetValues(Transform transform) => _targetTransform = Matrix4x4.TRS(transform.localPosition, transform.localRotation, transform.localScale);
        public void BakeDisableValues(Transform transform) => _disableTransform = Matrix4x4.TRS(transform.localPosition, transform.localRotation, transform.localScale);

        private void MakeDisableAndAdd(EnableDisableTweenAnimationPair animation, Func<float, Tween> tweenFactoryMethod)
        {
            if (animation == null)
                return;

            var tween = animation.MakeDisableTween(tweenFactoryMethod, out var fullDuration).SetDelay(animation.DisableDelay + _disableDelay);
            _tweenList.Add(tween, fullDuration);
        }
        
        private void MakeEnableAndAdd(EnableDisableTweenAnimationPair animation, Func<float, Tween> tweenFactoryMethod)
        {
            if (animation == null)
                return;

            var tween = animation.MakeEnableTween(tweenFactoryMethod, out var fullDuration).SetDelay(animation.EnableDuration + _enableDelay);
            _tweenList.Add(tween, fullDuration);
        }
    }
}