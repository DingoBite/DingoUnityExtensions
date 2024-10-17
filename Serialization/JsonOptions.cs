#if NEWTONSOFT_EXISTS
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

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
        }
    }
}
#endif
