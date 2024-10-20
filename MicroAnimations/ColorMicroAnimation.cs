using System;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using UnityEngine;
using UnityEngine.UI;

namespace DingoUnityExtensions.MicroAnimations
{
    [Serializable]
    public class ColorMicroAnimation : TweenMicroAnimation
    {
        [SerializeField] private List<Graphic> _graphics;
        [SerializeField] private Color _color = Color.white;
        [SerializeField] private float _eachDelay;

        private readonly List<Color> _defaultValues = new();
        
        public override void ForwardAnimate()
        {
            for (var i = 0; i < _graphics.Count; i++)
            {
                var g = _graphics[i];
                if (_defaultValues.Count <= i)
                    _defaultValues.Add(g.color);
                var i1 = i;
                PlayTween((this, g), d => DOBlendableColor(g, _color - _defaultValues[i1], d), true, i * _eachDelay);
            }
        }

        public override void BackwardAnimate()
        {
            for (var i = 0; i < _graphics.Count; i++)
            {
                var g = _graphics[i];
                if (_defaultValues.Count <= i)
                    _defaultValues.Add(g.color);
                var i1 = i;
                PlayTween((this, g), d => DOBlendableColor(g, _defaultValues[i1] - _color, d), false, (_graphics.Count - i - 1) * _eachDelay);
            }
        }

        private Tween DOBlendableColor(Graphic graphic, Color byValue, float duration)
        {
            var to = Color.clear;
            return DOTween.To(() => to, x =>
            {
                var color = x - to;
                to = x;
                graphic.color += color;
            }, byValue, duration).Blendable().SetTarget(graphic);
        }
        
        protected override void ResetViewValues()
        {
            for (var i = 0; i < _graphics.Count; i++)
            {
                var g = _graphics[i];
                if (i < _defaultValues.Count)
                    g.color = _defaultValues[i];
            }
        }
    }
}