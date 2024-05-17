using DingoUnityExtensions.UnityViewProviders.Text;
using UnityEngine;

namespace DingoUnityExtensions.UnityViewProviders.Slider
{
    public class SliderWithViewValueProvider : SliderProvider
    {
        [SerializeField] private KeyTextReplacer _valueText;
        [SerializeField] private string _format;
        [SerializeField] private bool _convertToPercent;

        public override void SetValueWithoutNotify(float value)
        {
            base.SetValueWithoutNotify(value);
            _valueText.SetValueWithoutNotify(ConvertValue(GetFinalValue(value)));
        }

        protected string ConvertValue(float value)
        {
            return _convertToPercent ? ((int)(value * 100)).ToString(_format) : value.ToString(_format);
        }
    }
}