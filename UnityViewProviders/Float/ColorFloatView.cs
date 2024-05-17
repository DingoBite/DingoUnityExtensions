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
        
        private float _value;
        public override float Value => _value;

        public override void SetValueWithoutNotify(float value)
        {
            _value = value;
            foreach (var graphic in _graphics)
            {
                graphic.color = _gradient.Evaluate(value);
            }
        }
        protected override void SubscribeOnly() { }
        protected override void UnsubscribeOnly() { }
    }
}