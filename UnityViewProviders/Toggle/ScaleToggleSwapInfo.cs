using DingoUnityExtensions.UnityViewProviders.Core.Data;
using DingoUnityExtensions.UnityViewProviders.Toggle.Core;
using UnityEngine;

namespace DingoUnityExtensions.UnityViewProviders.Toggle
{
    public class ScaleToggleSwapInfo : ToggleSwapInfoBase
    {
        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private Vector2 _offScale;
        [SerializeField] private Vector2 _onScale;

        public override void SetViewActive(BoolTimeContext value)
        {
            var pos = value.Bool() ? _onScale : _offScale;
            _rectTransform.localScale = new Vector3(pos.x, pos.y, 1);
        }
    }
}