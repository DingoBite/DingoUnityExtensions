using System;
using AYellowpaper.SerializedCollections;
using DingoUnityExtensions.UnityViewProviders.Core;
using UnityEngine;
using UnityEngine.Events;

namespace DingoUnityExtensions.UnityViewProviders.Enums
{
    public abstract class EnumEventValueContainer<TEnum> : ValueContainer<TEnum> where TEnum : Enum
    {
        [SerializeField] private SerializedDictionary<TEnum, UnityEvent> _eventsDict;
        [SerializeField] private UnityEvent<TEnum> _events;

        protected override void SetValueWithoutNotify(TEnum value)
        {
            if (_eventsDict.TryGetValue(value, out var e))
                e.Invoke();
            _events?.Invoke(value);
        }
    }
}