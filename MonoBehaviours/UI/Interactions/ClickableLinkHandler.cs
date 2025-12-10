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

        [Header("Hover settings")] [SerializeField]
        private Color32 _hoverColor = new Color32(0, 170, 255, 255);

        private int _lastLinkIndex = -1;
        private Color32? _normalColor;

        public void SetCamera(Camera c) => _camera = c;
        
        public void OnPointerClick(PointerEventData eventData, float time)
        {
            var linkIndex = TMP_TextUtilities.FindIntersectingLink(_text, eventData.position, _camera);
            Debug.Log($"Click: {eventData.position}: {linkIndex}");

            if (linkIndex != -1)
            {
                var linkInfo = _text.textInfo.linkInfo[linkIndex];
                var url = linkInfo.GetLinkID();
                Application.OpenURL(url);
            }
        }

        private void OnPointerMove(PointerEventData data, float time)
        {
            if (_text == null)
                return;

            var linkIndex = TMP_TextUtilities.FindIntersectingLink(_text, data.position, _camera);
            if (linkIndex == _lastLinkIndex)
                return;

            _normalColor ??= _text.color;
            if (_lastLinkIndex != -1)
                SetLinkColor(_lastLinkIndex, _normalColor.Value);

            if (linkIndex != -1)
                SetLinkColor(linkIndex, _hoverColor);

            _lastLinkIndex = linkIndex;
        }

        private void SetLinkColor(int linkIndex, Color32 color)
        {
            if (linkIndex < 0 || linkIndex >= _text.textInfo.linkCount)
                return;

            var linkInfo = _text.textInfo.linkInfo[linkIndex];

            for (var i = 0; i < linkInfo.linkTextLength; i++)
            {
                var charIndex = linkInfo.linkTextfirstCharacterIndex + i;
                if (charIndex < 0 || charIndex >= _text.textInfo.characterCount)
                    continue;

                var charInfo = _text.textInfo.characterInfo[charIndex];
                if (!charInfo.isVisible)
                    continue;

                var meshIndex = charInfo.materialReferenceIndex;
                var vertexIndex = charInfo.vertexIndex;

                var colors = _text.textInfo.meshInfo[meshIndex].colors32;

                colors[vertexIndex + 0] = color;
                colors[vertexIndex + 1] = color;
                colors[vertexIndex + 2] = color;
                colors[vertexIndex + 3] = color;
            }

            _text.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
        }

        protected override void SubscribeOnly()
        {
            _pointerHandlerClick.PointerClickEvent += OnPointerClick;
            _pointerHandlerClick.PointerMoveEvent += OnPointerMove;
        }

        protected override void UnsubscribeOnly()
        {
            _pointerHandlerClick.PointerClickEvent -= OnPointerClick;
            _pointerHandlerClick.PointerMoveEvent -= OnPointerMove;
        }
    }
}