using System;

namespace DingoUnityExtensions.Configurator.Core
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class ConfigurableAttribute : Attribute
    {
        public readonly string ConfigFieldName;

        public ConfigurableAttribute(string configFieldName)
        {
            ConfigFieldName = configFieldName;
        }
    }
}