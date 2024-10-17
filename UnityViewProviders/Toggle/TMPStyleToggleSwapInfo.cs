using DingoUnityExtensions.UnityViewProviders.Toggle.Core;
using TMPro;
using UnityEngine;

namespace DingoUnityExtensions.UnityViewProviders.Toggle
{
    public class TMPStyleToggleSwapInfo : ToggleSwapInfoBase
    {
        [SerializeField] private TMP_Text _text;
        [SerializeField] private FontStyles _enableFontStyle;
        [SerializeField] private FontStyles _disableFontStyle;

        public override void SetViewActive(bool value)
        {
            if (value)
                _text.fontStyle = _enableFontStyle;
            else
                _text.fontStyle = _disableFontStyle;
        }
    }
}