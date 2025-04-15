using System;
using System.Linq;
using System.Reflection;

namespace DingoUnityExtensions.Utils
{
    public static class ReflectionUtils
    {
        public static Type[] FindAllSubclasses<T>()
        {
            var baseType = typeof(T);
            var assembly = Assembly.GetAssembly(baseType);
            var types = assembly.GetTypes();
            var subclasses = types.Where(t => t.IsSubclassOf(baseType) && !t.IsAbstract).ToArray();
            return subclasses;
        } 
        
        public static Type[] FindAllInterfaceImplements<T>()
        {
            var baseType = typeof(T);
            var assembly = Assembly.GetAssembly(baseType);
            var types = assembly.GetTypes();
            var subclasses = types.Where(t => t.GetInterfaces().Contains(baseType) && !t.IsAbstract).ToArray();
            return subclasses;
        } 
    }
}