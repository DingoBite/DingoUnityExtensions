using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using DingoUnityExtensions.UnityViewProviders.Core;
using DingoUnityExtensions.UnityViewProviders.PointerHandlerWrappers;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace DingoUnityExtensions.MonoBehaviours.UI.Interactions
{
    public static class TextLinkHelpers
    {
        public static string CreateLink(string linkId, string linkTitle)
        {
            return $"<link=\"{linkId}\">{linkTitle}</link>";
        }
    }
    
    public class ClickLinkHandler : ValueContainer<string>
    {
        [SerializeField] private TMP_Text _text;
        [SerializeField] private PointerHandlerClickDrag _pointerHandlerClick;
        [SerializeField] private Camera _camera;

        [Header("Hover settings")]
        [SerializeField] private Color32 _hoverColor = new(0, 170, 255, 255);
        [SerializeField] private SerializedDictionary<string, Color> _linkColors;
        
        private int _lastLinkIndex = -1;

        public IReadOnlyList<TMP_LinkInfo> Links => _text.textInfo.linkInfo;

        public void SetCamera(Camera c) => _camera = c;

        protected override void OnSetInteractable(bool value) => _pointerHandlerClick.enabled = value;

        public void OnPointerClick(PointerEventData eventData, float time)
        {
            if (_text == null)
                return;

            if (_lastLinkIndex == -1)
                return;

            var info = _text.textInfo;
            if (info == null || _lastLinkIndex < 0 || _lastLinkIndex >= info.linkCount)
                return;

            var linkInfo = info.linkInfo[_lastLinkIndex];
            var url = linkInfo.GetLinkID();

            if (!string.IsNullOrEmpty(url))
            {
                ValueChangeInvoke(url);
                UrlClickedHandle(eventData, linkInfo, url);
            }

            var color = _linkColors.GetValueOrDefault(url, _hoverColor);
            SetLinkColor(_lastLinkIndex, color);
        }

        protected virtual void UrlClickedHandle(PointerEventData eventData, TMP_LinkInfo linkInfo, string url)
        {
        }

        private void OnPointerMove(PointerEventData data, float time)
        {
            if (_text == null)
                return;

            var info = _text.textInfo;
            if (info == null || info.linkCount == 0)
            {
                ClearHover();
                return;
            }

            var linkIndex = TMP_TextUtilities.FindIntersectingLink(_text, data.position, _camera);

            if (linkIndex == _lastLinkIndex)
                return;

            if (_lastLinkIndex != -1)
                RestoreColors();

            if (linkIndex != -1)
                CoroutineParent.AddLateUpdater(this, () => SetLinkColor(linkIndex, _hoverColor));

            _lastLinkIndex = linkIndex;
        }

        private void ClearHover()
        {
            if (_lastLinkIndex != -1 && _text != null)
            {
                RestoreColors();
                _lastLinkIndex = -1;
            }
        }

        private void RestoreColors()
        {
            if (_text == null)
                return;

            CoroutineParent.RemoveLateUpdater(this);
            _text.ForceMeshUpdate();
            _text.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
        }

        private void SetLinkColor(int linkIndex, Color32 color)
        {
            if (_text == null)
                return;
            var info = _text.textInfo;
            if (info == null || linkIndex < 0 || linkIndex >= info.linkCount)
                return;

            var linkInfo = info.linkInfo[linkIndex];

            var firstCharIndex = linkInfo.linkTextfirstCharacterIndex;
            var length = linkInfo.linkTextLength;

            var charInfos = info.characterInfo;
            var meshInfos = info.meshInfo;

            for (var i = 0; i < length; i++)
            {
                var charIndex = firstCharIndex + i;
                if (charIndex < 0 || charIndex >= info.characterCount)
                    continue;

                var charInfo = charInfos[charIndex];
                if (!charInfo.isVisible)
                    continue;

                var meshIndex = charInfo.materialReferenceIndex;
                var vertexIndex = charInfo.vertexIndex;

                var colors = meshInfos[meshIndex].colors32;
                if (colors == null || colors.Length <= vertexIndex + 3)
                    continue;

                colors[vertexIndex + 0] = color;
                colors[vertexIndex + 1] = color;
                colors[vertexIndex + 2] = color;
                colors[vertexIndex + 3] = color;

                if ((info.textComponent.fontStyle & FontStyles.Underline) != 0)
                {
                    var underlineVertexIndex = charInfo.underlineVertexIndex;
                    if (underlineVertexIndex >= 0 && underlineVertexIndex + 7 < colors.Length)
                    {
                        colors[underlineVertexIndex + 0] = color;
                        colors[underlineVertexIndex + 1] = color;
                        colors[underlineVertexIndex + 2] = color;
                        colors[underlineVertexIndex + 3] = color;
                        colors[underlineVertexIndex + 4] = color;
                        colors[underlineVertexIndex + 5] = color;
                        colors[underlineVertexIndex + 6] = color;
                        colors[underlineVertexIndex + 7] = color;
                    }
                }
                if ((info.textComponent.fontStyle & FontStyles.Strikethrough) != 0)
                {
                    var strikethroughVertexIndex = charInfo.strikethroughVertexIndex;
                    if (strikethroughVertexIndex >= 0 && strikethroughVertexIndex + 7 < colors.Length)
                    {
                        colors[strikethroughVertexIndex + 0] = color;
                        colors[strikethroughVertexIndex + 1] = color;
                        colors[strikethroughVertexIndex + 2] = color;
                        colors[strikethroughVertexIndex + 3] = color;
                        colors[strikethroughVertexIndex + 4] = color;
                        colors[strikethroughVertexIndex + 5] = color;
                        colors[strikethroughVertexIndex + 6] = color;
                        colors[strikethroughVertexIndex + 7] = color;
                    }
                }
            }

            _text.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
        }

        protected override void SubscribeOnly()
        {
            SetupDictionary();
            if (_pointerHandlerClick == null)
                return;

            _pointerHandlerClick.PointerUpEvent += OnPointerMove;
            _pointerHandlerClick.NonDragClickEvent += OnPointerClick;
            _pointerHandlerClick.PointerMoveEvent += OnPointerMove;
        }

        protected override void UnsubscribeOnly()
        {
            if (_pointerHandlerClick == null)
                return;

            _pointerHandlerClick.PointerUpEvent -= OnPointerMove;
            _pointerHandlerClick.NonDragClickEvent -= OnPointerClick;
            _pointerHandlerClick.PointerMoveEvent -= OnPointerMove;

            ClearHover();
        }

        private void OnValidate() => SetupDictionary();

        private void SetupDictionary()
        {
            var linkInfos = _text.textInfo.linkInfo;
            if (_linkColors.Count > 0)
            {
                foreach (var key in _linkColors.Keys.ToList())
                {
                    if (linkInfos.Any(l => l.GetLinkID() == key))
                        continue;
                    _linkColors.Remove(key);
                }
            }

            foreach (var linkInfo in linkInfos)
            {
                if (_linkColors.ContainsKey(linkInfo.GetLinkID()))
                    continue;
                _linkColors[linkInfo.GetLinkID()] = _hoverColor;
            }
        }

        public void SetTemplate(string template)
        {
            ClearHover();
            _text.text = template;
        }
        
        protected override void SetValueWithoutNotify(string value)
        {
        }
    }
}