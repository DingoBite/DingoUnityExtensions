#if NEWTONSOFT_EXISTS
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DingoUnityExtensions.Serialization
{
    public static class JsonOptions
    {
        public static readonly JsonSerializerSettings Options = new ();

        static JsonOptions()
        {
            Options.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            Options.ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            };
        }
    }
}
#endif
