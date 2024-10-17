using System.Collections.Generic;
using System.Linq;
using DingoUnityExtensions.UnityViewProviders.Toggle.Core;
using UnityEngine;
using UnityEngine.UI;

namespace DingoUnityExtensions.UnityViewProviders.Toggle
{
    public class ColorToggleSwapInfo : ToggleSwapInfoBase
    {
        [SerializeField] private List<Graphic> _graphics;
        [SerializeField] private Color _inactiveColor = Color.white;
        [SerializeField] private Color _activeColor = Color.white;
        
        public override void SetViewActive(bool value)
        {
            var c = value ? _activeColor : _inactiveColor;
            foreach (var graphic in _graphics.Where(g => g != null))
            {
                graphic.color = c;
            }
        }
    }
}