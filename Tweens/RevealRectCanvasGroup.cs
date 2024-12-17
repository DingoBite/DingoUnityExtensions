using System.Collections.Generic;
using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;

namespace DingoUnityExtensions.Tweens
{
    public class RevealRectCanvasGroup : AnimatableBehaviour
    {
        [SerializeField] protected CanvasGroup CanvasGroup;
        [SerializeField] protected EnableDisableTweenAnimationPair FadeAnimation;
        [SerializeField] private float _addictiveEnableDelay;
        [SerializeField] private float _addictiveDisableDelay;
        [SerializeField] private bool _bakeButtons;

        [SerializeField] private RectTransformAnimationEndpoints _bakeAnimationEndpoints;

        private RectTransform _rectTransform;
        
        protected override bool ValidAnimationParams => IsFade || IsTransformTranslation;
        
        private bool IsFade => FadeAnimation != null;
        private bool IsTransformTranslation => _bakeAnimationEndpoints.ValidAnimation;

        private RectTransform RectTransform
        {
            get
            {
                if (_rectTransform == null)
                    _rectTransform = GameObject.GetComponent<RectTransform>();

                return _rectTransform;
            }
        }

        protected override IEnumerable<Tween> CollectEnableTweens(bool isPlaying, float addDelay)
        {
            if (IsFade && CanvasGroup != null)
            {
                var animationDelay = isPlaying ? 0 : FadeAnimation.EnableDelay + addDelay + _addictiveEnableDelay;
                yield return FadeAnimation.MakeEnableTween(d => CanvasGroup.DOFade(1, d)).SetDelay(animationDelay);
            }
            if (IsTransformTranslation)
            {
                foreach (var tween in _bakeAnimationEndpoints.Enable(RectTransform, addDelay: isPlaying ? 0 : addDelay))
                {
                    yield return tween;
                }
            }
        }

        protected override IEnumerable<Tween> CollectDisableTweens(bool isPlaying, float addDelay)
        {
            if (IsFade && CanvasGroup != null)
            {
                var animationDelay = isPlaying ? 0 : FadeAnimation.DisableDelay + addDelay + _addictiveDisableDelay;
                yield return FadeAnimation.MakeDisableTween(d => CanvasGroup.DOFade(IsFade ? 0 : 1, d)).SetDelay(animationDelay);
            }
            if (IsTransformTranslation)
            {
                foreach (var tween in _bakeAnimationEndpoints.Disable(RectTransform, addDelay: isPlaying ? 0 : addDelay))
                {
                    yield return tween;
                }
            }
        }

        protected override void SetFullActive(AnimateState state, bool force = false)
        {
            base.SetFullActive(state, force);
            if (state is AnimateState.Enabled)
            {
                if (force && CanvasGroup != null)
                    CanvasGroup.alpha = IsFade ? 1 : CanvasGroup.alpha;
                if (IsTransformTranslation)
                    _bakeAnimationEndpoints.SetTargetValues(RectTransform);
            }
            else if (state is AnimateState.Disabled)
            {
                if (force && CanvasGroup != null)
                    CanvasGroup.alpha = IsFade ? 0 : CanvasGroup.alpha;
                if (IsTransformTranslation)
                {
                    if (force)
                        _bakeAnimationEndpoints.SetDefaultValues(RectTransform);
                    else 
                        _bakeAnimationEndpoints.SetDisableValues(RectTransform);
                }
            }
        }

        [Button]
        private void SetDefaultValues()
        {
            _bakeAnimationEndpoints.SetDefaultValues(RectTransform);
            foreach (var animatableBehaviour in Stack)
            {
                if (animatableBehaviour is RevealRectCanvasGroup revealCanvasGroup)
                    revealCanvasGroup._bakeAnimationEndpoints.SetDefaultValues(revealCanvasGroup.RectTransform);
            }
        }

        [Button]
        private void SetTargetValues()
        {
            _bakeAnimationEndpoints.SetTargetValues(RectTransform);
            foreach (var animatableBehaviour in Stack)
            {
                if (animatableBehaviour is RevealRectCanvasGroup revealCanvasGroup)
                    revealCanvasGroup._bakeAnimationEndpoints.SetTargetValues(revealCanvasGroup.RectTransform);
            }
        }

        [Button]
        private void SetDisableValues()
        {
            _bakeAnimationEndpoints.SetDisableValues(RectTransform);
            foreach (var animatableBehaviour in Stack)
            {
                if (animatableBehaviour is RevealRectCanvasGroup revealCanvasGroup)
                    revealCanvasGroup._bakeAnimationEndpoints.SetDisableValues(revealCanvasGroup.RectTransform);
            }
        }

        [Button, ShowIf(nameof(_bakeButtons))]
        private void BakeDefaultValues() => _bakeAnimationEndpoints.BakeDefaultValues(RectTransform);
        [Button, ShowIf(nameof(_bakeButtons))]
        private void BakeTargetValues() => _bakeAnimationEndpoints.BakeTargetValues(RectTransform);
        [Button, ShowIf(nameof(_bakeButtons))]
        private void BakeDisableValues() => _bakeAnimationEndpoints.BakeDisableValues(RectTransform);
    }
}