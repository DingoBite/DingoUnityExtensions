using System;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DingoUnityExtensions.Tweens;
using UnityEngine;

namespace DingoUnityExtensions.MicroAnimations
{
    [Serializable]
    public class ScaleMicroAnimation : TweenMicroAnimation
    {
        [SerializeField] private List<Transform> _graphics;
        [SerializeField] private float _scaleDelta = 0.05f;
        [SerializeField] private float _eachDelay;

        private readonly List<Vector3> _defaultValues = new();

        public override void ForwardAnimate()
        {
            for (var i = 0; i < _graphics.Count; i++)
            {
                var g = _graphics[i];
                if (g == null)
                    continue;
                if (_defaultValues.Count <= i)
                    _defaultValues.Add(g.localScale);
                PlayTween((this, g), d => DOBlendableScaleBy(g, _scaleDelta * Vector3.one, d), true, i * _eachDelay);
            }
        }

        public override void BackwardAnimate()
        {
            for (var i = 0; i < _graphics.Count; i++)
            {
                var g = _graphics[i];
                if (g == null)
                    continue;
                if (_defaultValues.Count <= i)
                    _defaultValues.Add(g.localScale);
                PlayTween((this, g), d => DOBlendableScaleBy(g, -_scaleDelta * Vector3.one, d), false, (_graphics.Count - i - 1) * _eachDelay);
            }
        }
        
        public static Tweener DOBlendableScaleBy(
            Transform transform,
            Vector3 byValue,
            float duration)
        {
            var to = Vector3.zero;
            return DOTween.To(() => to, x =>
            {
                var vector3 = x - to;
                to = x;
                transform.localScale += vector3;
            }, byValue, duration).Blendable().SetTarget(transform);
        }
        
        protected override void ResetViewValues()
        {
            for (var i = 0; i < _graphics.Count; i++)
            {
                var g = _graphics[i];
                if (i >= _defaultValues.Count)
                    return;
                g.localScale = _defaultValues[i];
            }
        }
    }
}