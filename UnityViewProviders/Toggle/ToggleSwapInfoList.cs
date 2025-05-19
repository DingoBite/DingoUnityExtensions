using System.Collections.Generic;
using DingoUnityExtensions.UnityViewProviders.Core.Data;
using DingoUnityExtensions.UnityViewProviders.Toggle.Core;
using UnityEngine;

namespace DingoUnityExtensions.UnityViewProviders.Toggle
{
    public class ToggleSwapInfoList : ToggleSwapInfoBase
    {
        [SerializeField] private List<ToggleSwapInfoBase> _toggleSwapInfos;

        public override void SetViewActive(BoolTimeContext value)
        {
            foreach (var toggleSwapInfo in _toggleSwapInfos)
            {
                toggleSwapInfo.SetViewActive(value);
            }
        }
    }
}