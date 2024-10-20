using System.Collections.Generic;
using DingoUnityExtensions.UnityViewProviders.Toggle.Core;
using UnityEngine;

namespace DingoUnityExtensions.UnityViewProviders.Toggle
{
    public class ActiveToggleSwapInfo : ToggleSwapInfoBase
    {
        [SerializeField] private List<GameObject> _objects;
        [SerializeField] private bool _invert;
        
        public override void SetViewActive(bool value)
        {
            value = _invert ? !value : value;
            foreach (var go in _objects)
            {
                go.SetActive(value);
            }
        }
    }
}