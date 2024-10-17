using DingoUnityExtensions.Extensions;
using DingoUnityExtensions.MonoBehaviours;
using DingoUnityExtensions.UnityViewProviders.Core;
using UnityEngine;

namespace MobileExerciseAnalyzerView.View.Screens.History.ResultsScroll
{
    public class FilterManager : SubscribableBehaviour
    {
        [SerializeField] private ValueContainer<string> _inputField;
        [SerializeField] private GameObject _filterableBehaviour;
        
        private IFilterable _filterable;

        private IFilterable Filterable => _filterable ??= _filterableBehaviour.GetComponent<IFilterable>();

        private void Filter(string nameFilter) => Filterable?.Filter(nameFilter);

        protected override void SubscribeOnly() => _inputField.SafeSubscribe(Filter);
        protected override void UnsubscribeOnly() => _inputField.UnSubscribe(Filter);
    }
}