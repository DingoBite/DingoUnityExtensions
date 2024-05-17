using System.Globalization;
using DG.Tweening;
using UnityEngine;
using Utils.Tweens;

namespace DingoUnityExtensions.UnityViewProviders.Text
{
    public class SingleKeyTextSmoothIntChange : SingleKeyText
    {
        [SerializeField] private TweenAnimation _textChangeAnimation;
        [SerializeField] private string _format;
        [SerializeField] private float _multiplier = 1;
        
        private Tween _tween;
        private int _currentInteger;
        private int _targetInteger;

        public void StopAnimation() => _tween?.Kill();
        
        protected override void ReplaceKeyBy(string text)
        {
            var parsed = int.TryParse(text, out var integer);
            if (parsed)
            {
                _targetInteger = (int)(integer * _multiplier);
            }
            else if (float.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out var value))
            {
                parsed = true;
                _targetInteger = (int)(value * _multiplier);
            }

            
            if (_textChangeAnimation == null || _textChangeAnimation.IsInstant || !parsed)
            {
                base.ReplaceKeyBy(!parsed ? text : _targetInteger.ToString(_format));
                return;
            }
            if (_targetInteger == integer && _tween != null && _tween.IsPlaying())
                return;
            StopAnimation();
            _tween = _textChangeAnimation.Do(d => DOTween.To(GetInt, SetInt, integer, d));
            _tween.Play();
        }

        private void SetInt(int i)
        {
            if (i == _currentInteger)
                return;
            _currentInteger = i;
            base.ReplaceKeyBy(_currentInteger.ToString(_format));
        }

        private int GetInt() => _currentInteger;
    }
}