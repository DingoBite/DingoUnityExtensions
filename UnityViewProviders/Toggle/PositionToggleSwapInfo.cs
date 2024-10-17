using DingoUnityExtensions.UnityViewProviders.Toggle.Core;
using UnityEngine;

namespace DingoUnityExtensions.UnityViewProviders.Toggle
{
    public class PositionToggleSwapInfo : ToggleSwapInfoBase
    {
        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private Vector2 _offPosition;
        [SerializeField] private Vector2 _onPosition;

        public override void SetViewActive(bool value)
        {
            var pos = value ? _onPosition : _offPosition;
            _rectTransform.anchoredPosition = pos;
        }
    }
}