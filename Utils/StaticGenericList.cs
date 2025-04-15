using System.Collections.Generic;

namespace DingoUnityExtensions.Utils
{
    public static class StaticGenericList<T>
    {
        private static readonly List<T> InstancesInternal = new ();
        public static IReadOnlyList<T> Instances => Instances;
        
        public static void Clear() => InstancesInternal.Clear();
        
        public static void Set(IEnumerable<T> instances)
        {
            Clear();
            InstancesInternal.AddRange(instances);
        }
    }
}