using System;
using DingoUnityExtensions.UnityViewProviders.Core;

namespace DingoUnityExtensions.UnityViewProviders.BoxedValue
{
    public readonly struct BoxedValueWrapper
    {
        public readonly object BoxedValue;
        public readonly Type Type;

        private BoxedValueWrapper(object boxedValue = null, Type type = null)
        {
            BoxedValue = boxedValue;
            Type = type == null && boxedValue != null ? boxedValue.GetType() : type;
        }

        public static BoxedValueWrapper Create<T>(T value) => new(value, typeof(T));
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