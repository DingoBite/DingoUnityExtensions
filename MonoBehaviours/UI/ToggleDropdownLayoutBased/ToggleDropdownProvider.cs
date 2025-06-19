using System.Collections.Generic;
using DingoUnityExtensions.MonoBehaviours.UI.SearchDropdownLayoutBased;
using DingoUnityExtensions.UnityViewProviders.Core;
using UnityEngine;

namespace DingoUnityExtensions.MonoBehaviours.UI.ToggleDropdownLayoutBased
{
    public class ToggleDropdownProvider : UnityViewProvider<ToggleDropdown, IReadOnlyDictionary<SearchDropdownValue, bool>>
    {
        
        private bool _escapeDown;
        private bool _returnDown;

        protected override void SetValueWithoutNotify(IReadOnlyDictionary<SearchDropdownValue, bool> value) { }
        public void ProvideCloseButton(EventContainer eventContainer) => View.SetupCloseBackground(eventContainer);
        
        public IReadOnlyList<SearchDropdownValue> InstantiateOptions(IEnumerable<(bool, string)> options)
        {
            var searchDropdownValues = View.InstantiateOptions(options);
            UpdateValueWithoutNotify(View.CachedSelection);
            return searchDropdownValues;
        }
        
        public IReadOnlyList<SearchDropdownValue> InstantiateOptions(IEnumerable<(bool, string, string)> options)
        {
            var searchDropdownValues = View.InstantiateOptions(options);
            UpdateValueWithoutNotify(View.CachedSelection);
            return searchDropdownValues;
        }
        
        protected override void OnSetInteractable(bool value)
        {
            View.Interactable = value;
        }

        private void SelectionChange(IReadOnlyDictionary<SearchDropdownValue, bool> readOnlyDictionary)
        {
            SetValueWithNotify(readOnlyDictionary);
        }
        
        private void Update()
        {
            if (!Interactable || !View.Opened)
                return;
            
            if (View.Opened)
                View.EnableCloseBackground();
            
            _escapeDown = Input.GetKeyDown(KeyCode.Escape);
            _returnDown = Input.GetKeyDown(KeyCode.Return);
        }

        private void LateUpdate()
        {
            if (!Interactable || !View.Opened)
                return;

            if (_escapeDown || _returnDown)
                View.Close();
        }

        protected override void SubscribeOnly() => View.SelectionChange += SelectionChange;
        protected override void UnsubscribeOnly() => View.SelectionChange -= SelectionChange;
    }
}