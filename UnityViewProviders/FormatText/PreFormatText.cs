using System.Collections.Generic;
using System.Linq;
using DingoUnityExtensions.UnityViewProviders.BoxedValue;

namespace DingoUnityExtensions.UnityViewProviders.FormatText
{
    public readonly struct PreFormatText
    {
        public readonly object[] ArgsArray;
        public readonly string Fallback;
        
        public PreFormatText(IEnumerable<BoxedValueWrapper> boxedValues = null, string fallback = null)
        {
            Fallback = fallback;
            ArgsArray = boxedValues?.Select(b => b.BoxedValue).ToArray();
        }
        
        public PreFormatText(object value, string fallback = null)
        {
            Fallback = fallback;
            ArgsArray = new[] { value };
        }
       
        public PreFormatText(string fallback = null, params object[] values)
        {
            Fallback = fallback;
            ArgsArray = values;
        }
    }
}
