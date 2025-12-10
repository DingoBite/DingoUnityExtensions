using DingoUnityExtensions.MonoBehaviours.UI.UIGraph;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using DingoUnityExtensions.UnityViewProviders.PointerHandlerWrappers;

namespace DingoUnityExtensions.MonoBehaviours.UI.Interactions
{
    public enum SelectionOrigin
    {
        None,
        Word,
        Paragraph,
        All,
        Drag
    }

    public class SelectableTextHandler : SubscribableBehaviour
    {
        [SerializeField] private TMP_Text _text;
        [SerializeField] private UIGraphic _overlayGraphics;
        [SerializeField] private PointerHandlerClickDrag _pointerHandlerClick;
        [SerializeField] private Camera _camera;

        [SerializeField] private float _maxClickMovePixels = 6f;

        private int _chainWordStart = -1;
        private int _chainWordEnd = -1;
        private bool _chainWordValid = false;
        
        private int _selectionStart = -1;
        private int _selectionEnd = -1;
        private string _baseText = "";
        private bool _hasSelection = false;

        private int _chainCharIndex = -1;
        private int _clickChainCount = 0;

        private int _doubleWordStart = -1;
        private int _doubleWordEnd = -1;

        private Vector2 _lastPointerDownPos;
        private bool _hasLastPointerDownPos = false;

        private SelectionOrigin _selectionOrigin = SelectionOrigin.None;

        public void SetCamera(Camera c) => _camera = c;

        public void OnPointerDown(PointerEventData eventData, float time)
        {
            if (_text == null || _overlayGraphics == null || eventData == null)
                return;

            _baseText = _text.text ?? "";
            if (string.IsNullOrEmpty(_baseText))
            {
                ClearSelection();
                ResetClickChain();
                _hasLastPointerDownPos = false;
                return;
            }

            _text.ForceMeshUpdate();

            var pos = eventData.position;
            var charIndex = TMP_TextUtilities.FindNearestCharacter(_text, pos, _camera, true);

            var stationary = IsPointerStationary(pos);

            if (!stationary || charIndex < 0)
            {
                StartNewChain(charIndex, pos);
                return;
            }

            if (_clickChainCount == 1)
            {
                if (_chainWordValid && charIndex >= _chainWordStart && charIndex <= _chainWordEnd)
                {
                    if (TryGetWordRangeByCharIndex(charIndex, out var ws, out var we))
                    {
                        _doubleWordStart = ws;
                        _doubleWordEnd = we;

                        _clickChainCount = 2;

                        _lastPointerDownPos = pos;
                        _hasLastPointerDownPos = true;

                        SelectWordRange(ws, we);
                        return;
                    }

                    ClearSelection();
                    StartNewChain(charIndex, pos);
                    return;
                }

                StartNewChain(charIndex, pos);
                return;
            }

            if (_clickChainCount == 2)
            {
                if (_doubleWordStart >= 0 && _doubleWordEnd >= 0 && charIndex >= _doubleWordStart && charIndex <= _doubleWordEnd)
                {
                    SelectParagraphByCharIndex(charIndex);

                    _clickChainCount = 3;
                    _chainCharIndex = charIndex;

                    _doubleWordStart = -1;
                    _doubleWordEnd = -1;

                    _lastPointerDownPos = pos;
                    _hasLastPointerDownPos = true;
                    return;
                }

                StartNewChain(charIndex, pos);
                return;
            }

            if (_clickChainCount == 3)
            {
                if (_hasSelection && _selectionStart >= 0 && _selectionEnd >= 0 && charIndex >= Mathf.Min(_selectionStart, _selectionEnd) && charIndex <= Mathf.Max(_selectionStart, _selectionEnd))
                {
                    SelectAll();

                    ResetClickChain();

                    _lastPointerDownPos = pos;
                    _hasLastPointerDownPos = true;
                    return;
                }

                StartNewChain(charIndex, pos);
                return;
            }

            StartNewChain(charIndex, pos);
        }

        private bool IsPointerStationary(Vector2 currentPos)
        {
            if (!_hasLastPointerDownPos)
                return true;

            var d = currentPos - _lastPointerDownPos;
            return d.sqrMagnitude <= _maxClickMovePixels * _maxClickMovePixels;
        }

