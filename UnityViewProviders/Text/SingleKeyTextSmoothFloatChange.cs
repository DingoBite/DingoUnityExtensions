using System;
using System.Globalization;
using DG.Tweening;
using DingoUnityExtensions.Tweens;
using UnityEngine;
using UnityEngine.Events;

namespace DingoUnityExtensions.UnityViewProviders.Text
{
    public class SingleKeyTextSmoothFloatChange : SingleKeyText
    {
        [SerializeField] private TweenAnimation _textChangeAnimation;
        [SerializeField] private float _precision;
        [SerializeField] private string _format;
        [SerializeField] private float _multiplier = 1;
        [SerializeField] private UnityEvent<float> _valueChange;
        
        private Tween _tween;
        private float _currentFloat;
        private float _targetFloat;

        public void StopAnimation() => _tween?.Kill();

        protected override void ReplaceKeyBy(string text)
        {
            var parsed = float.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out var value);
            if (parsed)
                _targetFloat = value ;

            if (_textChangeAnimation == null || _textChangeAnimation.IsInstant || !parsed)
            {
                if (parsed)
                    _valueChange.Invoke(value);
                
                base.ReplaceKeyBy(!parsed ? text : (_targetFloat * _multiplier).ToString(_format));
                return;
            }
            if (Math.Abs(_targetFloat - value) < _precision && _tween != null && _tween.IsPlaying())
                return;
            _valueChange.Invoke(_currentFloat);
            base.ReplaceKeyBy((_currentFloat * _multiplier).ToString(_format));
            StopAnimation();
            _tween = _textChangeAnimation.Do(d => DOTween.To(GetFloat, SetFloat, value, d));
            _tween.OnComplete(() =>
            {
                _currentFloat = value;
                base.ReplaceKeyBy((_currentFloat * _multiplier).ToString(_format));
            });
            _tween.Play();
        }

        private void SetFloat(float v)
        {
            if (Math.Abs(v - _currentFloat) < _precision)
                return;
            _currentFloat = v;
            _valueChange.Invoke(_currentFloat);
            base.ReplaceKeyBy((_currentFloat * _multiplier).ToString(_format));
        }

        private float GetFloat() => _currentFloat;
    }
}