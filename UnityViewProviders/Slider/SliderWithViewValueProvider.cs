using DingoUnityExtensions.UnityViewProviders.Core;
using UnityEngine;

namespace DingoUnityExtensions.UnityViewProviders.Slider
{
    public class SliderWithViewValueProvider : SliderProvider
    {
        [SerializeField] private ValueContainer<string> _valueText;
        [SerializeField] private string _format;
        [SerializeField] private bool _convertToPercent;

        protected override void SetValueWithoutNotify(float value)
        {
            base.SetValueWithoutNotify(value);
            _valueText.UpdateValueWithoutNotify(ConvertValue(GetFinalValue(value)));
        }

        protected string ConvertValue(float value) => _convertToPercent ? ((int)(value * 100)).ToString(_format) : value.ToString(_format);
    }
}