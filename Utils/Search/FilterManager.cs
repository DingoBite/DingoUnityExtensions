using DingoUnityExtensions.MonoBehaviours;
using DingoUnityExtensions.UnityViewProviders.Core;
using UnityEngine;

namespace DingoUnityExtensions.Utils.Search
{
    public class FilterManager : SubscribableBehaviour
    {
        [SerializeField] private ValueContainer<string> _inputField;
        [SerializeField] private GameObject _filterableBehaviour;
        
        private IFilterable _filterable;

        private IFilterable Filterable => _filterable ??= _filterableBehaviour.GetComponent<IFilterable>();

        private void Filter(string nameFilter) => Filterable?.Filter(nameFilter);

        protected override void SubscribeOnly() => _inputField.OnValueChange += Filter;
        protected override void UnsubscribeOnly() => _inputField.OnValueChange -= Filter;
    }
}