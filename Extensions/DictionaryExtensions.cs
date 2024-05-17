using System.Collections.Generic;
using System.Linq;

namespace DingoUnityExtensions.Extensions
{
    public static class DictionaryExtensions
    {
        public static void AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (dictionary.ContainsKey(key))
                dictionary[key] = value;
            else
                dictionary.Add(key, value);
        }
        
        public static TValue GetOrAddAndGet<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key) where TValue : new()
        {
            if (!dictionary.TryGetValue(key, out var value))
            {
                value = new TValue();
                dictionary.Add(key, value);
            }
            else
            {
                dictionary[key] = value;
            }

            return value;
        }

        public static Dictionary<T1, T2> ToDictionary<T1, T2>(this IEnumerable<(T1, T2)> pairs)
        {
            return pairs.ToDictionary(p => p.Item1, p => p.Item2);
        }
        
        public static Dictionary<T1, T2> ToDictionary<T1, T2>(this IReadOnlyDictionary<T1, T2> dict)
        {
            return dict.ToDictionary(p => p.Key, p => p.Value);
        }
    }
}