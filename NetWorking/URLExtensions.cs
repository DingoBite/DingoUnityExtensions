using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DingoUnityExtensions.NetWorking
{
    public static class URLExtensions
    {
        public static string ParametersToURL(this IEnumerable<KeyValuePair<string, string>> parameters, bool addParamMark = true)
            => ParametersToURL(parameters.Select(p => (p.Key, p.Value)), addParamMark);
        
        public static string ParametersToURL(this IEnumerable<(string, string)> parameters, bool addParamMark = true)
        {
            var sb = new StringBuilder();
            if (addParamMark)
                sb.Append("?");
            var anyElements = false;
            foreach (var (key, value) in parameters)
            {
                if (anyElements)
                    sb.Append("&");
                sb.Append($"{key}={value}");
                anyElements = true;
            }

            if (!anyElements)
                return string.Empty;
            return sb.ToString();
        }

        public static IEnumerable<(string, string)> CreteEmpty() => Enumerable.Empty<(string, string)>();
        public static IEnumerable<(string, string)> CreateParametersEnumerable(string key, string value) => Enumerable.Repeat<(string, string)>((key, value), 1);
        public static IEnumerable<(string, string)> AppendParametersEnumerable(this IEnumerable<(string, string)> parameters, string key, string value) => parameters.Append((key, value));
    }
}