using DingoUnityExtensions.UnityViewProviders.Core;
using UnityEngine;

namespace DingoUnityExtensions.UnityViewProviders.Slider
{
    [RequireComponent(typeof(UnityEngine.UI.Slider))]
    public class SliderProvider : UnityViewProvider<UnityEngine.UI.Slider, float>
    {
        [SerializeField] private bool _convertToSliderLerpValue;

        private float _value;
        public override float Value => _value;

        protected override void OnSetInteractable(bool value)
        {
            View.interactable = value;
            if (View.handleRect != null)
                View.handleRect.gameObject.SetActive(value);
        }

        public override void SetValueWithoutNotify(float value)
        {
            _value = value;
            View.SetValueWithoutNotify(_value);
        }
        
        public float GetFinalValue(float percent) => _convertToSliderLerpValue ? Mathf.Lerp(View.minValue, View.maxValue, percent) : percent;
        public float GetPercentValue(float finalValue) => _convertToSliderLerpValue ? (finalValue - View.minValue) / (View.maxValue - View.minValue) : finalValue;
        
        protected override void SubscribeOnly() => View.onValueChanged.AddListener(SetValueWithNotify);
        protected override void UnsubscribeOnly() => View.onValueChanged.RemoveListener(SetValueWithNotify);
    }
}