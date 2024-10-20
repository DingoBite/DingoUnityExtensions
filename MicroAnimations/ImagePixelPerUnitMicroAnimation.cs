using System;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using UnityEngine;
using UnityEngine.UI;

namespace DingoUnityExtensions.MicroAnimations
{
    [Serializable]
    public class ImagePixelPerUnitMicroAnimation : TweenMicroAnimation
    {
        [SerializeField] private List<Image> _graphics;
        [SerializeField] private float _pixelsPerUnitMultiplierDelta;
        [SerializeField] private float _eachDelay;

        private readonly List<float> _defaultValues = new();
        
        public override void ForwardAnimate()
        {
            for (var i = 0; i < _graphics.Count; i++)
            {
                var g = _graphics[i];
                if (_defaultValues.Count <= i)
                    _defaultValues.Add(g.pixelsPerUnitMultiplier);
                PlayTween((this, g), d => DOBlendablePixelPerUnit(g, _pixelsPerUnitMultiplierDelta, d), true, i * _eachDelay);
            }
        }

        public override void BackwardAnimate()
        {
            for (var i = 0; i < _graphics.Count; i++)
            {
                var g = _graphics[i];
                if (_defaultValues.Count <= i)
                    _defaultValues.Add(g.pixelsPerUnitMultiplier);
                var i1 = i;
                PlayTween((this, g), d => DOBlendablePixelPerUnit(g, -_pixelsPerUnitMultiplierDelta, d), false, (_graphics.Count - i - 1) * _eachDelay);
            }
        }

        private Tween DOBlendablePixelPerUnit(Image graphic, float byValue, float duration)
        {
            var to = 0f;
            return DOTween.To(() => to, x =>
            {
                var v = x - to;
                to = x;
                graphic.pixelsPerUnitMultiplier += v;
            }, byValue, duration).Blendable().SetTarget(graphic);
        }
        
        protected override void ResetViewValues()
        {
            for (var i = 0; i < _graphics.Count; i++)
            {
                var g = _graphics[i];
                if (i < _defaultValues.Count)
                    g.pixelsPerUnitMultiplier = _defaultValues[i];
            }
        }
    }
}