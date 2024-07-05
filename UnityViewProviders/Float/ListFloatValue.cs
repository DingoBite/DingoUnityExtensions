using System.Collections.Generic;
using DingoUnityExtensions.UnityViewProviders.Core;
using UnityEngine;

namespace DingoUnityExtensions.UnityViewProviders.Float
{
    public class ListFloatValue : ValueContainer<float>
    {
        [SerializeField] private List<ValueContainer<float>> _valueContainers;

        protected override void SetValueWithoutNotify(float value) => _valueContainers.ForEach(c => c.UpdateValueWithoutNotify(value));

        protected override void OnSetInteractable(bool value)
        {
            base.OnSetInteractable(value);
            _valueContainers.ForEach(c => c.Interactable = value);
        }
    }
}