using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using DingoUnityExtensions.Tweens;
using UnityEngine;

namespace DingoUnityExtensions.MicroAnimations
{
    [Serializable]
    public abstract class MicroAnimation
    {
        public abstract void ForwardAnimate();
        public abstract void BackwardAnimate();
        public abstract void ResetView();
    }
    
    [Serializable]
    public abstract class TweenMicroAnimation : MicroAnimation
    {
        [SerializeField] private EnableDisableTweenAnimationPair _animation;
        [SerializeField] private float _delay;

        private readonly Dictionary<object, Tween> _tweens = new();

        private readonly Dictionary<object, int> _lastForwardFrameAnimatedDict = new();
        private readonly Dictionary<object, int> _lastBackwardFrameAnimatedDict = new();

        protected EnableDisableTweenAnimationPair Animation => _animation;
        
        protected float PlayTween(object id, TweenUtils.Factory tweenFactory, bool forward, float addDelay = 0, bool useCache = false)
        {
            id = (id, forward);
            var duration = forward ? _animation.EnableDuration : _animation.DisableDuration;
            var fullDuration = Math.Max(duration + _delay + addDelay, 0);
            var newFrame = Time.frameCount;
            var lastForwardFrameAnimated = _lastForwardFrameAnimatedDict.GetValueOrDefault(id, 0);
            var lastBackwardFrameAnimated = _lastBackwardFrameAnimatedDict.GetValueOrDefault(id, 0);
            var forwardFrame = newFrame - lastForwardFrameAnimated < 1;
            var backwardFrame = newFrame - lastBackwardFrameAnimated < 1;
            if (forward && forwardFrame || !forward && backwardFrame)
                return fullDuration;

            Tween tween;
            if (forward)
            {
                _lastForwardFrameAnimatedDict[id] = newFrame;
                tween = _animation.MakeEnableTween(tweenFactory).SetDelay(_delay + addDelay);
            }
            else
            {
                _lastBackwardFrameAnimatedDict[id] = newFrame;
                tween = _animation.MakeDisableTween(tweenFactory).SetDelay(_delay + addDelay);
            }
            tween.Play();
            _tweens[id] = tween;
            return fullDuration;
        }

        public override void ResetView()
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