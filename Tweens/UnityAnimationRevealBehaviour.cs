using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DingoUnityExtensions.Tweens
{
    public class UnityAnimationRevealBehaviour : RevealBehaviour
    {
        private static readonly List<string> None = new List<string> { "-" };

        [SerializeField] private GameObject _gameObject;
        [SerializeField] private Animator _animator;
        [SerializeField] private AnimationEventDispatcher _animationEventDispatcher;
        [SerializeField] private bool _manageActiveness = true;
        [SerializeField] private List<Graphic> _setDirtyGraphics;
        
        [SerializeField] private string _enableAnimation;
        [SerializeField] private string _disableAnimation;
        
        private bool _initialized;
        private Action _onEnableComplete;
        private Action _onDisableComplete;

        protected GameObject GameObject
        {
            get
            {
                if (_gameObject == null)
                    _gameObject = gameObject;
                return _gameObject;
            }
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

        public override float Enable(float addDelay = 0, Action onComplete = null)
        {
            _onEnableComplete = onComplete;
            Initialize();
            var prevState = State;
            if (prevState is AnimateState.Enabling)
                return 0f;
            if (prevState is AnimateState.Enabled)
            {
                _onEnableComplete?.Invoke();
                _onEnableComplete = null;
                return 0f;
            }
            GameObject.SetActive(true);
            enabled = true;
            _animator.enabled = true;
            if (prevState is AnimateState.None or AnimateState.Disabled)
            {
                _animator.Rebind();
                _animator.Update(0);
            }

            State = AnimateState.Enabling;
            _animator.Play(_enableAnimation);
            _animationEventDispatcher.OnAnimationComplete -= SetFullEnable;
            _animationEventDispatcher.OnAnimationComplete += SetFullEnable;
            return 0;
        }

        public override float Disable(float addDelay = 0, Action onComplete = null)
        {
            _onDisableComplete = onComplete;
            Initialize();
            var prevState = State;
            if (prevState is AnimateState.Disabling)
                return 0f;
            if (prevState is AnimateState.None or AnimateState.Disabled)
            {
                _onDisableComplete?.Invoke();
                _onDisableComplete = null;
                return 0f;
            }

            State = AnimateState.Disabling;
            _animator.Play(_disableAnimation);
            _animationEventDispatcher.OnAnimationComplete -= SetFullDisable;
            _animationEventDispatcher.OnAnimationComplete += SetFullDisable;
            return 0;
        }

        private void LateUpdate()
        {
            if (_setDirtyGraphics.Count == 0 || State is AnimateState.Enabled or AnimateState.Disabled)
                return;
            foreach (var graphic in _setDirtyGraphics)
            {
                graphic.SetAllDirty();
            }
        }

        public override void DisableNoParams()
        {
            if (!Application.isPlaying && Application.isEditor)
                DisableImmediately();
            else 
                Disable();
        }

        public override void EnableImmediately()
        {
            State = AnimateState.Enabled;
            SetFullActive(State);
        }

        public override void DisableImmediately()
        {
            _animator.enabled = false;
            State = AnimateState.Disabled;
            SetFullActive(State);
        }

        private void SetFullEnable(string s)
        {
            _onEnableComplete?.Invoke();
            _onEnableComplete = null;
            SetFullActive(AnimateState.Enabled);
        }

        private void SetFullDisable(string s)
        {
            _onDisableComplete?.Invoke();
            _onDisableComplete = null;
            SetFullActive(AnimateState.Disabled);
        }

        protected virtual void SetFullActive(AnimateState state)
        {
            _animationEventDispatcher.OnAnimationComplete -= SetFullEnable;
            _animationEventDispatcher.OnAnimationComplete -= SetFullDisable;

            enabled = state is AnimateState.Enabled or AnimateState.Enabling or AnimateState.Disabling;
            if (_manageActiveness)
                GameObject.SetActive(enabled);
            State = state;
            if (_setDirtyGraphics.Count == 0)
                return;
            foreach (var graphic in _setDirtyGraphics)
            {
                graphic.SetAllDirty();
            }
        }

        private void Initialize()
        {
            if (_initialized)
                return;
            _animationEventDispatcher.Initialize(_animator);
            _initialized = true;
        }
    }
}