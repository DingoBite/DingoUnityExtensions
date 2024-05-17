using System;
using System.Collections.Generic;
using System.Linq;
using DingoUnityExtensions.Extensions;
using DingoUnityExtensions.UnityViewProviders;
using DingoUnityExtensions.UnityViewProviders.Core;
using DingoUnityExtensions.UnityViewProviders.Toggle.Core;
using DingoUnityExtensions.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DingoUnityExtensions.MonoBehaviours.UI.SearchDropdownLayoutBased
{
    public class SearchDropdown : SubscribableBehaviour
    {
        public const int NONE_ID = int.MinValue;
        
        public event Action<SearchDropdownValue> OnSelect;

        public delegate float SearchFunc(string input, string option);

        [SerializeField] private SelectableButton _buttonPrefab;
        [SerializeField] private TMP_InputField _inputField;
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private LayoutRebuildProvider _layoutContent;
        [SerializeField] private bool _invertSearch;
        [SerializeField] private EventContainer _closeDropdownBackground;
        [SerializeField] private bool _interactable;
        [SerializeField] private int _overrideSortingOrder;
        [SerializeField] private string _nonePlaceholder = "None";
        [SerializeField] private bool _customValueSupport;
        
        private float? _templateDefaultHeight;
        private string _startInput;

        private readonly EventContainerGroup<int, SelectableButton> _buttons = new();
        private readonly List<SearchDropdownValue> _values = new();
        private readonly Dictionary<int, Action> _buttonCallbacks = new();
        private readonly List<(int id, float searchValue)> _cachedSearchList = new();

        private int _preOpenSelectedId;
        private int _selectedId;
        private int _currentSelectedId;
        private RectTransform _scrollRectRt;
        private Canvas _scrollRectCanvas;
        private int? _canvasOrder;

        public bool CustomValueSupport => _customValueSupport;
        
        private int CurrentSelectedId
        {
            get => _currentSelectedId;
            set
            {
                var button = _buttons.GetButton(_currentSelectedId);
                _currentSelectedId = value;
                if (button == null)
                    return;
                button.Deselect();
                button = _buttons.GetButton(_currentSelectedId);
                button.Select();
            }
        }

        public bool Interactable
        {
            get => _interactable;
            set
            {
                _interactable = value;
                if (_inputField != null)
                    _inputField.interactable = value;
                if (_scrollRect != null)
                    _scrollRect.enabled = value;

                if (!value)
                    SelectValueWithoutNotify(NONE_ID);
                
                UnsubscribeOnly();
                if (value && gameObject.activeInHierarchy)
                    SubscribeOnly();
                else
                    UnselectInputField();
                
                CloseSearchActive();
            }
        }

        private RectTransform ScrollRectRt => _scrollRectRt ??= _scrollRect.GetComponent<RectTransform>();
        private Canvas ScrollRectCanvas => _scrollRectCanvas ??= _scrollRect.GetComponent<Canvas>();
        private float TemplateDefaultHeight => _templateDefaultHeight ??= ScrollRectRt.rect.height + 1;
        public bool Opened { get; private set; }
        public bool Selected { get; private set; }
        
        public IReadOnlyList<SearchDropdownValue> InstantiateOptions(IEnumerable<(string, string)> names)
        {
            ClearOptions();
            var buttons = new Dictionary<int, SelectableButton>();
            var i = -1;
            foreach (var n in names)
            {
                i++;
                var dropdownValue = new SearchDropdownValue(i, n.Item1, n.Item2);
                _values.Add(dropdownValue);
                var button = InstantiateButton(dropdownValue);
                buttons[i] = button;
            }

            Interactable = i > 0;
            if (i == -1)
            {
                SetInputTextWithoutNotify(new SearchDropdownValue(-1, _nonePlaceholder));
                return _values;
            }

            CurrentSelectedId = _values[0].Id;

            SetInputTextWithoutNotify(_values[0]);

            _buttons.Initialize(buttons);
            return _values;
        }
        
        public IReadOnlyList<SearchDropdownValue> InstantiateOptions(IEnumerable<string> names)
        {
            ClearOptions();
            var buttons = new Dictionary<int, SelectableButton>();
            var i = -1;
            foreach (var n in names)
            {
                i++;
                var dropdownValue = new SearchDropdownValue(i, n);
                _values.Add(dropdownValue);
                var button = InstantiateButton(dropdownValue);
                buttons[i] = button;
            }

            Interactable = i > 0;
            if (i == -1)
            {
                SetInputTextWithoutNotify(new SearchDropdownValue(-1, _nonePlaceholder));
                return _values;
            }

            CurrentSelectedId = _values[0].Id;

            SetInputTextWithoutNotify(_values[0]);

            _buttons.Initialize(buttons);
            return _values;
        }

        public void SelectTopSearchValue()
        {
            var value = GetTopSearchValue();
            var id = SearchDropdownValue.IsNull(value) ? _preOpenSelectedId : value.Id;
            SelectValue(id);
        }

        public SearchDropdownValue GetSelectedValue()
        {
            if (_selectedId == -1)
                return SearchDropdownValue.Null;
            var index = GetIndex(_selectedId);
            return _values[index];
        }

        public void Submit()
        {
            if (_customValueSupport && _selectedId < 0)
                SelectCustomValue();
            else
                SelectValue(CurrentSelectedId);
        }
        
        public void SelectValueWithoutNotify(int id)
        {
            var index = GetIndex(id);
            if (index < 0)
            {
                CloseSearch();
                if (index == NONE_ID)
                    SetInputTextWithoutNotify(new SearchDropdownValue(-1, _nonePlaceholder));
                return;
            }
            if (_selectedId != id)
            {
                _selectedId = id;
                _preOpenSelectedId = _selectedId;
                var dropdownValue = _values[index];
                SetInputTextWithoutNotify(dropdownValue);
            }
            CloseSearchActive();
        }

        private void SelectCustomValue()
        {
            OnSelect?.Invoke(new SearchDropdownValue(-1, _inputField.text));
            CloseSearchActive();
        }

        private void SelectValue(int id)
        {
            SelectValueWithoutNotify(id);
            var index = GetIndex(id);
            if (index < 0)
                return;
            OnSelect?.Invoke(_values[index]);
        }
        
        private void SearchByName(string value) => SearchByName(value, DefaultSearchFunc);

        private void SearchByName(string value, SearchFunc searchFunc)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                ResetSearch();
                return;
            }

            _cachedSearchList.Clear();
            foreach (var dropdownValue in _values)
            {
                var searchValue = Math.Max(searchFunc(value, dropdownValue.OptionName), searchFunc(value, dropdownValue.SelectionName));
                _cachedSearchList.Add((dropdownValue.Id, searchValue));
            }

            _cachedSearchList.Sort((p1, p2) => p2.searchValue.CompareTo(p1.searchValue));
            var hoveredButton = false;
            foreach (var (id, searchValue) in _cachedSearchList)
            {
                var button = _buttons.GetButton(id);
                var isZero = searchValue < Vector2.kEpsilon;
                button.gameObject.SetActive(!isZero);
                if (isZero)
                {
                    hoveredButton = true;
                    continue;
                }

                if (_invertSearch)
                    button.transform.SetAsFirstSibling();
                else
                    button.transform.SetAsLastSibling();
            }
            // if (hoveredButton)
            // HoverPrev();

            var firstZeroIndex = _cachedSearchList.FindIndex(p => p.searchValue < Vector2.kEpsilon);
            if (firstZeroIndex >= 0)
                _cachedSearchList.RemoveRange(firstZeroIndex, _cachedSearchList.Count - firstZeroIndex);

            var topId = GetTopSearchId();
            if (topId != null)
            {
                _selectedId = topId.Value;
                CurrentSelectedId = _selectedId;
            }
            else
            {
                _buttons.GetButton(CurrentSelectedId).Deselect();
                _selectedId = -1;
                CurrentSelectedId = _values[0].Id;
            }

            AlignHeight();
        }

        public void OpenSearch()
        {
            if (!Interactable || Opened || Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Return) || _values.Count <= 1)
            {
                UnselectInputField();
                return;
            }

            if (ScrollRectCanvas != null)
            {
                _canvasOrder ??= ScrollRectCanvas.sortingOrder;
                ScrollRectCanvas.overrideSorting = true;
                ScrollRectCanvas.sortingOrder = _overrideSortingOrder;
            }
            
            if (_closeDropdownBackground != null)
            {
                _closeDropdownBackground.SafeSubscribe(CloseSearch);
                _closeDropdownBackground.gameObject.SetActive(true);
            }
            _startInput = _inputField.text;

            Opened = true;
            var anchoredPosition = _layoutContent.RectTransform.anchoredPosition;
            _layoutContent.RectTransform.anchoredPosition = new Vector2(anchoredPosition.x, 0);
            _layoutContent.gameObject.SetActive(true);
            ResetSearch();
            _scrollRect.gameObject.SetActive(true);
        }

        public void CloseSearch()
        {
            if (_values.Count == 0)
                return;

            if (_preOpenSelectedId >= 0)
                SelectValueWithoutNotify(_preOpenSelectedId);

            if (ScrollRectCanvas != null && _canvasOrder != null)
                ScrollRectCanvas.sortingOrder = _canvasOrder.Value;
            
            CloseSearchActive();
        }

        private void ResetSearch()
        {
            _cachedSearchList.Clear();
            foreach (var dropdownValue in _values)
            {
                var button = _buttons.GetButton(dropdownValue.Id);
                button.gameObject.SetActive(true);
                if (_invertSearch)
                    button.transform.SetAsFirstSibling();
                else
                    button.transform.SetAsLastSibling();
            }

            _cachedSearchList.AddRange(_values.Select(v => (v.Id, 1f)));
            foreach (var button in _buttons.GetButtons())
            {
                button.Deselect();
            }

            CurrentSelectedId = _values[0].Id;

            AlignHeight();
        }

        public void ClearOptions()
        {
            _selectedId = -1;
            _preOpenSelectedId = 0;
            _values.Clear();
            foreach (var button in _buttons.GetButtons())
            {
                Destroy(button.gameObject);
            }

            _buttons.Clear();
            _cachedSearchList.Clear();
            _buttonCallbacks.Clear();
            _layoutContent.gameObject.SetActive(false);
        }

        private void CloseSearchActive()
        {
            Opened = false;
            UnselectInputField();

            if (_closeDropdownBackground != null)
            {
                _closeDropdownBackground.OnEvent -= CloseSearch;
                _closeDropdownBackground.gameObject.SetActive(false);
            }

            _cachedSearchList.Clear();
            _layoutContent.gameObject.SetActive(false);
            _scrollRect.gameObject.SetActive(false);
        }
        
        public void SetupCloseBackground(EventContainer eventContainer)
        {
            if (eventContainer == null)
                throw new NullReferenceException(nameof(eventContainer));
            _closeDropdownBackground = eventContainer;
        }

        public void HoverNext() => Hover(+1);
        public void HoverPrev() => Hover(-1);

        public void Hover(int sign)
        {
            if (!Interactable)
                return;
            
            sign = Math.Sign(sign);
            var id = GetHoveredId(CurrentSelectedId, sign);
            var count = _cachedSearchList.Count;
            var button = _buttons.GetButton(id);
            var i = -1;
            while (!button.gameObject.activeSelf && i < count)
            {
                i++;
                if (i >= count)
                {
                    CurrentSelectedId = _cachedSearchList[0].id;
                    return;
                }

                id = GetHoveredId(CurrentSelectedId, sign);
                button = _buttons.GetButton(id);
            }

            button.Select();
            CurrentSelectedId = id;
            MoveContentToHovered(button.RectTransform);
        }

        public void InputSelectionActive(bool value)
        {
            if (value)
                SelectInputField();
            else
                UnselectInputField();
        }
        
        private void MoveContentToHovered(RectTransform item)
        {
            var scrollRt = ScrollRectRt;
            var rect = scrollRt.rect;
            if (rect.height < TemplateDefaultHeight)
                return;
        
            MoveContentToHovered(scrollRt, _layoutContent.RectTransform, item);
        }
        
        private void MoveContentToHovered(RectTransform parent, RectTransform content, RectTransform item)
        {
            var parentRect = parent.rect;
            var parentPivot = parent.pivot;
        
            var itemRect = item.rect;
            var itemPivot = item.pivot;
            var itemPos = item.anchoredPosition;
            var itemHeight = itemRect.height;
        
            var contentRect = content.rect;
            var contentPivot = content.pivot;
            var contentPos = content.anchoredPosition;

            var itemYBottom = -itemPos.y + itemPivot.y * itemHeight;
            var itemYTop = -itemPos.y + (1 - itemPivot.y) * itemHeight;

            var defaultHeight = TemplateDefaultHeight;

            var downBorder = itemYBottom - contentPos.y;
            var topBorder = itemYTop - contentPos.y;


            if (downBorder > defaultHeight + itemHeight)
                contentPos.y = itemYBottom - defaultHeight;
            else if (topBorder < -itemHeight) // Not overlaps
                contentPos.y = itemYTop;
            else if (topBorder <= 0) // Not Full visible: top viewport
                contentPos.y = itemYTop;
            else if (downBorder > defaultHeight) // Not Full visible: down viewport
                contentPos.y = itemYBottom - defaultHeight;
            else // Full visible 
                return;
            content.anchoredPosition = contentPos;
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

        private SearchDropdownValue GetTopSearchValue()
        {
            var id = GetTopSearchId();
            if (id == null)
                return SearchDropdownValue.Null;
            return _values[id.Value];
        }

        private int? GetTopSearchId()
        {
            if (_cachedSearchList.Count == 0)
                return null;

            var (index, searchValue) = _invertSearch ? _cachedSearchList[^1] : _cachedSearchList[0];
            if (searchValue < float.Epsilon)
                return null;
            return index;
        }

        private static float DefaultSearchFunc(string input, string option)
        {
            if (string.IsNullOrWhiteSpace(input) || string.IsNullOrWhiteSpace(option))
                return 1;

            return (float)FuzzyMatcher.SubstringSimilarity(input, option);
        }

        private int GetIndex(int id)
        {
            if (id == NONE_ID)
                return NONE_ID;
            
            return _values.FindIndex(v => v.Id == id);
        }

        private int GetHoveredId(int id, int sign)
        {
            var index = _cachedSearchList.FindIndex(v => v.id == id);
            index = (index + sign + _cachedSearchList.Count) % _cachedSearchList.Count;
            return _cachedSearchList[index].id;
        }

        private SelectableButton InstantiateButton(SearchDropdownValue dropdownValue)
        {
            var button = Instantiate(_buttonPrefab, _layoutContent.transform);
            if (!_buttonCallbacks.TryGetValue(dropdownValue.Id, out var callback))
            {
                callback = () => SelectValue(dropdownValue.Id);
                _buttonCallbacks.Add(dropdownValue.Id, callback);
            }
            if (gameObject.activeInHierarchy)
                button.SafeSubscribe(callback);
            button.name = $"--{dropdownValue.OptionName}_{dropdownValue.Id}";
            button.SetTitle(dropdownValue.OptionName);
            return button;
        }

        private void UnselectInputField()
        {
            if (!Selected)
                return;

            Selected = false;
            _inputField.DeactivateInputField(true);
            _inputField.ReleaseSelection();
            var eventSystem = EventSystem.current;
            if (eventSystem == null)
                return;
            
            var selectedGameObject = eventSystem.currentSelectedGameObject;
            if (selectedGameObject == _inputField.gameObject)
                eventSystem.SetSelectedGameObject(null);
        }

        private void SelectInputField()
        {
            if (Selected)
                return;
            
            Selected = true;
            _inputField.ActivateInputField();
            var eventSystem = EventSystem.current;
            if (eventSystem == null)
                return;
            
            var selectedGameObject = eventSystem.currentSelectedGameObject;
            if (selectedGameObject != _inputField.gameObject)
                eventSystem.SetSelectedGameObject(_inputField.gameObject);
        }


        protected override void SubscribeOnly()
        {
            SubscribeInputField();
            foreach (var (id, callback) in _buttonCallbacks)
            {
                var button = _buttons.GetButton(id);
                if (button != null)
                    button.SafeSubscribe(callback);
            }
        }

        protected override void UnsubscribeOnly()
        {
            if (_closeDropdownBackground != null)
                _closeDropdownBackground.OnEvent -= CloseSearch;
            UnsubscribeInputField();
            foreach (var (id, callback) in _buttonCallbacks)
            {
                var button = _buttons.GetButton(id);
                if (button != null)
                    button.OnEvent -= callback;
            }
        }

        private void InputHandle(string value)
        {
            if (_values.Count == 0)
                return;
            
            if (_startInput == null)
            {
                SearchByName(value);
                return;
            }

            var appendedSubstring = _startInput.FindFirstDifferentCharacter(value);
            SetInputTextWithoutNotify(new SearchDropdownValue(-1, appendedSubstring));
            _startInput = null;
            SearchByName(appendedSubstring);
        }

        private void SetInputTextWithoutNotify(SearchDropdownValue dropdownValue)
        {
            _inputField.SetTextWithoutNotify(dropdownValue.SelectionName);
        }
        
        private void SubscribeInputField()
        {
            if (_inputField != null)
            {
                _inputField.onValueChanged.AddListener(InputHandle);
                _inputField.onSelect.AddListener(OpenSearch);
            }
        }

        private void OpenSearch(string _) => OpenSearch();
        private void CloseSearch(string _) => CloseSearch();

        private void UnsubscribeInputField()
        {
            if (_inputField != null)
            {
                _inputField.onValueChanged.RemoveListener(InputHandle);
                _inputField.onSelect.RemoveListener(OpenSearch);
            }
        }
    }
}