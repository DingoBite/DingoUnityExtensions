using System;
using System.Globalization;
using AYellowpaper.SerializedCollections;
using DingoUnityExtensions.UnityViewProviders.Core;
using UnityEngine;

namespace DingoUnityExtensions.UnityViewProviders
{
    public class TimeSpanContainer : ValueContainer<TimeSpan>
    {
        [SerializeField] private SerializedDictionary<string, ValueContainer<string>> _dateViews;
        
        protected override void SetValueWithoutNotify(TimeSpan value)
        {
            foreach (var (key, view) in _dateViews)
            {
                view.UpdateValueWithoutNotify(value.ToString(key.Replace(@"\\", "\\"), CultureInfo.InvariantCulture));
            }
        }
    }
}