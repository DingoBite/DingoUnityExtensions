#if NEWTONSOFT_EXISTS
using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
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
            AddAllConvertersFromNamespace(settings, "Newtonsoft.Json.UnityConverters.Mathematics");
        }
        
        public static void AddAllConvertersFromNamespace(JsonSerializerSettings settings, string targetNamespace)
        {
            var existing = new HashSet<Type>();
            for (var i = 0; i < settings.Converters.Count; i++)
                existing.Add(settings.Converters[i].GetType());

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (var a = 0; a < assemblies.Length; a++)
            {
                var asm = assemblies[a];
                var asmName = asm.GetName().Name ?? string.Empty;

                if (asmName.IndexOf("UnityConverters", StringComparison.OrdinalIgnoreCase) < 0 && asmName.IndexOf("Newtonsoft", StringComparison.OrdinalIgnoreCase) < 0)
                    continue;

                Type[] types;
                try
                {
                    types = asm.GetTypes();
                }
                catch (ReflectionTypeLoadException e)
                {
                    types = e.Types;
                }

                if (types == null)
                    continue;

                for (var t = 0; t < types.Length; t++)
                {
                    var type = types[t];
                    if (type == null)
                        continue;

                    if (type.IsAbstract)
                        continue;
                    if (type.IsGenericTypeDefinition)
                        continue;
                    if (type.Namespace != targetNamespace)
                        continue;
                    if (!typeof(JsonConverter).IsAssignableFrom(type))
                        continue;
                    if (type.GetConstructor(Type.EmptyTypes) == null)
                        continue;
                    if (existing.Contains(type))
                        continue;

                    settings.Converters.Add((JsonConverter)Activator.CreateInstance(type));
                    existing.Add(type);
                }
            }
        }
    }
}
#endif
