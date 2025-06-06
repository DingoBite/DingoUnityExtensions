﻿using System;
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
        protected EnableDisableTweenAnimationPair Animation => _animation;
        
        protected float PlayTween(object id, TweenUtils.Factory tweenFactory, bool forward, float addDelay = 0)
        {
            id = (id, forward);
            var duration = forward ? _animation.EnableDuration : _animation.DisableDuration;
            var fullDuration = Math.Max(duration + _delay + addDelay, 0);

            var delay = _delay + addDelay;
            var tween = forward ? _animation.MakeEnableTween(tweenFactory) : _animation.MakeDisableTween(tweenFactory);

            if (_delay > Vector2.kEpsilon)
                tween.SetDelay(delay);
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