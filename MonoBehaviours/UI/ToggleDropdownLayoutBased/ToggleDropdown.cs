using System;
using System.Collections.Generic;
using System.Linq;
using DingoUnityExtensions.Extensions;
using DingoUnityExtensions.MonoBehaviours.UI.SearchDropdownLayoutBased;
using DingoUnityExtensions.UnityViewProviders.Core;
using UnityEngine;
using UnityEngine.UI;

namespace DingoUnityExtensions.MonoBehaviours.UI.ToggleDropdownLayoutBased
{
    public class ToggleDropdown : SubscribableBehaviour
    {
        public event Action<IReadOnlyDictionary<SearchDropdownValue, bool>> SelectionChange;

        [SerializeField] private DropdownToggleContainerPool _dropdownToggleContainerPool;
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private LayoutRebuildProvider _layoutContent;
        [SerializeField] private ValueContainer<string> _selectionInfo;
        [SerializeField] private EventContainer _openDropdownButton;
        [SerializeField] private EventContainer _closeDropdownBackground;

        [SerializeField] private bool _interactable = true;
        [SerializeField] private int _overrideSortingOrder = 15;

        [SerializeField] private string _noneSelectPlaceholder = "None";
        [SerializeField] private string _allSelectPlaceholder = "All";
        [SerializeField] private string _multipleSelectPlaceholder = "Multiple";

        private readonly ValueContainerGroup<int, bool> _toggles = new();
        private readonly List<SearchDropdownValue> _values = new();
        private readonly Dictionary<SearchDropdownValue, bool> _cachedSelection = new();
        
        private float? _templateDefaultHeight;
        private int? _canvasOrder;
        private RectTransform _scrollRectRt;
        private Canvas _scrollRectCanvas;
        
        private RectTransform ScrollRectRt => _scrollRectRt ??= _scrollRect.GetComponent<RectTransform>();
        private Canvas ScrollRectCanvas => _scrollRectCanvas ??= _scrollRect.GetComponent<Canvas>();

        public IReadOnlyDictionary<SearchDropdownValue, bool> CachedSelection => _cachedSelection;
        
        private float TemplateDefaultHeight => _templateDefaultHeight ??= ScrollRectRt.rect.height + 1;
        public bool Opened { get; private set; }
        
        public bool Interactable
        {
            get => _interactable;
            set
            {
                _interactable = value;
                if (_openDropdownButton != null)
                    _openDropdownButton.Interactable = value;
                if (_scrollRect != null)
                    _scrollRect.enabled = value;
                
                UnsubscribeOnly();
                if (value && gameObject.activeInHierarchy)
                    SubscribeOnly();
            }
        }
        
        public void SetupCloseBackground(EventContainer eventContainer)
        {
            if (eventContainer == null)
                throw new NullReferenceException(nameof(eventContainer));
            _closeDropdownBackground = eventContainer;
        }

        public IReadOnlyList<SearchDropdownValue> InstantiateOptions(IEnumerable<(bool, string, string)> values)
        {
            ClearOptions();
            var i = -1;
            foreach (var (value, optionTitle, selectionTitle) in values)
            {
                i++;
                var dropdownValue = new SearchDropdownValue(i, optionTitle, selectionTitle);
                _values.Add(dropdownValue);
                var toggle = InstantiateToggle(dropdownValue, value);
            }

            var containers = _values.Select((v, index) => (v.Id, (ValueContainer<bool>)_dropdownToggleContainerPool.PulledElements[index]));
            _toggles.Initialize(containers);
            Interactable = i > 0;
            UpdateCachedSelection();
            return _values;
        }

        public IReadOnlyList<SearchDropdownValue> InstantiateOptions(IEnumerable<(bool, string)> values)
        {
            ClearOptions();
            var i = -1;
            foreach (var (value, optionTitle) in values)
            {
                i++;
                var dropdownValue = new SearchDropdownValue(i, optionTitle);
                _values.Add(dropdownValue);
                var toggle = InstantiateToggle(dropdownValue, value);
            }
            
            var containers = _values.Select((v, index) => (v.Id, (ValueContainer<bool>)_dropdownToggleContainerPool.PulledElements[index]));
            _toggles.Initialize(containers);
            Interactable = i > 0;
            UpdateCachedSelection();
            return _values;
        }

