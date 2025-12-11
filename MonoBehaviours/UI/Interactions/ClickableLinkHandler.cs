using DingoUnityExtensions.UnityViewProviders.PointerHandlerWrappers;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DingoUnityExtensions.MonoBehaviours.UI.Interactions
{
    public class ClickableLinkHandler : SubscribableBehaviour
    {
        [SerializeField] private TMP_Text _text;
        [SerializeField] private PointerHandlerClickDrag _pointerHandlerClick;
        [SerializeField] private Camera _camera;

        [Header("Hover settings")]
        [SerializeField] private Color32 _hoverColor = new(0, 170, 255, 255);

        private int _lastLinkIndex = -1;

        public void SetCamera(Camera c) => _camera = c;

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
                Application.OpenURL(url);
            SetLinkColor(_lastLinkIndex, _hoverColor);
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

            _text.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
        }

        protected override void SubscribeOnly()
        {
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
    }
}
