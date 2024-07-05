using System;
using DingoUnityExtensions.UnityViewProviders.Core;
using UnityEngine;

namespace DingoUnityExtensions.UnityViewProviders
{
    public class VectorView : ValueContainer<Vector3>
    {
        [SerializeField] private bool _roundToInt;
        [SerializeField] private string _format;
        [SerializeField] private ValueContainer<string> _x;
        [SerializeField] private ValueContainer<string> _y;
        [SerializeField] private ValueContainer<string> _z;
        
        protected override void SetValueWithoutNotify(Vector3 value)
        {
            if (_roundToInt)
            {
                _x.UpdateValueWithoutNotify(Round(value.x).ToString(_format));
                _y.UpdateValueWithoutNotify(Round(value.y).ToString(_format));
                _z.UpdateValueWithoutNotify(Round(value.z).ToString(_format));
            }
            else
            {
                _x.UpdateValueWithoutNotify(value.x.ToString(_format));
                _y.UpdateValueWithoutNotify(value.y.ToString(_format));
                _z.UpdateValueWithoutNotify(value.z.ToString(_format));
            }
        }

        private static int Round(float value) => (int)Math.Round(value);
        
        protected override void SubscribeOnly() { }
        protected override void UnsubscribeOnly() { }
    }
}