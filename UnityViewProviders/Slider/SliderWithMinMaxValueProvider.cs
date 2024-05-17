using DingoUnityExtensions.UnityViewProviders.Core;
using UnityEngine;

namespace DingoUnityExtensions.UnityViewProviders.Slider
{
    public class SliderWithMinMaxValueProvider : SliderWithViewValueProvider
    {
        [SerializeField] private ValueContainer<string> _minText;
        [SerializeField] private ValueContainer<string> _maxText;

        [SerializeField] private float _min;
        [SerializeField] private float _max = 1;

        public void SetMinMax(float min, float max)
        {
            View.minValue = min;
            View.maxValue = max;

            if (_minText != null)
                _minText.SetValueWithoutNotify(ConvertValue(min));
            if (_maxText != null)
                _maxText.SetValueWithoutNotify(ConvertValue(max));
        }

        protected override void Validate() => SetMinMax(_min, _max);
    }
}