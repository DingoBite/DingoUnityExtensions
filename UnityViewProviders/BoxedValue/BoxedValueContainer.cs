using System;
using DingoUnityExtensions.UnityViewProviders.Core;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace DingoUnityExtensions.UnityViewProviders.BoxedValue
{
    [Serializable, Preserve]
    public readonly struct BoxedValueWrapper
    {
        public readonly object BoxedValue;
        public readonly Type Type;

        [JsonConstructor]
        private BoxedValueWrapper(object boxedValue = null, Type type = null)
        {
            BoxedValue = boxedValue;
            Type = type == null && boxedValue != null ? boxedValue.GetType() : type;
        }

        public static BoxedValueWrapper Create<T>(T value) => new(value, typeof(T));
        public static BoxedValueWrapper Create(object value, Type type) => new(value, type);
        public static BoxedValueWrapper None => new (null);
    }
    
    public abstract class BoxedValueContainer : ValueContainer<BoxedValueWrapper>
    {
    }

    public enum ValueUpdateBehaviour
    {
        None,
        ActiveManage,
        CreateChild,
    }
}