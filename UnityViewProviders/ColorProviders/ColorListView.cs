using System.Collections.Generic;
using DingoUnityExtensions.UnityViewProviders.Core;
using UnityEngine;
using UnityEngine.UI;

namespace DingoUnityExtensions.UnityViewProviders.ColorProviders
{
    public class ColorListView : ValueContainer<Color>
    {
        [SerializeField] private List<Graphic> _graphics;
        [SerializeField] private bool _changeAlpha;

        protected override void SetValueWithoutNotify(Color value)
        {
            foreach (var graphic in _graphics)
            {
                if (!_changeAlpha)
                    graphic.color = value;
                else
                    graphic.color = new Color(value.r, value.g, value.b, graphic.color.a);
            }
        }
    }
}