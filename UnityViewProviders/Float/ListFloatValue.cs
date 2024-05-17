using System.Collections.Generic;
using DingoUnityExtensions.UnityViewProviders.Core;
using UnityEngine;

namespace DingoUnityExtensions.UnityViewProviders.Float
{
    public class ListFloatValue : ValueContainer<float>
    {
        [SerializeField] private List<ValueContainer<float>> _valueContainers;
        
        private float _value;
        public override float Value => _value;

        protected override void SubscribeOnly() { }
        protected override void UnsubscribeOnly() { }

        public override void SetValueWithoutNotify(float value)
        {
            _valueContainers.ForEach(c => c.SetValueWithoutNotify(value));
        }

        protected override void OnSetInteractable(bool value)
        {
            base.OnSetInteractable(value);
            _valueContainers.ForEach(c => c.Interactable = value);
        }
    }
}