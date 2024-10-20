using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using DingoUnityExtensions.Tweens;
using UnityEngine;

namespace DingoUnityExtensions.MicroAnimations
{
    [Serializable]
    public abstract class TweenMicroAnimation
    {
        [SerializeField] private EnableDisableTweenAnimationPair _animation;
        [SerializeField] private float _delay;

        private readonly Dictionary<object, Tween> _tweens = new();

        private int _lastForwardFrameAnimated;
        private int _lastBackwardFrameAnimated;
        
        protected float PlayTween(object id, Func<float, Tween> tweenFactory, bool forward, float addDelay = 0)
        {
            id = (id, forward);
            var duration = forward ? _animation.EnableDuration : _animation.DisableDuration;
            var fullDuration = Math.Max(duration + _delay + addDelay, 0);
            var newFrame = Time.frameCount;
            var forwardFrame = newFrame - _lastForwardFrameAnimated < 1;
            var backwardFrame = newFrame - _lastBackwardFrameAnimated < 1;
            if (forward && forwardFrame || !forward && backwardFrame)
                return fullDuration;

            Tween tween;
            if (forward)
            {
                _lastForwardFrameAnimated = newFrame;
                tween = _animation.MakeEnableTween(tweenFactory).SetDelay(_delay + addDelay);
            }
            else
            {
                _lastBackwardFrameAnimated = newFrame;
                tween = _animation.MakeDisableTween(tweenFactory).SetDelay(_delay + addDelay);
            }
            tween.Play();
            _tweens[id] = tween;
            return fullDuration;
        }

        public abstract void ForwardAnimate();
        public abstract void BackwardAnimate();

        public void ResetView()
        {
            foreach (var tween in _tweens.Values.Where(t => t != null))
            {
                tween.Goto(0);
                tween.Kill();
            }
            _tweens.Clear();
            ResetViewValues();
        }

        protected abstract void ResetViewValues();
    }
}