        private void StartNewChain(int charIndex, Vector2 pos)
        {
            _clickChainCount = 1;
            _chainCharIndex = charIndex;

            _doubleWordStart = -1;
            _doubleWordEnd = -1;

            if (charIndex >= 0 && TryGetWordRangeByCharIndex(charIndex, out var ws, out var we))
            {
                _chainWordStart = ws;
                _chainWordEnd = we;
                _chainWordValid = true;
            }
            else
            {
                _chainWordStart = -1;
                _chainWordEnd = -1;
                _chainWordValid = false;
            }

            _lastPointerDownPos = pos;
            _hasLastPointerDownPos = true;
        }

        private void ResetClickChain()
        {
            _clickChainCount = 0;
            _chainCharIndex = -1;

            _doubleWordStart = -1;
            _doubleWordEnd = -1;

            _chainWordStart = -1;
            _chainWordEnd = -1;
            _chainWordValid = false;
        }
        
        private bool TryGetWordRangeByCharIndex(int charIndex, out int start, out int end)
        {
            start = -1;
            end = -1;

            var info = _text.textInfo;
            if (info == null || info.wordCount <= 0)
                return false;

            for (int i = 0; i < info.wordCount; i++)
            {
                var w = info.wordInfo[i];
                if (charIndex >= w.firstCharacterIndex && charIndex <= w.lastCharacterIndex)
                {
                    start = w.firstCharacterIndex;
                    end = w.lastCharacterIndex;
                    return true;
                }
            }

            return false;
        }

        private void SelectWordRange(int start, int end)
        {
            if (_text == null || _overlayGraphics == null)
                return;

            _baseText = _text.text ?? "";
            if (string.IsNullOrEmpty(_baseText))
            {
                ClearSelection();
                return;
            }

            _selectionStart = start;
            _selectionEnd = end;
            _hasSelection = true;
            _selectionOrigin = SelectionOrigin.Word;

            UpdateHighlight();
        }

        private void SelectParagraphByCharIndex(int charIndex)
        {
            if (_text == null || _overlayGraphics == null)
                return;

            _baseText = _text.text ?? "";
            if (string.IsNullOrEmpty(_baseText))
            {
                ClearSelection();
                return;
            }

            charIndex = Mathf.Clamp(charIndex, 0, _baseText.Length - 1);

            var startNl = _baseText.LastIndexOf('\n', charIndex);
            var endNl = _baseText.IndexOf('\n', charIndex);

            var start = startNl < 0 ? 0 : startNl + 1;
            var end = endNl < 0 ? _baseText.Length - 1 : endNl - 1;

            if (start < _baseText.Length && _baseText[start] == '\r')
                start++;

            if (end >= 0 && end < _baseText.Length && _baseText[end] == '\r')
                end--;

            if (start > end)
            {
                ClearSelection();
                return;
            }

            _selectionStart = start;
            _selectionEnd = end;
            _hasSelection = true;
            _selectionOrigin = SelectionOrigin.Paragraph;

            UpdateHighlight();
        }

        public void OnStartDrag(PointerEventData data, float time)
        {
            if (_text == null || _overlayGraphics == null)
                return;

            _baseText = _text.text ?? "";

            var idx = TMP_TextUtilities.GetCursorIndexFromPosition(_text, data.position, _camera);

            if (_hasSelection && _selectionOrigin == SelectionOrigin.Word && _selectionStart >= 0 && _selectionEnd >= 0)
            {
                var min = Mathf.Min(_selectionStart, _selectionEnd);
                var max = Mathf.Max(_selectionStart, _selectionEnd);

                if (idx >= min && idx <= max)
                {
                    return;
                }
            }

            _selectionStart = idx;
            _selectionEnd = _selectionStart;
            _hasSelection = true;
            _selectionOrigin = SelectionOrigin.Drag;

            UpdateHighlight();
        }

        private void OnEndDrag(PointerEventData data, float time)
        {
            if (!_hasSelection || _text == null || _overlayGraphics == null)
                return;

            _selectionEnd = TMP_TextUtilities.GetCursorIndexFromPosition(_text, data.position, _camera);
            UpdateHighlight();
        }

