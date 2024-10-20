using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace DingoUnityExtensions.MicroAnimations
{
    [Serializable]
    public class PositionMicroAnimation : TweenMicroAnimation
    {
        [SerializeField] private List<Transform> _graphics;
        [SerializeField] private Vector3 _positionDelta = Vector3.zero;
        [SerializeField] private float _eachDelay;

        private readonly List<Vector3> _defaultValues = new();
        
        public override void ForwardAnimate()
        {
            for (var i = 0; i < _graphics.Count; i++)
            {
                var g = _graphics[i];
                if (_defaultValues.Count <= i)
                    _defaultValues.Add(g.localPosition);
                
                PlayTween((this, g), d => g.DOBlendableLocalMoveBy(_positionDelta, d), true, i * _eachDelay);
            }
        }

        public override void BackwardAnimate()
        {
            for (var i = 0; i < _graphics.Count; i++)
            {
                var g = _graphics[i];
                if (_defaultValues.Count <= i)
                    _defaultValues.Add(g.localPosition);
                PlayTween((this, g), d => g.DOBlendableLocalMoveBy(-_positionDelta, d), false, (_graphics.Count - i - 1) * _eachDelay);
            }
        }

        protected override void ResetViewValues()
        {
            for (var i = 0; i < _graphics.Count; i++)
            {
                var g = _graphics[i];
                if (i >= _defaultValues.Count)
                    return;
                g.localPosition = _defaultValues[i];
            }
        }
    }
}