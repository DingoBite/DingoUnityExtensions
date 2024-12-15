using System.Collections.Generic;

namespace DingoUnityExtensions.Generic
{
    public interface IEnumerableContainer<out T>
    {
        public IEnumerable<T> ComponentElements { get; }
    }

    public interface IContainer<out T>
    {
        public T ComponentElement { get; }
    }
}