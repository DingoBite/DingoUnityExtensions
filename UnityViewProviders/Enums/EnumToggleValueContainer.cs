using System;
using AYellowpaper.SerializedCollections;
using DingoUnityExtensions.UnityViewProviders.Core;
using UnityEngine;

namespace DingoUnityExtensions.UnityViewProviders.Enums
{
    public abstract class EnumToggleValueContainer<TEnum> : ValueContainer<TEnum> where TEnum : Enum
    {
        [SerializeField] private SerializedDictionary<TEnum, ValueContainer<bool>> _eventsDict;

        protected override void PreviousValueFree(TEnum previousData)
        {
            if (_eventsDict.TryGetValue(previousData, out var e))
                e.UpdateValueWithoutNotify(false);
        }

        protected override void SetValueWithoutNotify(TEnum value)
        {
            if (_eventsDict.TryGetValue(value, out var e))
                e.UpdateValueWithoutNotify(true);
        }
    }
}