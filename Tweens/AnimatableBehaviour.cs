using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace DingoUnityExtensions.Tweens
{
    public abstract class AnimatableBehaviour : MonoBehaviour
    {
        protected enum AnimateState
        {
            Enabling,
            Enabled,
            Disabling,
            Disabled
        }

        [field: SerializeField] protected bool RestartPlayingTween { get; private set; }
        
        protected abstract bool ValidAnimationParams { get; }
        protected AnimateState State { get; private set; }

        private readonly TweenList _enableTweens = new();
        private readonly TweenList _disableTweens = new();
        
        public void Enable(float addDelay = 0, Action onComplete = null)
        {
            if (!ValidAnimationParams)
            {
                EnableImmediately();
                return;
            }

            SetFullActive(true);
            var isPlaying = IsPlaying(_enableTweens, out var remainingTime);
            if (!RestartPlayingTween && (isPlaying || State is AnimateState.Enabling or AnimateState.Enabled))
                return;
            
            Kill();
            State = AnimateState.Enabling;
            foreach (var enableTween in CollectEnableTweens(isPlaying, addDelay))
            {
                _enableTweens.AddWithDuration(enableTween);
            }
            _enableTweens.Play(() =>
            {
                onComplete?.Invoke();
                EnableImmediately();
            });
        }

        public void Disable(float addDelay = 0, Action onComplete = null)
        {
            State = AnimateState.Disabling;
            if (!ValidAnimationParams)
            {
                DisableImmediately();
                return;
            }
            
            var isPlaying = IsPlaying(_disableTweens, out var remainingTime);
            if (!RestartPlayingTween && (isPlaying || State is AnimateState.Disabling or AnimateState.Disabled))
                return;
            
            Kill();
            State = AnimateState.Disabling;
            foreach (var disableTween in CollectDisableTweens(isPlaying, addDelay))
            {
                _disableTweens.AddWithDuration(disableTween);
            }
            _disableTweens.Play(() =>
            {
                onComplete?.Invoke();
                DisableImmediately();
            });
        }

        public void EnableImmediately()
        {
            State = AnimateState.Enabled;
            SetFullActive(true, true);
            if (!ValidAnimationParams)
                return;
            
            Kill();
        }

        public void DisableImmediately()
        {
            State = AnimateState.Disabled;
            SetFullActive(false, true);
            if (!ValidAnimationParams)
                return;

            Kill();
        }

        protected abstract IEnumerable<Tween> CollectEnableTweens(bool isPlaying, float addDelay);
        protected abstract IEnumerable<Tween> CollectDisableTweens(bool isPlaying, float addDelay);
        
        protected virtual void SetFullActive(bool value, bool force = false)
        {
            gameObject.SetActive(value);
            enabled = value;
        }

        private static bool IsPlaying(TweenList tweenList, out float remainingTime)
        {
            remainingTime = 0;
            if (tweenList.MaxTween != null && tweenList.IsPlaying())
            {
                remainingTime = (1 - tweenList.MaxTween.ElapsedPercentage()) * tweenList.MaxTween.Duration();
                return true;
            }

            return false;
        }
        
        private void Kill()
        {
            _enableTweens.Kill();
            _disableTweens.Kill();
        }
    }
}