using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DingoUnityExtensions.Utils.Search
{
    public class SearchGroup<T, TOrderValue>  where T : Component where TOrderValue : IComparable<TOrderValue>
    {
        private readonly Func<T, string, bool> _filterFunc;
        private readonly Func<T, TOrderValue> _orderFunc;

        private string _lastFilter;
        private bool _forwardSort;

        public SearchGroup(Func<T, string, bool> filterFunc, Func<T, TOrderValue> orderFunc, bool forwardSort = true)
        {
            _forwardSort = forwardSort;
            _orderFunc = orderFunc;
            _filterFunc = filterFunc;
        }
        
        public void Filter(string filter, IEnumerable<T> elements, bool isOrderGameObjects = true)
        {
            _lastFilter = filter;
            ReFilter(elements, isOrderGameObjects);
        }

        public void ReFilter(IEnumerable<T> elements, bool isOrderGameObjects = true)
        {
            var filteredElements = HideByFilter(elements);
            if (isOrderGameObjects)
                OrderGameObjects(filteredElements);
        }

        private IEnumerable<T> HideByFilter(IEnumerable<T> elements)
        {
            foreach (var element in elements)
            {
                var isFiltered = _filterFunc.Invoke(element, _lastFilter);
                element.gameObject.SetActive(isFiltered);
                if (isFiltered)
                    yield return element;
            }
        }

        private void OrderGameObjects(IEnumerable<T> elements)
        {
            var orderedEnumerable = _forwardSort ? elements.OrderBy(_orderFunc) : elements.OrderByDescending(_orderFunc);
            foreach (var element in orderedEnumerable)
            {
                element.transform.SetAsLastSibling();
            }
        }
    }
}