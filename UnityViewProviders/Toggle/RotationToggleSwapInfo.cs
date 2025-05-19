using System.Collections.Generic;
using DingoUnityExtensions.UnityViewProviders.Core.Data;
using DingoUnityExtensions.UnityViewProviders.Toggle.Core;
using UnityEngine;

namespace DingoUnityExtensions.UnityViewProviders.Toggle
{
    public class RotationToggleSwapInfo : ToggleSwapInfoBase
    {
        [SerializeField] private List<Transform> _transforms;

        [SerializeField] private Vector3 _enableRotation;
        [SerializeField] private Vector3 _disableRotation;

        public override void SetViewActive(BoolTimeContext value)
        {
            var rot = value.Bool() ? Quaternion.Euler(_enableRotation) : Quaternion.Euler(_disableRotation);
            foreach (var tr in _transforms)
            {
                tr.localRotation = rot;
            }
        }
    }
}