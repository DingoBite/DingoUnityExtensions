using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = System.Random;

namespace DingoUnityExtensions.Extensions
{
    public static class EnumerableExtensions
    {
        private static readonly Random Random = new();

        public static T GetRandomElement<T>(this ICollection<T> enumerable)
        {
            var index = enumerable.GetRandomIndex();
            return enumerable.ElementAt(index);
        }
        
        public static int GetRandomIndex<T>(this ICollection<T> enumerable)
        {
            var index = Random.Next(0, enumerable.Count - 1);
            return index;
        }
        
        public static void AddOrUpdate<T>(this IList<T> list, int index, T value)
        {
            if (index == list.Count)
                list.Add(value);
            else
                list[index] = value;
        }

        public static void AddIfNotContains<T>(this IList<T> list, T value)
        {
            if (list.Contains(value))
                return;
            list.Add(value);
        }
        
        public static TValue GetOrAddAndGet<TValue>(this IList<TValue> collection, Func<TValue, bool> predicate) where TValue : new()
        {
            var value = collection.FirstOrDefault(predicate);
            if (value == null)
            {
                value = new TValue();
                collection.Add(value);
            }

            return value;
        }

        public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> enumerable)
        {
            return enumerable.ToDictionary(p => p.Key, p => p.Value);
        }
        
        public static T MinBy<T>(this IEnumerable<T> list, Func<T, IComparable> selector)
        {
            IComparable lowest = null;
            var result = default(T);
            foreach (var elem in list)
            {
                var currElemVal = selector(elem);
                if (lowest == null || currElemVal.CompareTo(lowest) < 0)
                {
                    lowest = currElemVal;
                    result = elem;
                }
            }
            return result;
        }

        public static T MaxBy<T>(this IEnumerable<T> list, Func<T, IComparable> selector)
        {
            IComparable highest = null;
            var result = default(T);
            foreach (var elem in list)
            {
                var currElemVal = selector(elem);
                if (highest == null || currElemVal.CompareTo(highest) > 0)
                {
                    highest = currElemVal;
                    result = elem;
                }
            }
            return result;
        }
        
        public static (T, int i) MinByWithIndex<T>(this IReadOnlyList<T> list, Func<T, IComparable> selector)
        {
            IComparable lowest = null;
            var result = default(T);
            var index = -1;
            for (var i = 0; i < list.Count; i++)
            {
                var elem = list[i];
                var currElemVal = selector(elem);
                if (lowest == null || currElemVal.CompareTo(lowest) < 0)
                {
                    lowest = currElemVal;
                    result = elem;
                    index = i;
                }
            }

            return (result, index);
        }

        public static (T, int i) MaxByWithIndex<T>(this IReadOnlyList<T> list, Func<T, IComparable> selector)
        {
            IComparable highest = null;
            var result = default(T);
            var index = -1;
            for (var i = 0; i < list.Count; i++)
            {
                var elem = list[i];
                var currElemVal = selector(elem);
                if (highest == null || currElemVal.CompareTo(highest) > 0)
                {
                    highest = currElemVal;
                    result = elem;
                    index = i;
                }
            }
            return (result, index);
        }
        
        public static void ProcessCollectionErrorHandle<T>(this IEnumerable<T> collection, Action<T> processAction, Func<T, string> catchMessage)
        {
            if (collection == null)
                return;
            
            foreach (var element in collection.Where(s => s != null))
            {
                try
                {
                    processAction.Invoke(element);
                }
                catch (Exception e)
                {
                    Debug.LogError(catchMessage(element));
                    Debug.LogException(e);
                }
            }
        }
        
        public static async Task ProcessCollectionErrorHandleAsync<T>(this IEnumerable<T> collection, Func<T, Task> processAction, Func<T, string> catchMessage)
        {
            if (collection == null)
                return;
            
            foreach (var element in collection.Where(s => s != null))
            {
                try
                {
                    await processAction.Invoke(element);
                }
                catch (Exception e)
                {
                    Debug.LogError(catchMessage(element));
                    Debug.LogException(e);
                }
            }
        }
        
        public static async UniTask ProcessCollectionErrorHandleAsync<T>(this IEnumerable<T> collection, Func<T, UniTask> processAction, Func<T, string> catchMessage)
        {
            if (collection == null)
                return;
            
            foreach (var element in collection.Where(s => s != null))
            {
                try
                {
                    await processAction.Invoke(element);
                }
                catch (Exception e)
                {
                    Debug.LogError(catchMessage(element));
                    Debug.LogException(e);
                }
            }
        }
    }
}