        private void OnPointerDrag(PointerEventData data, float time)
        {
            if (!_hasSelection || _text == null || _overlayGraphics == null)
                return;

            _selectionEnd = TMP_TextUtilities.GetCursorIndexFromPosition(_text, data.position, _camera);
            UpdateHighlight();
        }

        private void SelectAll()
        {
            if (_text == null || _overlayGraphics == null)
                return;

            _baseText = _text.text ?? "";
            if (string.IsNullOrEmpty(_baseText))
            {
                ClearSelection();
                return;
            }

            _selectionStart = 0;
            _selectionEnd = _baseText.Length - 1;
            _hasSelection = true;
            _selectionOrigin = SelectionOrigin.All;

            UpdateHighlight();
        }

        private void ClearSelection()
        {
            _selectionStart = _selectionEnd = -1;
            _hasSelection = false;
            _selectionOrigin = SelectionOrigin.None;

            if (_overlayGraphics != null)
                _overlayGraphics.gameObject.SetActive(false);
        }

        private void UpdateHighlight()
        {
            if (_overlayGraphics == null || _text == null)
                return;

            if (!_hasSelection || _selectionStart < 0 || _selectionEnd < 0 || string.IsNullOrEmpty(_baseText))
            {
                _overlayGraphics.gameObject.SetActive(false);
                return;
            }

            var start = Mathf.Min(_selectionStart, _selectionEnd);
            var end = Mathf.Max(_selectionStart, _selectionEnd);

            if (start >= _baseText.Length)
            {
                _overlayGraphics.gameObject.SetActive(false);
                return;
            }

            end = Mathf.Min(end, _baseText.Length - 1);

            _overlayGraphics.Clear();

            var highlightColor = new Color(0.26f, 0.52f, 0.96f, 0.35f);
            var shapes = UIGraphSelectionBuilder.BuildTextSelectionShapes(_text, start, end, highlightColor);

            foreach (var shape in shapes)
            {
                if (!UIGraphShape.IsNull(in shape))
                    _overlayGraphics.AddGraphShape(shape);
            }

            _overlayGraphics.gameObject.SetActive(shapes.Count > 0);
        }

        private void Update()
        {
            if (_text == null)
                return;

            var ctrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

            if (ctrl && Input.GetKeyDown(KeyCode.A))
            {
                SelectAll();
                return;
            }

            if (_hasSelection && ctrl && Input.GetKeyDown(KeyCode.C))
            {
                if (_selectionStart < 0 || _selectionEnd < 0 || string.IsNullOrEmpty(_baseText))
                    return;

                var start = Mathf.Min(_selectionStart, _selectionEnd);
                var end = Mathf.Max(_selectionStart, _selectionEnd);

                if (start < 0 || start >= _baseText.Length)
                    return;

                end = Mathf.Min(end, _baseText.Length - 1);

                var selected = _baseText.Substring(start, end - start + 1);
                GUIUtility.systemCopyBuffer = selected;
                return;
            }

            if (_hasSelection && Input.anyKeyDown && !ctrl && !IsMouseButtonDownAny())
                ClearSelection();
        }

        private bool IsMouseButtonDownAny()
        {
            return Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2);
        }

        protected override void SubscribeOnly()
        {
            if (_pointerHandlerClick == null)
                return;

            _pointerHandlerClick.PointerDownEvent += OnPointerDown;
            _pointerHandlerClick.BeginDragEvent += OnStartDrag;
            _pointerHandlerClick.DragEvent += OnPointerDrag;
            _pointerHandlerClick.EndDragEvent += OnEndDrag;
        }

        protected override void UnsubscribeOnly()
        {
            if (_pointerHandlerClick == null)
                return;

            _pointerHandlerClick.PointerDownEvent -= OnPointerDown;
            _pointerHandlerClick.BeginDragEvent -= OnStartDrag;
            _pointerHandlerClick.DragEvent -= OnPointerDrag;
            _pointerHandlerClick.EndDragEvent -= OnEndDrag;
        }
    }
}