using System.Collections.Generic;
using DingoUnityExtensions.UnityViewProviders.Core.Data;
using DingoUnityExtensions.UnityViewProviders.Toggle.Core;
using UnityEngine;
using uPalette.Runtime.Core;
using uPalette.Runtime.Core.Synchronizer;

namespace DingoUnityExtensions.UnityViewProviders.Toggle.SwapInfo
{
    public class StyleColorChangeToggleSwapInfo : ToggleSwapInfoBase
    {
        [SerializeField] private ColorEntryId _enableId;
        [SerializeField] private ColorEntryId _disableId;

        [SerializeField] private List<UPaletteStyleSelector> _styleSelectors;
        [SerializeField] private int _enableStyle = -1;
        [SerializeField] private int _disableStyle = -1;

        public override void SetViewActive(BoolTimeContext value)
        {
            foreach (var styleSelector in _styleSelectors)
            {
                styleSelector.ChangeStyle(value.Bool() ? _enableStyle : _disableStyle, value.Bool() ? _enableId : _disableId);
            }
        }
    }
}