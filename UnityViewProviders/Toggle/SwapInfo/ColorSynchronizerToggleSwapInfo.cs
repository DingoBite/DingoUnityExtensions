using System.Collections.Generic;
using DingoUnityExtensions.UnityViewProviders.Core.Data;
using DingoUnityExtensions.UnityViewProviders.Toggle.Core;
using UnityEngine;
using uPalette.Runtime.Core;
using uPalette.Runtime.Core.Synchronizer.Color;

namespace DingoUnityExtensions.UnityViewProviders.Toggle.SwapInfo
{
    public class ColorSynchronizerToggleSwapInfo : ToggleSwapInfoBase
    {
        [SerializeField] private ColorEntryId _enableId;
        [SerializeField] private ColorEntryId _disableId;

        [SerializeField] private List<ColorSynchronizer> _colorSynchronizers;
        
        public override void SetViewActive(BoolTimeContext value)
        {
            foreach (var colorSynchronizer in _colorSynchronizers)
            {
                colorSynchronizer.SetEntryId(value.Bool() ? _enableId.Value : _disableId.Value);
            }
        }
    }
}