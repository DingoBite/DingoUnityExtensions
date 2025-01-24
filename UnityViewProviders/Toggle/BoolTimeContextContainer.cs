using System.Collections.Generic;
using DingoUnityExtensions.Tweens;
using DingoUnityExtensions.UnityViewProviders.Core;
using DingoUnityExtensions.UnityViewProviders.Core.Data;
using UnityEngine;

namespace DingoUnityExtensions.UnityViewProviders.Toggle
{
    public class BoolTimeContextContainer : ValueContainer<BoolTimeContext>
    {
        [SerializeField] private List<RevealBehaviour> _revealBehaviours;
        [SerializeField] private List<RevealBehaviour> _invertRevealBehaviours;
        
        [SerializeField] private ValueContainer<bool> _toggle;
        [SerializeField] private ValueContainer<bool> _invertToggle;

        protected override void SetValueWithoutNotify(BoolTimeContext value)
        {
            var boolValue = value.Bool();
            var immediately = value.Immediately();
            if (_toggle != null)
                _toggle.UpdateValueWithoutNotify(boolValue);
            if (_invertToggle != null)
                _invertToggle.UpdateValueWithoutNotify(!boolValue);
            foreach (var revealBehaviour in _revealBehaviours)
            {
                revealBehaviour.SetActive(boolValue, immediately);
            }

            foreach (var invertRevealBehaviour in _invertRevealBehaviours)
            {
                invertRevealBehaviour.SetActive(!boolValue, immediately);
            }
        }
    }
}