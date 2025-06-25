using System;
using DingoUnityExtensions.UnityViewProviders.Core;
using UnityEngine;

namespace DingoUnityExtensions.UnityViewProviders.Float
{
    public enum RoundTypeToInt
    {
        None,
        Floor,
        Ceil,
        Round
    }

    public class NumberContainer : ValueContainer<float>
    {
        [SerializeField] private ValueContainer<string> _stringContainer;
        [SerializeField] private string _format = "0.000";
        [SerializeField] private RoundTypeToInt _roundType = RoundTypeToInt.None;
        [SerializeField] private float _multiplier = 1f;

        protected override void SetValueWithoutNotify(float value)
        {
            value *= _multiplier;
            var str = _roundType switch
            {
                RoundTypeToInt.Floor => ((int)Math.Floor(value)).ToString(_format),
                RoundTypeToInt.Ceil => ((int)Math.Ceiling(value)).ToString(_format),
                RoundTypeToInt.Round => ((int)Math.Round(value)).ToString(_format),
                _ => value.ToString(_format)
            };
            _stringContainer.UpdateValueWithoutNotify(str);
        }
    }
}