using System;
using System.Globalization;

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

        public static int IntConvertOrDefault(this string str)
        {
            if (int.TryParse(str, out var i))
                return i;
            return 0;
        }

        public static float FloatConvertOrDefault(this string str)
        {
            if (string.IsNullOrWhiteSpace(str))
                return 0;
            if (float.TryParse(str.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out var i))
                return i;
            return 0;
        }
        
        public static int FloatConvertToIntOrDefault(this string str)
        {
            if (string.IsNullOrWhiteSpace(str))
                return 0;
            if (float.TryParse(str.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out var v))
                return (int)(v + float.Epsilon);
            return 0;
        }
    }
}