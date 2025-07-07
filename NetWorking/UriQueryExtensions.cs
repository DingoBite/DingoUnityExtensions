using System.Collections.Generic;
using System.Collections.Specialized;

namespace DingoUnityExtensions.NetWorking
{
    public static class UriQueryExtensions
    {
        public static NameValueCollection AppendParameters(this NameValueCollection nameValueCollection, IEnumerable<KeyValuePair<string, string>> parameters)
        {
            foreach (var (key, value) in parameters)
            {
                nameValueCollection[key] = value;
            }
            return nameValueCollection;
        }

        public static NameValueCollection AppendParameters(this NameValueCollection nameValueCollection, IEnumerable<(string, string)> parameters)
        {
            foreach (var (key, value) in parameters)
            {
                nameValueCollection[key] = value;
            }
            return nameValueCollection;
        }
    }
}