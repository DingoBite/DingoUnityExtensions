#if NEWTONSOFT_EXISTS
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.UnityConverters.Math;

namespace DingoUnityExtensions.Serialization
{
    public static class JsonOptions
    {
        public static readonly JsonSerializerSettings PythonOptions = new ();
        public static readonly JsonSerializerSettings CSharpOptions = new ();

        static JsonOptions()
        {
            PythonOptions.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            PythonOptions.ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            };
            
            CSharpOptions.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            CSharpOptions.Converters.Add(new Vector2Converter());
            CSharpOptions.Converters.Add(new Vector3Converter());
            CSharpOptions.Converters.Add(new QuaternionConverter());
            CSharpOptions.Converters.Add(new Matrix4x4Converter());
        }
    }
}
#endif
