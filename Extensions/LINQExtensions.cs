using System;
using System.Collections.Generic;

namespace DingoUnityExtensions.Extensions
{
    public static class LINQExtensions
    {
        public static T MinByScalar<T>(this IEnumerable<T> values, Func<T, float> scalarFunc)
        {
            var minScalar = float.MaxValue;
            T minValue = default;
            foreach (var value in values)
            {
                var scalar = scalarFunc(value);
                if (scalar < minScalar)
                {
                    minScalar = scalar;
                    minValue = value;
                }
            }

            return minValue;
        }
        
        public static T MaxByScalar<T>(this IEnumerable<T> values, Func<T, float> scalarFunc)
        {
            var maxScalar = float.MinValue;
            T maxValue = default;
            foreach (var value in values)
            {
                var scalar = scalarFunc(value);
                if (scalar > maxScalar)
                {
                    maxScalar = scalar;
                    maxValue = value;
                }
            }

            return maxValue;
        }
    }
}