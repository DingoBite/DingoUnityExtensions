using System.Collections.Generic;
using DingoUnityExtensions.UnityViewProviders.Core.Data;
using DingoUnityExtensions.UnityViewProviders.Toggle.Core;
using UnityEngine;
using uPalette.Runtime.Core.Synchronizer;

namespace DingoUnityExtensions.UnityViewProviders.Toggle.SwapInfo
{
    public class StyleSynchronizerToggleSwapInfo : ToggleSwapInfoBase
    {
        [SerializeField] private List<UPaletteStyleSelector> _styleSelectors;
        [SerializeField] private int _styleIndex = -1;
        [SerializeField] private bool _invert;

        public override void SetViewActive(BoolTimeContext value)
        {
            foreach (var styleSelector in _styleSelectors)
            {
                var active = value.Bool();
                if (_invert)
                    active = !active;
                if (active)
                    styleSelector.EnableStyle(_styleIndex);
                else
                    styleSelector.DisableStyle(_styleIndex);
            }
        }
    }
}