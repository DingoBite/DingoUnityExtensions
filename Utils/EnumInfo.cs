using System;
using System.Collections.Generic;
using System.Linq;

namespace DingoUnityExtensions.Utils
{
    public static class EnumInfo<T> where T : Enum
    {
        private static readonly List<T> _values;
        public static readonly IReadOnlyList<T> Values;

        public static Dictionary<T, TValue> CreateAndPopulate<TValue>(Func<TValue> factory) => Values.ToDictionary(key => key, _ => factory());

        static EnumInfo()
        {
            _values = Enum.GetValues(typeof(T)).Cast<T>().ToList();
            Values = _values;
        }

        public static int GetIndex(T value) => _values.IndexOf(value);
    }
}