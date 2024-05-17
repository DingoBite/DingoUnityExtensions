using System;
using System.Globalization;
using DG.Tweening;
using DingoUnityExtensions.Tweens;
using UnityEngine;

namespace DingoUnityExtensions.UnityViewProviders.Text
{
    public class SingleKeyTextSmoothFloatChange : SingleKeyText
    {
        [SerializeField] private TweenAnimation _textChangeAnimation;
        [SerializeField] private float _precision;
        [SerializeField] private string _format;
        [SerializeField] private float _multiplier = 1;

        private Tween _tween;
        private float _currentFloat;
        private float _targetFloat;

        public void StopAnimation() => _tween?.Kill();

        protected override void ReplaceKeyBy(string text)
        {
            var parsed = float.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out var value);
            if (parsed)
                _targetFloat = value * _multiplier;

            if (_textChangeAnimation == null || _textChangeAnimation.IsInstant || !parsed)
            {
                base.ReplaceKeyBy(!parsed ? text : _targetFloat.ToString(_format));
                return;
            }
            if (Math.Abs(_targetFloat - value) < _precision && _tween != null && _tween.IsPlaying())
                return;
            StopAnimation();
            _tween = _textChangeAnimation.Do(d => DOTween.To(GetFloat, SetFloat, value, d));
            _tween.OnComplete(() =>
            {
                _currentFloat = value;
                base.ReplaceKeyBy(_currentFloat.ToString(_format));
            });
            _tween.Play();
        }

        private void SetFloat(float v)
        {
            if (Math.Abs(v - _currentFloat) < _precision)
                return;
            _currentFloat = v;
            base.ReplaceKeyBy(_currentFloat.ToString(_format));
        }

        private float GetFloat() => _currentFloat;
    }
}