using System.Collections.Generic;
using DingoUnityExtensions.UnityViewProviders.Core.Data;
using DingoUnityExtensions.UnityViewProviders.Toggle.Core;
using UnityEngine;

namespace DingoUnityExtensions.UnityViewProviders.Toggle
{
    public class ActiveToggleSwapInfo : ToggleSwapInfoBase
    {
        [SerializeField] private List<GameObject> _objects;
        [SerializeField] private List<GameObject> _reverseObjects;
        [SerializeField] private bool _invert;
        
        public override void SetViewActive(BoolTimeContext value)
        {
            var bValue = value.Bool();
            bValue = _invert ? !bValue : bValue;
            foreach (var go in _objects)
            {
                go.SetActive(bValue);
            }

            foreach (var go in _reverseObjects)
            {
                go.SetActive(!bValue);
            }
        }
    }
}