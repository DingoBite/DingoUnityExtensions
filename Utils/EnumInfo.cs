using System;
using System.Collections.Generic;
using System.Linq;

namespace DingoUnityExtensions.Utils
{
    public static class EnumInfo<T> where T : Enum
    {
        public static readonly IReadOnlyList<T> Values;

        public static Dictionary<T, TValue> CreateAndPopulate<TValue>(Func<TValue> factory) => Values.ToDictionary(key => key, _ => factory());

        static EnumInfo()
        {
            Values = Enum.GetValues(typeof(T)).Cast<T>().ToList();
        }
    }
}