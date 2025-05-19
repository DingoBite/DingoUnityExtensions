using DingoUnityExtensions.UnityViewProviders.Core.Data;
using DingoUnityExtensions.UnityViewProviders.Toggle.Core;
using UnityEngine;

namespace DingoUnityExtensions.UnityViewProviders.Toggle
{
    public class PositionToggleSwapInfo : ToggleSwapInfoBase
    {
        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private Vector2 _offPosition;
        [SerializeField] private Vector2 _onPosition;

        public override void SetViewActive(BoolTimeContext value)
        {
            var pos = value.Bool() ? _onPosition : _offPosition;
            _rectTransform.anchoredPosition = pos;
        }
    }
}