        public void ClearOptions()
        {
            UnsubscribeOnly();
            _dropdownToggleContainerPool.Clear();
            _values.Clear();
            _toggles.Clear();
        }

        private void Open()
        {
            if (!Interactable || Opened || Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Return) || _values.Count <= 1)
                return;
            
            if (ScrollRectCanvas != null)
            {
                _canvasOrder ??= ScrollRectCanvas.sortingOrder;
                ScrollRectCanvas.overrideSorting = true;
                ScrollRectCanvas.sortingOrder = _overrideSortingOrder;
            }
            
            if (_closeDropdownBackground != null)
            {
                _closeDropdownBackground.SafeSubscribe(Close);
                _closeDropdownBackground.gameObject.SetActive(true);
            }
            
            Opened = true;
            var anchoredPosition = _layoutContent.RectTransform.anchoredPosition;
            _layoutContent.RectTransform.anchoredPosition = new Vector2(anchoredPosition.x, 0);
            _layoutContent.gameObject.SetActive(true);
            _scrollRect.gameObject.SetActive(true);
            AlignHeight();
        }

        public void Close()
        {
            if (ScrollRectCanvas != null && _canvasOrder != null)
                ScrollRectCanvas.sortingOrder = _canvasOrder.Value;
            Opened = false;
            if (_closeDropdownBackground != null)
            {
                _closeDropdownBackground.OnEvent -= Close;
                _closeDropdownBackground.gameObject.SetActive(false);
            }

            _layoutContent.gameObject.SetActive(false);
            _scrollRect.gameObject.SetActive(false);
        }

        private void InvokeChanges(bool _)
        {
            UpdateCachedSelection();
            SelectionChange?.Invoke(_cachedSelection);
        }

        private void UpdateCachedSelection()
        {
            _cachedSelection.Clear();
            foreach (var value in _values)
            {
                _cachedSelection.Add(value, _toggles.GetContainer(value.Id)?.Value ?? false);
            }

            UpdateSelectionInfo();
        }

        private void UpdateSelectionInfo()
        {
            var selectionInfo = CollectSelectionInfoFromCachedSelection();
            _selectionInfo.UpdateValueWithoutNotify(selectionInfo);
        }

        private string CollectSelectionInfoFromCachedSelection()
        {
            var count = _cachedSelection.Count(v => v.Value);
            if (count == 0)
                return _noneSelectPlaceholder;
            if (count == _values.Count)
                return _allSelectPlaceholder;
            if (count > 1)
                return _multipleSelectPlaceholder;

            return _cachedSelection.First(v => v.Value).Key.SelectionName;
        }

        private ValueContainer<bool> InstantiateToggle(SearchDropdownValue dropdownValue, bool value)
        {
            var toggle = _dropdownToggleContainerPool.PullElement();
            toggle.UpdateValueWithoutNotify(value);
            if (gameObject.activeInHierarchy)
                toggle.SafeSubscribe(InvokeChanges);

            toggle.Title.UpdateValueWithoutNotify(dropdownValue.OptionName);
            toggle.transform.SetAsLastSibling();

            return toggle;
        }

        private void AlignHeight()
        {
            _layoutContent.RebuildOnNextFrame(() =>
            {
                var contentHeight = _layoutContent.RectTransform.rect.height + 1;
                var height = contentHeight >= TemplateDefaultHeight ? TemplateDefaultHeight : contentHeight;
                ScrollRectRt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
            });
        }
        
        protected override void SubscribeOnly()
        {
            _openDropdownButton.SafeSubscribe(Open);
            foreach (var toggle in _toggles.GetContainers())
            {
                toggle.SafeSubscribe(InvokeChanges);
            }
        }

        protected override void UnsubscribeOnly()
        {
            if (_closeDropdownBackground != null)
                _closeDropdownBackground.OnEvent -= Close;

            _openDropdownButton.UnSubscribe(Open);
            foreach (var toggle in _toggles.GetContainers())
            {
                toggle.UnSubscribe(InvokeChanges);
            }
        }
        
        public void EnableCloseBackground()
        {
            if (_closeDropdownBackground != null)
                _closeDropdownBackground.gameObject.SetActive(true);
        }
    }
}