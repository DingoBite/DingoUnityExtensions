using System;
using System.Collections.Generic;
using System.Linq;
using DingoUnityExtensions.UnityViewProviders.Core;
using UnityEngine;

namespace DingoUnityExtensions.MonoBehaviours.UI.SearchDropdownLayoutBased
{
    public class SearchDropdownProvider : UnityViewProvider<SearchDropdown, int>
    {
        public event Action<SearchDropdownValue> SelectCustomValue;
        
        [SerializeField] private float _timeToMoveNext = 0.12f;
        [SerializeField] private float _delayToMove = 0.25f;
        
        private float _currentTimeToMoveNext;
        private int _dir;
        
        private bool _down;
        private bool _up;
        private bool _anyKeyDown;
        private bool _escapeDown;
        private bool _returnDown;
        private bool _anyMouseDown;

        protected override int NonInteractablePlaceholder => SearchDropdown.NONE_ID;

        public IReadOnlyList<SearchDropdownValue> InstantiateOptions<T>(IEnumerable<T> options) => InstantiateOptions(options.Select(o => o.ToString()));
        public IReadOnlyList<SearchDropdownValue> InstantiateOptions(IEnumerable<string> options)
        {
            var searchDropdownValues = View.InstantiateOptions(options);
            return searchDropdownValues;
        }
        
        public IReadOnlyList<SearchDropdownValue> InstantiateOptions(IEnumerable<(string, string)> options)
        {
            var searchDropdownValues = View.InstantiateOptions(options);
            return searchDropdownValues;
        }
        
        public void ProvideCloseButton(EventContainer eventContainer) => View.SetupCloseBackground(eventContainer);

        protected override void OnSetInteractable(bool value)
        {
            View.Interactable = value;
        }

        protected override void SetValueWithoutNotify(int value) => View.SelectValueWithoutNotify(value);

        private void OnSelect(SearchDropdownValue dropdownValue)
        {
            SetValueWithNotify(dropdownValue.Id);
            if (dropdownValue.Id < 0 && View.CustomValueSupport)
                SelectCustomValue?.Invoke(dropdownValue);
        }

        protected override void SubscribeOnly() => View.OnSelect += OnSelect;
        protected override void UnsubscribeOnly() => View.OnSelect -= OnSelect;

        private void Update()
        {
            if (!Interactable || !View.Opened)
                return;
            
            _down = Input.GetKey(KeyCode.DownArrow);
            _up = Input.GetKey(KeyCode.UpArrow);
            _escapeDown = Input.GetKeyDown(KeyCode.Escape);
            _returnDown = Input.GetKeyDown(KeyCode.Return);
            _anyKeyDown = Input.anyKeyDown;
            _anyMouseDown = Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButton(2);
        }

        private void LateUpdate()
        {
            if (!Interactable || !View.Opened)
                return;

            if (_returnDown)
            {
                View.Submit();
                return;
            }
            
            if (!_down && !_up)
            {
                if (_anyKeyDown)
                    View.InputSelectionActive(!(_returnDown || _escapeDown || _anyMouseDown));
                _dir = 0;
                return;
            }
            
            View.InputSelectionActive(false);

            if (_down && _up)
            {
                _currentTimeToMoveNext = _timeToMoveNext;
                _dir = 0;
                return;
            }

            var dir = 1;
            if (_down)
                dir = -1;

            if (_dir != dir)
            {
                _currentTimeToMoveNext = _delayToMove;
                _dir = dir;
                View.Hover(-_dir);
                return;
            }

            _currentTimeToMoveNext -= Time.deltaTime;
            if (_currentTimeToMoveNext > Vector2.kEpsilon)
                return;

            View.Hover(-_dir);
            _currentTimeToMoveNext = _timeToMoveNext;
        }
    }
}