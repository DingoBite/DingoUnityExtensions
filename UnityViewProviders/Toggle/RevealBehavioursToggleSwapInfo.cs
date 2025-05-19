using System.Collections.Generic;
using DingoUnityExtensions.Tweens;
using DingoUnityExtensions.UnityViewProviders.Core.Data;
using DingoUnityExtensions.UnityViewProviders.Toggle.Core;
using UnityEngine;

namespace DingoUnityExtensions.UnityViewProviders.Toggle
{
    public class RevealBehavioursToggleSwapInfo : ToggleSwapInfoBase
    {
        [SerializeField] private List<RevealBehaviour> _objects;
        [SerializeField] private List<RevealBehaviour> _reverseObjects;
        [SerializeField] private bool _invert;
        
        public override void SetViewActive(BoolTimeContext value)
        {
            var bValue = value.Bool();
            bValue = _invert ? !bValue : bValue;
            foreach (var revealBehaviour in _objects)
            {
                revealBehaviour.SetActive(value.Bool(), value.Immediately());
            }

            foreach (var revealBehaviour in _reverseObjects)
            {
                revealBehaviour.SetActive(!value.Bool(), value.Immediately());
            }
        }
    }
}