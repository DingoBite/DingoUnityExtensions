using System;
using System.Collections.Generic;
using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;

namespace DingoUnityExtensions.Tweens
{
    public class RevealCanvasGroup : AnimatableBehaviour
    {
        [SerializeField] protected CanvasGroup CanvasGroup;
        [SerializeField] protected EnableDisableTweenAnimationPair FadeAnimation;
        [SerializeField] private float _addictiveEnableDelay;
        [SerializeField] private float _addictiveDisableDelay;
        [SerializeField] private bool _bakeButtons;
        [SerializeField] private bool _manageInteractable;

        [SerializeField] private TransformAnimationEndpoints _bakeAnimationEndpoints;

        protected override bool ValidAnimationParams => IsFade || IsTransformTranslation;
        
        private bool IsFade => FadeAnimation != null;
        private bool IsTransformTranslation => _bakeAnimationEndpoints.ValidAnimation;
        
        protected override IEnumerable<Tween> CollectEnableTweens(bool isPlaying, float addDelay)
        {
            if (IsFade && CanvasGroup != null)
            {
                var animationDelay = isPlaying ? 0 : FadeAnimation.EnableDelay + addDelay + _addictiveEnableDelay;
                yield return FadeAnimation.MakeEnableTween(d => CanvasGroup.DOFade(1, d)).SetDelay(animationDelay);
            }
            if (IsTransformTranslation)
            {
                foreach (var tween in _bakeAnimationEndpoints.Enable(GameObject.transform, addDelay: isPlaying ? 0 : addDelay))
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
                foreach (var tween in _bakeAnimationEndpoints.Disable(GameObject.transform, addDelay: isPlaying ? 0 : addDelay))
                {
                    yield return tween;
                }
            }
        }

        protected override void SetFullActive(AnimateState state, bool force = false)
        {
            base.SetFullActive(state, force);

            if (_manageInteractable && CanvasGroup != null)
            {
                if (state is AnimateState.Enabling or AnimateState.Enabled)
                    CanvasGroup.interactable = true;
                else
                    CanvasGroup.interactable = false;
            }
            
            if (state is AnimateState.Enabled)
            {
                if (force && CanvasGroup != null)
                    CanvasGroup.alpha = IsFade ? 1 : CanvasGroup.alpha;
                if (IsTransformTranslation)
                    _bakeAnimationEndpoints.SetTargetValues(GameObject.transform);
            }
            else if (state is AnimateState.Disabled)
            {
                if (force && CanvasGroup != null)
                    CanvasGroup.alpha = IsFade ? 0 : CanvasGroup.alpha;
                if (IsTransformTranslation)
                {
                    if (force)
                        _bakeAnimationEndpoints.SetDefaultValues(GameObject.transform);
                    else 
                        _bakeAnimationEndpoints.SetDisableValues(GameObject.transform);
                }
            }
        }

        [Button]
        private void SetDefaultValues()
        {
            _bakeAnimationEndpoints.SetDefaultValues(GameObject.transform);
            foreach (var animatableBehaviour in Stack)
            {
                if (animatableBehaviour is RevealCanvasGroup revealCanvasGroup)
                    revealCanvasGroup._bakeAnimationEndpoints.SetDefaultValues(revealCanvasGroup.GameObject.transform);
            }
        }

        [Button]
        private void SetTargetValues()
        {
            _bakeAnimationEndpoints.SetTargetValues(GameObject.transform);
            foreach (var animatableBehaviour in Stack)
            {
                if (animatableBehaviour is RevealCanvasGroup revealCanvasGroup)
                    revealCanvasGroup._bakeAnimationEndpoints.SetTargetValues(revealCanvasGroup.GameObject.transform);
            }
        }

        [Button]
        private void SetDisableValues()
        {
            _bakeAnimationEndpoints.SetDisableValues(GameObject.transform);
            foreach (var animatableBehaviour in Stack)
            {
                if (animatableBehaviour is RevealCanvasGroup revealCanvasGroup)
                    revealCanvasGroup._bakeAnimationEndpoints.SetDisableValues(revealCanvasGroup.GameObject.transform);
            }
        }

        [Button, ShowIf(nameof(_bakeButtons))]
        private void BakeDefaultValues() => _bakeAnimationEndpoints.BakeDefaultValues(GameObject.transform);
        [Button, ShowIf(nameof(_bakeButtons))]
        private void BakeTargetValues() => _bakeAnimationEndpoints.BakeTargetValues(GameObject.transform);
        [Button, ShowIf(nameof(_bakeButtons))]
        private void BakeDisableValues() => _bakeAnimationEndpoints.BakeDisableValues(GameObject.transform);
    }
}