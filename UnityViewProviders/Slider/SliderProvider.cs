using System;
using DG.Tweening;
using DingoUnityExtensions.Tweens;
using DingoUnityExtensions.UnityViewProviders.Core;
using UnityEngine;

namespace DingoUnityExtensions.UnityViewProviders.Slider
{
    [RequireComponent(typeof(UnityEngine.UI.Slider))]
    public class SliderProvider : UnityViewProvider<UnityEngine.UI.Slider, float>, IAnimationContainer<UnityEngine.UI.Slider, float>
    {
        [SerializeField] private bool _convertToSliderLerpValue;
        [SerializeField] private TweenAnimation _changeAnimation;
        
        private Tween _tween;
        private Action<UnityEngine.UI.Slider, float> _beforeStart;
        private Action<UnityEngine.UI.Slider, float, Tween> _tweenModification;

        protected override void OnSetInteractable(bool value)
        {
            View.interactable = value;
            if (View.handleRect != null)
                View.handleRect.gameObject.SetActive(value);
        }

        protected override void SetValueWithoutNotify(float value)
        {
            if (_changeAnimation == null)
            {
                View.SetValueWithoutNotify(value);
                return;
            }

            _tween?.Kill();

            _beforeStart?.Invoke(View, value);
            _tween = _changeAnimation.Do(d => DOTween.To(() => View.value, v => View.SetValueWithoutNotify(v), value, d));
            _tweenModification?.Invoke(View, value, _tween);
            _tween.Play();
        }
        
        public void SetUpdateFunc(Action<UnityEngine.UI.Slider, float> beforeStart, Action<UnityEngine.UI.Slider, float, Tween> tweenModification)
        {
            _tweenModification = tweenModification;
            _beforeStart = beforeStart;
        }

        public float GetFinalValue(float percent) => _convertToSliderLerpValue ? Mathf.Lerp(View.minValue, View.maxValue, percent) : percent;
        public float GetPercentValue(float finalValue) => _convertToSliderLerpValue ? (finalValue - View.minValue) / (View.maxValue - View.minValue) : finalValue;

        protected override void SubscribeOnly() => View.onValueChanged.AddListener(SetValueWithNotify);
        protected override void UnsubscribeOnly() => View.onValueChanged.RemoveListener(SetValueWithNotify);
    }
}