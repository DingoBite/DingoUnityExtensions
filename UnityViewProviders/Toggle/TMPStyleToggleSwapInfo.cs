using DingoUnityExtensions.UnityViewProviders.Toggle.Core;
using TMPro;
using UnityEngine;

namespace DingoUnityExtensions.UnityViewProviders.Toggle
{
    public class TMPStyleToggleSwapInfo : ToggleSwapInfoBase
    {
        [SerializeField] private TMP_Text _text;
        [SerializeField] private FontStyles _enableFontStyle;
        [SerializeField] private Color _enableFontColor = Color.white;
        [SerializeField] private FontStyles _disableFontStyle;
        [SerializeField] private Color _disableFontColor = Color.white;

        public override void SetViewActive(bool value)
        {
            if (value)
            {
                _text.fontStyle = _enableFontStyle;
                _text.color = _enableFontColor;
            }
            else
            {
                _text.fontStyle = _disableFontStyle;
                _text.color = _disableFontColor;
            }
        }
    }
}