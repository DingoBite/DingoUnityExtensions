using System.Collections.Generic;
using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;

namespace DingoUnityExtensions.Tweens
{
    public class RevealCanvasGroup : AnimatableBehaviour
    {
        [SerializeField] protected CanvasGroup CanvasGroup;
        [SerializeField] private bool _fade = true;
        [SerializeField] protected EnableDisableTweenAnimationPair FadeAnimation;

        [SerializeField, ShowIf(nameof(ValidAnimationParams))] private bool _useTransformTranslation;
        [SerializeField, ShowIf(nameof(ValidAnimationParams))] private bool _bakeButtons;

        [SerializeField, ShowIf(nameof(ValidAnimationParams))] private TransformAnimationEndpoints _bakeAnimationEndpoints;

        protected override bool ValidAnimationParams => ValidFadeAnimationParams && ValidTransformAnimationParams;
        
        private bool ValidTransformAnimationParams => !_useTransformTranslation || _useTransformTranslation && _bakeAnimationEndpoints != null;
        private bool ValidFadeAnimationParams => !_fade || _fade && FadeAnimation != null;

        protected override IEnumerable<Tween> CollectEnableTweens(bool isPlaying, float addDelay)
        {
            var animationDelay = isPlaying ? 0 : FadeAnimation.EnableDelay + addDelay;
            yield return FadeAnimation.MakeEnableTween(d => CanvasGroup.DOFade(1, d)).SetDelay(animationDelay);
            if (_useTransformTranslation)
            {
                foreach (var tween in _bakeAnimationEndpoints.Enable(transform, addDelay: isPlaying ? 0 : addDelay))
                {
                    yield return tween;
                }
            }
        }

        protected override IEnumerable<Tween> CollectDisableTweens(bool isPlaying, float addDelay)
        {
            var animationDelay = isPlaying ? 0 : FadeAnimation.DisableDelay + addDelay;
            yield return FadeAnimation.MakeDisableTween(d => CanvasGroup.DOFade(_fade ? 0 : 1, d)).SetDelay(animationDelay);
            if (_useTransformTranslation)
            {
                foreach (var tween in _bakeAnimationEndpoints.Disable(transform, addDelay: isPlaying ? 0 : addDelay))
                {
                    yield return tween;
                }
            }
        }

        protected override void SetFullActive(bool value, bool force = false)
        {
            base.SetFullActive(value, force);
            if (value)
            {
                if (force)
                    CanvasGroup.alpha = _fade ? 1 : CanvasGroup.alpha;
                if (_useTransformTranslation)
                    _bakeAnimationEndpoints.SetTargetValues(transform);
            }
            else
            {
                if (force)
                    CanvasGroup.alpha = _fade ? 0 : CanvasGroup.alpha;
                if (_useTransformTranslation)
                    _bakeAnimationEndpoints.SetDisableValues(transform);
            }
        }

        [Button]
        private void SetDefaultValues() => _bakeAnimationEndpoints.SetDefaultValues(transform);
        [Button]
        private void SetTargetValues() => _bakeAnimationEndpoints.SetTargetValues(transform);
        [Button]
        private void SetDisableValues() => _bakeAnimationEndpoints.SetDisableValues(transform);
        
        [Button, ShowIf(nameof(_bakeButtons))]
        private void BakeDefaultValues() => _bakeAnimationEndpoints.BakeDefaultValues(transform);
        [Button, ShowIf(nameof(_bakeButtons))]
        private void BakeTargetValues() => _bakeAnimationEndpoints.BakeTargetValues(transform);
        [Button, ShowIf(nameof(_bakeButtons))]
        private void BakeDisableValues() => _bakeAnimationEndpoints.BakeDisableValues(transform);
    }
}