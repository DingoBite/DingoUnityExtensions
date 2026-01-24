using System.Collections.Generic;
using System.Linq;
using DingoUnityExtensions.UnityViewProviders.BoxedValue;

namespace DingoUnityExtensions.UnityViewProviders.FormatText
{
    public readonly struct PreFormatText
    {
        public readonly object[] ArgsArray;
        public readonly string Fallback;
        
        public PreFormatText(string fallback, IEnumerable<BoxedValueWrapper> boxedValues = null)
        {
            Fallback = fallback;
            ArgsArray = boxedValues?.Select(b => b.BoxedValue).ToArray();
        }
        
        public PreFormatText(string fallback, params object[] values)
        {
            Fallback = fallback;
            ArgsArray = values;
        }
    }
}
