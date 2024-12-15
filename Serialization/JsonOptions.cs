#if NEWTONSOFT_EXISTS
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.UnityConverters.Math;

namespace DingoUnityExtensions.Serialization
{
    public static class JsonOptions
    {
        public static readonly JsonSerializerSettings SnakeCaseOptions = new ();
        public static readonly JsonSerializerSettings CamelCaseOptions = new ();
        public static readonly JsonSerializerSettings UnitySerializationOptions = new ();

        static JsonOptions()
        {
            SnakeCaseOptions.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            SnakeCaseOptions.ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            };
            AddUnityConverters(SnakeCaseOptions);
            
            CamelCaseOptions.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            CamelCaseOptions.ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            };
            AddUnityConverters(CamelCaseOptions);

            UnitySerializationOptions.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            AddUnityConverters(UnitySerializationOptions);
        }

        private static void AddUnityConverters(JsonSerializerSettings settings)
        {
            settings.Converters.Add(new Vector2Converter());
            settings.Converters.Add(new Vector3Converter());
            settings.Converters.Add(new QuaternionConverter());
            settings.Converters.Add(new Matrix4x4Converter());
        }
    }
}
#endif
