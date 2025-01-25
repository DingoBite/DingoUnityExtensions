using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace DingoUnityExtensions.Tweens
{
    public abstract class AnimatableBehaviour : RevealBehaviour
    {
        [SerializeField] private List<AnimatableBehaviour> _stack;
        [SerializeField] private GameObject _gameObject;

        [field: SerializeField] protected bool RestartPlayingTween { get; private set; }
        [SerializeField] private bool _manageActiveness = true;
        [SerializeField] private bool _firstValueReset = true;
        
        protected abstract bool ValidAnimationParams { get; }
        protected IReadOnlyList<AnimatableBehaviour> Stack => _stack;

        protected GameObject GameObject
        {
            get
            {
                if (_gameObject == null)
                    _gameObject = gameObject;
                return _gameObject;
            }
        }
        
        public bool IsFullyDisabled => State == AnimateState.Disabled;
        public bool IsFullyEnabled => State == AnimateState.Enabled;

        private readonly TweenList _enableTweens = new();
        private readonly TweenList _disableTweens = new();

        public override void SetActive(bool value, bool isImmediately)
        {
            if (isImmediately)
                SetActiveImmediately(value);
            else 
                AnimatableSetActive(value);
        }
        
        public override void AnimatableSetActive(bool value)
        {
            if (value)
                EnableNoParams();
            else 
                DisableNoParams();
        }
        
        public override void SetActiveImmediately(bool value)
        {
            if (value)
                EnableImmediately();
            else 
                DisableImmediately();
        }
        
        public override void EnableNoParams()
        {
            if (!Application.isPlaying && Application.isEditor)
                EnableImmediately();
            else 
                Enable();
        }

        public override void DisableNoParams()
        {
            if (!Application.isPlaying && Application.isEditor)
                DisableImmediately();
            else 
                Disable();
        }

        public override float Enable(float addDelay = 0, Action onComplete = null)
        {
            if (!ValidAnimationParams || State == AnimateState.Enabled)
            {
                EnableImmediately();
                onComplete?.Invoke();
                return 0;
            }

            foreach (var animatableBehaviour in _stack)
            {
                animatableBehaviour.Enable(addDelay);
            }

            var prevState = State;
            if (prevState == AnimateState.None && _firstValueReset)
            {
                SetFullActive(AnimateState.Disabled, true);
                State = AnimateState.Disabled;
            }
            
            SetFullActive(AnimateState.Enabling);
            var isPlaying = IsPlaying(_enableTweens, out var remainingTime, out var elapsedTime);
            if (!RestartPlayingTween && (isPlaying || prevState is AnimateState.Enabling or AnimateState.Enabled))
                return remainingTime;
            
            Kill();
            State = AnimateState.Enabling;
            foreach (var enableTween in CollectEnableTweens(isPlaying, addDelay))
            {
                _enableTweens.AddWithDuration(enableTween);
            }
            _enableTweens.Play(() =>
            {
                EnableImmediately();
                onComplete?.Invoke();
            });
            return _enableTweens.MaxDuration;
        }

        public override float Disable(float addDelay = 0, Action onComplete = null)
        {
            if (!ValidAnimationParams || State == AnimateState.Disabled)
            {
                DisableImmediately();
                onComplete?.Invoke();
                return 0;
            }
         
            foreach (var animatableBehaviour in _stack)
            {
                animatableBehaviour.Disable(addDelay);
            }
            
            var prevState = State;
            if (prevState == AnimateState.None && _firstValueReset)
            {
                SetFullActive(AnimateState.Disabled, true);
                State = AnimateState.Disabled;
                onComplete?.Invoke();
                return 0f;
            }
            SetFullActive(AnimateState.Disabling);
            var isPlaying = IsPlaying(_disableTweens, out var remainingTime, out var elapsedTime);
            if (!RestartPlayingTween && (isPlaying || prevState is AnimateState.Disabling or AnimateState.Disabled))
                return remainingTime;
            
            Kill();
            State = AnimateState.Disabling;
            foreach (var disableTween in CollectDisableTweens(isPlaying, addDelay))
            {
                _disableTweens.AddWithDuration(disableTween);
            }
            _disableTweens.Play(() =>
            {
                DisableImmediately();
                onComplete?.Invoke();
            });
            return _disableTweens.MaxDuration;
        }

        private void EnableImmediatelyPlayHandle()
        {
            SetFullActive(AnimateState.Enabling);
            var isPlaying = IsPlaying(_enableTweens, out _, out _);
            if (!RestartPlayingTween && (isPlaying || State is AnimateState.Enabling or AnimateState.Enabled))
                return;
            EnableImmediately();
        }
        
        private void DisableImmediatelyPlayHandle()
        {
            SetFullActive(AnimateState.Disabling);
            var isPlaying = IsPlaying(_disableTweens, out _, out _);
            if (!RestartPlayingTween && (isPlaying || State is AnimateState.Disabling or AnimateState.Disabled))
                return;
            DisableImmediately();
        }
        
        public override void EnableImmediately()
        {
            foreach (var animatableBehaviour in _stack)
            {
                animatableBehaviour.EnableImmediatelyPlayHandle();
            }
            
            State = AnimateState.Enabled;
            SetFullActive(State, true);
            if (!ValidAnimationParams)
                return;
            
            Kill();
        }

        public override void DisableImmediately()
        {
            foreach (var animatableBehaviour in _stack)
            {
                animatableBehaviour.DisableImmediatelyPlayHandle();
            }
            
            State = AnimateState.Disabled;
            SetFullActive(State, true);
            if (!ValidAnimationParams)
                return;

            Kill();
        }
        
        protected abstract IEnumerable<Tween> CollectEnableTweens(bool isPlaying, float addDelay);
        protected abstract IEnumerable<Tween> CollectDisableTweens(bool isPlaying, float addDelay);
        
        protected virtual void SetFullActive(AnimateState state, bool force = false)
        {
            var active = state is AnimateState.Enabled or AnimateState.Enabling or AnimateState.Disabling;
            if (enabled == active && enabled == GameObject.activeSelf)
                return;
            enabled = active;
            if (_manageActiveness && enabled != GameObject.activeSelf)
                GameObject.SetActive(enabled);
        }

        private static bool IsPlaying(TweenList tweenList, out float remainingTime, out float elapsedTime)
        {
            remainingTime = 0;
            if (tweenList.MaxTween != null && tweenList.IsPlaying())
            {
                var duration = tweenList.MaxTween.Duration();
                var elapsedPercentage = tweenList.MaxTween.ElapsedPercentage();
                elapsedTime = elapsedPercentage * duration;
                remainingTime = (1 - elapsedPercentage) * duration;
                return true;
            }

            elapsedTime = 1;
            return false;
        }
        
        private void Kill()
        {
            _enableTweens.Kill();
            _disableTweens.Kill();
        }

        private void Reset()
        {
            _gameObject = gameObject;
        }

        private void OnDestroy()
        {
            Kill();
        }
    }
}