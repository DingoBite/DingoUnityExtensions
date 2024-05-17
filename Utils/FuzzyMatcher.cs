using System;

namespace DingoUnityExtensions.Utils
{
    public static class FuzzyMatcher
    {
        public static double SubstringSimilarityWithFallbackLevenshtein(string source, string target)
        {
            var maxLength = Math.Max(source.Length, target.Length);
            var v1IndexOf = source.ToLower().IndexOf(target.ToLower(), StringComparison.Ordinal);
            var v2IndexOf = target.ToLower().IndexOf(source.ToLower(), StringComparison.Ordinal);
            if (v1IndexOf < 0 && v2IndexOf < 0)
                return 0.2 * LevenshteinSimilarity(source, target);

            if (v1IndexOf == v2IndexOf)
            {
                var ratio = source.Length < target.Length ? 1 : 0;
                return 0.8 + ratio * 0.2;
            }
            
            double substringRatio;
            if (v1IndexOf != -1 && v1IndexOf < v2IndexOf)
                substringRatio = (double) (maxLength - v1IndexOf) / maxLength;
            else 
                substringRatio = (double) (maxLength - v2IndexOf) / maxLength;
            
            substringRatio = 0.2 + substringRatio * 0.6;
            return substringRatio;
        }

        public static double SubstringSimilarity(string source, string target)
        {
            var maxLength = Math.Max(source.Length, target.Length);
            var v1IndexOf = source.ToLower().IndexOf(target.ToLower(), StringComparison.Ordinal);
            var v2IndexOf = target.ToLower().IndexOf(source.ToLower(), StringComparison.Ordinal);
            if (v1IndexOf < 0 && v2IndexOf < 0)
                return 0;

            if (v1IndexOf == v2IndexOf)
            {
                var ratio = source.Length > target.Length ? 1 : 0;
                return 0.5 + ratio * 0.5;
            }
            
            double substringRatio;
            if (v1IndexOf != -1 && v1IndexOf < v2IndexOf)
                substringRatio = (double) (maxLength - v1IndexOf) / maxLength;
            else 
                substringRatio = (double) (maxLength - v2IndexOf) / maxLength;
            
            substringRatio *= 0.5;
            return substringRatio;
        }
        
        public static double LevenshteinSimilarity(string source, string target)
        {
            var levenshteinDistance = LevenshteinDistance(source, target);
            var maxLength = Math.Max(source.Length, target.Length);
            if (maxLength == 0)
                return 1.0;
            if (levenshteinDistance < float.Epsilon)
                return 0;
            
            return 1.0 - (double)levenshteinDistance / maxLength;
        }
        
        public static int LevenshteinDistance(string source, string target)
        {
            if (string.IsNullOrEmpty(source))
            {
                return string.IsNullOrEmpty(target) ? 0 : target.Length;
            }

            if (string.IsNullOrEmpty(target))
            {
                return source.Length;
            }

            var sourceLength = source.Length;
            var targetLength = target.Length;
            var distance = new int[sourceLength + 1, targetLength + 1];

            for (var i = 0; i <= sourceLength; distance[i, 0] = i++) { }
            for (var j = 0; j <= targetLength; distance[0, j] = j++) { }

            for (var i = 1; i <= sourceLength; i++)
            {
                for (var j = 1; j <= targetLength; j++)
                {
                    var cost = (target[j - 1] == source[i - 1]) ? 0 : 1;

                    distance[i, j] = Math.Min(
                        Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1),
                        distance[i - 1, j - 1] + cost);
                }
            }

            return distance[sourceLength, targetLength];
        }
    }
}