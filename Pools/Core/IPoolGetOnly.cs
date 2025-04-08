using System.Collections.Generic;
using UnityEngine;

namespace DingoUnityExtensions.Pools.Core
{
    public enum SortTransformOrderOption
    {
        None,
        AsLast,
        AsFirst
    }

    public interface IPoolGetOnly<out T> where T : MonoBehaviour
    {
        public IReadOnlyList<T> PulledElements { get; }
        public T PullElement();
        public void Clear();
    }
}