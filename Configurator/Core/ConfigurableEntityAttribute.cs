using System;

namespace DingoUnityExtensions.Configurator.Core
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ConfigurableEntityAttribute : Attribute
    {
        public readonly int Priority;
        public readonly Type ConfigType;

        public ConfigurableEntityAttribute(Type configType, int priority = int.MaxValue)
        {
            ConfigType = configType;
            Priority = priority;
        }
    }
}