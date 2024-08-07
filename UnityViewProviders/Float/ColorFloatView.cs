using System.Collections.Generic;
using DingoUnityExtensions.UnityViewProviders.Core;
using UnityEngine;
using UnityEngine.UI;

namespace DingoUnityExtensions.UnityViewProviders.Float
{
    public class ColorFloatView : ValueContainer<float>
    {
        [SerializeField] private Gradient _gradient;
        [SerializeField] private List<Graphic> _graphics;

        protected override void SetValueWithoutNotify(float value)
        {
            foreach (var graphic in _graphics)
            {
                graphic.color = _gradient.Evaluate(value);
            }
        }
    }
}