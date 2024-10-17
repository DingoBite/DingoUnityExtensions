using System.Collections.Generic;
using DingoUnityExtensions.UnityViewProviders.Toggle.Core;
using UnityEngine;

namespace DingoUnityExtensions.UnityViewProviders
{
    public class ActiveToggleSwapInfo : ToggleSwapInfoBase
    {
        [SerializeField] private List<GameObject> _objects;
        
        public override void SetViewActive(bool value)
        {
            foreach (var go in _objects)
            {
                go.SetActive(value);
            }
        }
    }
}