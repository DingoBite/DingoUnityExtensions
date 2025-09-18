using System;
using DingoUnityExtensions.UnityViewProviders.Core;

namespace DingoUnityExtensions.UnityViewProviders.BoxedValue
{
    public readonly struct BoxedValueWrapper
    {
        public readonly object BoxedValue;
        public readonly Type Type;
        public readonly Func<object, object> Converter;

        public BoxedValueWrapper(object boxedValue, Type type = null, Func<object, object> converter = null)
        {
            BoxedValue = boxedValue;
            Type = type == null && boxedValue != null ? boxedValue.GetType() : type;
            Converter = converter;
        }

        public static BoxedValueWrapper None => new (null);
    }
    
    public abstract class BoxedValueContainer : ValueContainer<BoxedValueWrapper>
    {
    }

    public enum ValueUpdateBehaviour
    {
        None,
        ActiveManage
    }
}