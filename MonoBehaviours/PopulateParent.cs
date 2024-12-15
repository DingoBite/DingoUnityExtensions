using System.Collections.Generic;
using System.Linq;
using DingoUnityExtensions.Extensions;
using DingoUnityExtensions.Generic;
using DingoUnityExtensions.Utils;
using UnityEngine;

namespace DingoUnityExtensions.MonoBehaviours
{
    public class PopulateParent : MonoBehaviour
    {
        [SerializeField] private int _searchDepth = 1;
        [SerializeField] private bool _dirty = true;
        
        private static class ParentCache<T>
        {
            public static readonly Dictionary<PopulateParent, List<T>> Dict = new();
        }
        
        public IReadOnlyList<T> Get<T>() => ParentCache<T>.Dict.GetValueOrDefault(this);

        public void ForceRepopulate<T>()
        {
            var list = ParentCache<T>.Dict.GetOrAddAndGet(this);
            list.Clear();
            List<IContainer<T>> containers;
            List<IEnumerableContainer<T>> containersEnumerable;
            List<IEnumerableContainer<IContainer<T>>> containersInheritEnumerable;
            
            if (_searchDepth > 0)
            {
                containers = transform.FindComponents<IContainer<T>>(false, _searchDepth);
                containersEnumerable = transform.FindComponents<IEnumerableContainer<T>>(false, _searchDepth);
                containersInheritEnumerable = transform.FindComponents<IEnumerableContainer<IContainer<T>>>(false, _searchDepth);
            }
            else
            {
                containers = new List<IContainer<T>>();
                containersEnumerable = new List<IEnumerableContainer<T>>();
                containersInheritEnumerable = new List<IEnumerableContainer<IContainer<T>>>();
                transform.GetComponents(containers);
                transform.GetComponents(containersEnumerable);
                transform.GetComponents(containersInheritEnumerable);
            }
            
            var collection = containers
                .Select(e => e.ComponentElement)
                .Concat(containersEnumerable.SelectMany(e => e.ComponentElements))
                .Concat(containersInheritEnumerable.SelectMany(e => e.ComponentElements.Where(c => c.ComponentElement != null).Select(c => c.ComponentElement)))
                .Where(e => e != null);
            
            list.AddRange(collection);
            _dirty = false;
        }
        
        public void Repopulate<T>()
        {
            if (!_dirty)
                return;
            ForceRepopulate<T>();
        }

        private void OnTransformChildrenChanged() => _dirty = true;
        private void OnTransformParentChanged() => _dirty = true;
        public void SetDirty() => _dirty = true;
    }
}