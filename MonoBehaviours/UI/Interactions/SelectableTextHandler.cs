using System.Text;
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
        [SerializeField] private UIShapes _overlayShapeses;
        [SerializeField] private PointerHandlerClickDrag _pointerHandlerClick;
        [SerializeField] private Camera _camera;

        [SerializeField] private float _maxClickMovePixels = 6f;
        [SerializeField] private Color _selectionColor = new(0.26f, 0.52f, 0.96f, 0.35f);

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

        private bool _dragFromWordSelection = false;
        private int _dragWordStartCharIndex = -1;
        private int _dragWordEndCharIndex = -1;
        private int _dragWordInitialCharIndex = -1;

        public void SetCamera(Camera c) => _camera = c;

        public void OnPointerDown(PointerEventData eventData, float time)
        {
            if (_text == null || _overlayShapeses == null || eventData == null)
                return;

            _baseText = _text.text ?? "";
            if (string.IsNullOrEmpty(_baseText))
            {
                ClearSelection();
                ResetClickChain();
                _hasLastPointerDownPos = false;
                return;
            }

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
            if (_hasSelection)
                ClearSelection();

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

            _dragFromWordSelection = false;

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

            _dragFromWordSelection = false;
            _dragWordStartCharIndex = -1;
            _dragWordEndCharIndex = -1;
            _dragWordInitialCharIndex = -1;
        }

        private bool TryGetWordRangeByCharIndex(int charIndex, out int start, out int end)
        {
            start = -1;
            end = -1;

            var info = _text.textInfo;
            if (info == null || info.wordCount <= 0)
                return false;

            for (var i = 0; i < info.wordCount; i++)
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
            if (_text == null || _overlayShapeses == null)
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
            if (_text == null || _overlayShapeses == null)
                return;

            var info = _text.textInfo;
            if (info == null || info.characterCount == 0)
            {
                ClearSelection();
                return;
            }

            charIndex = Mathf.Clamp(charIndex, 0, info.characterCount - 1);

            var line = info.characterInfo[charIndex].lineNumber;
            if (line < 0 || line >= info.lineCount)
            {
                ClearSelection();
                return;
            }

            var lineInfo = info.lineInfo[line];

            _selectionStart = lineInfo.firstCharacterIndex;
            _selectionEnd = lineInfo.lastCharacterIndex;
            _hasSelection = true;
            _selectionOrigin = SelectionOrigin.Paragraph;

            UpdateHighlight();
        }

        private int GetCharIndexFromPositionLineAware(Vector2 position)
        {
            if (_text == null)
                return -1;

            return TMP_TextUtilities.FindNearestCharacter(_text, position, _camera, true);
        }

        public void OnStartDrag(PointerEventData data, float time)
        {
            if (_text == null || _overlayShapeses == null)
                return;

            _baseText = _text.text ?? "";

            var idx = GetCharIndexFromPositionLineAware(data.position);

            if (idx < 0)
            {
                ClearSelection();
                return;
            }

            if (_hasSelection && _selectionOrigin == SelectionOrigin.Word && _selectionStart >= 0 && _selectionEnd >= 0)
            {
                var min = Mathf.Min(_selectionStart, _selectionEnd);
                var max = Mathf.Max(_selectionStart, _selectionEnd);

                if (idx >= min && idx <= max)
                {
                    _dragFromWordSelection = true;
                    _dragWordStartCharIndex = min;
                    _dragWordEndCharIndex = max;
                    _dragWordInitialCharIndex = idx;
                    return;
                }
            }

            _dragFromWordSelection = false;

            _selectionStart = idx;
            _selectionEnd = _selectionStart;
            _hasSelection = true;
            _selectionOrigin = SelectionOrigin.Drag;

            UpdateHighlight();
        }

        private void OnEndDrag(PointerEventData data, float time)
        {
            if (_text == null || _overlayShapeses == null)
                return;

            var idx = GetCharIndexFromPositionLineAware(data.position);
            if (idx < 0)
            {
                _dragFromWordSelection = false;
                return;
            }

            if (_dragFromWordSelection)
            {
                ApplyDragFromWord(idx);
                _selectionOrigin = SelectionOrigin.Drag;
                _dragFromWordSelection = false;
                UpdateHighlight();
                return;
            }

            if (!_hasSelection)
                return;

            _selectionEnd = idx;
            UpdateHighlight();
        }

        private void OnPointerDrag(PointerEventData data, float time)
        {
            if (_text == null || _overlayShapeses == null)
                return;

            var idx = GetCharIndexFromPositionLineAware(data.position);
            if (idx < 0)
                return;

            if (_dragFromWordSelection)
            {
                ApplyDragFromWord(idx);
                UpdateHighlight();
                return;
            }

            if (!_hasSelection)
                return;

            _selectionEnd = idx;
            UpdateHighlight();
        }

        private void ApplyDragFromWord(int currentChar)
        {
            if (!_hasSelection || _dragWordStartCharIndex < 0 || _dragWordEndCharIndex < 0 || _dragWordInitialCharIndex < 0)
                return;

            if (currentChar >= _dragWordInitialCharIndex)
            {
                _selectionStart = _dragWordStartCharIndex;
                _selectionEnd = Mathf.Max(currentChar, _dragWordEndCharIndex);
            }
            else
            {
                _selectionStart = Mathf.Min(currentChar, _dragWordStartCharIndex);
                _selectionEnd = _dragWordEndCharIndex;
            }
        }

        private void SelectAll()
        {
            if (_text == null || _overlayShapeses == null)
                return;

            var info = _text.textInfo;
            if (info == null || info.characterCount == 0)
            {
                ClearSelection();
                return;
            }

            _selectionStart = 0;
            _selectionEnd = info.characterCount - 1;
            _hasSelection = true;
            _selectionOrigin = SelectionOrigin.All;

            UpdateHighlight();
        }

        private void ClearSelection()
        {
            _selectionStart = _selectionEnd = -1;
            _hasSelection = false;
            _selectionOrigin = SelectionOrigin.None;

            _dragFromWordSelection = false;
            _dragWordStartCharIndex = -1;
            _dragWordEndCharIndex = -1;
            _dragWordInitialCharIndex = -1;

            if (_overlayShapeses != null)
                _overlayShapeses.gameObject.SetActive(false);
        }

        private void UpdateHighlight()
        {
            if (_overlayShapeses == null || _text == null)
                return;

            if (!_hasSelection)
            {
                _overlayShapeses.gameObject.SetActive(false);
                return;
            }

            var info = _text.textInfo;
            if (info == null || info.characterCount == 0)
            {
                _overlayShapeses.gameObject.SetActive(false);
                return;
            }

            var start = Mathf.Min(_selectionStart, _selectionEnd);
            var end = Mathf.Max(_selectionStart, _selectionEnd);

            if (start < 0 || start >= info.characterCount)
            {
                _overlayShapeses.gameObject.SetActive(false);
                return;
            }

            end = Mathf.Min(end, info.characterCount - 1);

            _overlayShapeses.ClearShapes();

            var shapes = UIGraphSelectionBuilder.BuildTextSelectionShapes(_text, start, end, _selectionColor);

            foreach (var shape in shapes)
            {
                if (!UIGraphShape.IsNull(in shape))
                    _overlayShapeses.AddGraphShape(shape);
            }

            _overlayShapeses.gameObject.SetActive(shapes.Count > 0);
        }

        private void Update()
        {
            if (_text == null)
                return;

            var ctrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

            if (ctrl && Input.GetKeyDown(KeyCode.A))
            {
                if (_hasSelection)
                    SelectAll();
                else 
                    ClearSelection();
                return;
            }

            if (_hasSelection && ctrl && Input.GetKeyDown(KeyCode.C))
            {
                if (_selectionStart < 0 || _selectionEnd < 0)
                    return;

                var info = _text.textInfo;
                if (info == null || info.characterCount == 0)
                    return;

                var start = Mathf.Min(_selectionStart, _selectionEnd);
                var end = Mathf.Max(_selectionStart, _selectionEnd);

                if (start < 0 || start >= info.characterCount)
                    return;

                end = Mathf.Min(end, info.characterCount - 1);

                var sb = new StringBuilder();

                for (var i = start; i <= end; i++)
                {
                    var chInfo = info.characterInfo[i];
                    sb.Append(chInfo.character);
                }

                GUIUtility.systemCopyBuffer = sb.ToString();
                return;
            }

            if (_hasSelection && IsMouseButtonDownAny())
            {
                var overText = _pointerHandlerClick != null && _pointerHandlerClick.Entered;

                if (!overText)
                {
                    ClearSelection();
                    return;
                }
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