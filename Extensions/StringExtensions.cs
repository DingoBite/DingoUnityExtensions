using System;
using System.Globalization;
using UnityEngine;

namespace DingoUnityExtensions.Extensions
{
    public static class StringExtensions
    {
        public static string AppendedSubstring(this string original, string modified)
        {
            if (string.IsNullOrEmpty(original) || string.IsNullOrEmpty(modified))
                return modified;

            var originalLength = original.Length;
            var modifiedLength = modified.Length;

            if (modifiedLength <= originalLength)
                return "";

            if (modified.StartsWith(original))
                return modified[originalLength..];

            return "";
        }
        
        public static string FindFirstDifferentCharacter(this string original, string modified)
        {
            if (string.IsNullOrEmpty(original) || string.IsNullOrEmpty(modified))
                return null;

            var minLength = Math.Min(original.Length, modified.Length);

            for (var i = 0; i < minLength; i++)
            {
                if (original[i] != modified[i])
                    return modified[i].ToString();
            }

            if (modified.Length > minLength)
                return modified[minLength].ToString();

            return null;
        }

        public static int IntConvertOrDefault(this string str, int @default = 0)
        {
            if (int.TryParse(str, out var i))
                return i;
            return @default;
        }

        public static float FloatConvertOrDefault(this string str, float @default = 0)
        {
            if (string.IsNullOrWhiteSpace(str))
                return @default;
            if (float.TryParse(str.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out var i))
                return i;
            return @default;
        }
        
        public static int FloatConvertToIntOrDefault(this string str, int @default = 0)
        {
            if (string.IsNullOrWhiteSpace(str))
                return @default;
            if (float.TryParse(str.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out var v))
                return (int)(v + float.Epsilon);
            return @default;
        }

        public static Vector2Int FloatRangeOrSingleConvertToIntOrDefault(this string str, string delimiter, int @default = 0)
        {
            if (string.IsNullOrWhiteSpace(str))
                return new Vector2Int(@default, @default);
            var split = str.Split(delimiter);
            if (split.Length <= 1)
            {
                return new Vector2Int(@FloatConvertToIntOrDefault(str, @default), @FloatConvertToIntOrDefault(str, @default));
            }
            var a = split[0].Trim();
            var b = split[1].Trim();
            return new Vector2Int(FloatConvertToIntOrDefault(a, @default), FloatConvertToIntOrDefault(b, @default));
        }

        public static Vector2Int FloatRangeConvertToIntOrDefault(this string str, string delimiter, int @default = 0)
        {
            if (string.IsNullOrWhiteSpace(str))
                return new Vector2Int(@default, @default);
            var split = str.Split(delimiter);
            if (split.Length <= 1)
                return new Vector2Int(@default, @default);
            var a = split[0].Trim();
            var b = split[1].Trim();
            return new Vector2Int(FloatConvertToIntOrDefault(a, @default), FloatConvertToIntOrDefault(b, @default));
        }
        
        public static Vector2 FloatRangeConvertOrDefault(this string str, string delimiter, float @default = 0)
        {
            if (string.IsNullOrWhiteSpace(str))
                return new Vector2(@default, @default);
            var split = str.Split(delimiter);
            if (split.Length <= 1)
                return new Vector2(@default, @default);
            var a = split[0].Trim();
            var b = split[1].Trim();
            return new Vector2(FloatConvertOrDefault(a, @default), FloatConvertOrDefault(b, @default));
        }
        
        public static Vector2Int IntRangeConvertOrDefault(this string str, string delimiter, int @default = 0)
        {
            if (string.IsNullOrWhiteSpace(str))
                return new Vector2Int(@default, @default);
            var split = str.Split(delimiter);
            if (split.Length <= 1)
                return new Vector2Int(@default, @default);
            var a = split[0].Trim();
            var b = split[1].Trim();
            return new Vector2Int(IntConvertOrDefault(a, @default), IntConvertOrDefault(b, @default));
        }
        
        public static string FirstToUpper(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return string.Empty;

            if (str.Length == 1)
                return char.ToUpper(str[0]).ToString();

            return $"{char.ToUpper(str[0])}{str[1..]}";
        }
    }
}