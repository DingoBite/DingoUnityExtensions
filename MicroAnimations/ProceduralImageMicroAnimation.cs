#if PROCEDURAL_IMAGE_EXISTS
using System;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using UnityEngine;
using UnityEngine.UI.ProceduralImage;

namespace DingoUnityExtensions.MicroAnimations
{
    [Serializable]
    public class ProceduralImageParameters
    {
        public float BorderWidth;
        public ModifierID ModifierID;
    }

    [Serializable]
    public class ProceduralImageMicroAnimation : TweenMicroAnimation
    {
        [SerializeField] private List<ProceduralImage> _graphics;
        [SerializeField] private ProceduralImageParameters _parameters;
        [SerializeField] private float _eachDelay;
        
        public override void ForwardAnimate()
        {
            for (var i = 0; i < _graphics.Count; i++)
            {
                var g = _graphics[i];
                PlayTween((this, g), d => DOBlendableProceduralImageParameters(g, _parameters.BorderWidth, d), true, i * _eachDelay);
            }
        }

        public override void BackwardAnimate()
        {
            for (var i = 0; i < _graphics.Count; i++)
            {
                var g = _graphics[i];
                PlayTween((this, g), d => DOBlendableProceduralImageParameters(g, -_parameters.BorderWidth, d), false, (_graphics.Count - i - 1) * _eachDelay);
            }
        }

        private Tween DOBlendableProceduralImageParameters(ProceduralImage graphic, float byValue, float duration)
        {
            var to = 0f;
            return DOTween.To(() => to, x =>
            {
                var v = x - to;
                to = x;
                graphic.BorderWidth += v;
            }, byValue, duration).Blendable().SetTarget(graphic);
        }

        protected override void ResetViewValues() { }
    }
}
#endif
