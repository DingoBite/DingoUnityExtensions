using System.Collections.Generic;
using DingoUnityExtensions.MonoBehaviours.UI.UIGraph;
using TMPro;
using UnityEngine;

namespace DingoUnityExtensions.MonoBehaviours.UI.Interactions
{
    public static class UIGraphSelectionBuilder
    {
        public static List<UIGraphShape> BuildTextSelectionShapes(TMP_Text text, int startCharIndex, int endCharIndex, Color color)
        {
            var result = new List<UIGraphShape>();

            if (text == null || text.textInfo == null)
                return result;

            text.ForceMeshUpdate();

            var textInfo = text.textInfo;
            if (textInfo.characterCount == 0)
                return result;

            startCharIndex = Mathf.Clamp(startCharIndex, 0, textInfo.characterCount - 1);
            endCharIndex = Mathf.Clamp(endCharIndex, 0, textInfo.characterCount - 1);

            if (endCharIndex < startCharIndex)
            {
                var tmp = startCharIndex;
                startCharIndex = endCharIndex;
                endCharIndex = tmp;
            }

            var currentLine = -1;
            var lineStartChar = startCharIndex;

            for (var i = startCharIndex; i <= endCharIndex; i++)
            {
                var charInfo = textInfo.characterInfo[i];
                if (!charInfo.isVisible)
                    continue;

                if (currentLine < 0)
                {
                    currentLine = charInfo.lineNumber;
                    lineStartChar = i;
                }

                if (charInfo.lineNumber != currentLine)
                {
                    AddLineRect(text, textInfo, lineStartChar, i - 1, currentLine, color, result);
                    currentLine = charInfo.lineNumber;
                    lineStartChar = i;
                }
            }

            if (currentLine >= 0)
            {
                AddLineRect(text, textInfo, lineStartChar, endCharIndex, currentLine, color, result);
            }

            return result;
        }

        private static void AddLineRect(TMP_Text text, TMP_TextInfo textInfo, int startCharIndex, int endCharIndex, int lineNumber, Color color, List<UIGraphShape> list)
        {
            TMP_CharacterInfo firstChar = default;
            var firstFound = false;
            for (var i = startCharIndex; i <= endCharIndex; i++)
            {
                var ch = textInfo.characterInfo[i];
                if (!ch.isVisible)
                    continue;
                firstChar = ch;
                firstFound = true;
                break;
            }

            TMP_CharacterInfo lastChar = default;
            var lastFound = false;
            for (var i = endCharIndex; i >= startCharIndex; i--)
            {
                var ch = textInfo.characterInfo[i];
                if (!ch.isVisible)
                    continue;
                lastChar = ch;
                lastFound = true;
                break;
            }

            if (!firstFound || !lastFound)
                return;

            var lineInfo = textInfo.lineInfo[lineNumber];

            var left = firstChar.bottomLeft.x;
            var right = lastChar.topRight.x;

            var bottom = lineInfo.descender;
            var top = lineInfo.ascender;

            var paddingX = 1f;
            var paddingY = 1f;
            left -= paddingX;
            right += paddingX;
            bottom -= paddingY;
            top += paddingY;

            var rectTransform = text.rectTransform;
            var rect = rectTransform.rect;

            var p0 = new Vector2(normalizeX(left), normalizeY(bottom));
            var p1 = new Vector2(normalizeX(left), normalizeY(top));
            var p2 = new Vector2(normalizeX(right), normalizeY(top));
            var p3 = new Vector2(normalizeX(right), normalizeY(bottom));

            var points = new List<Vector2> { p0, p1, p2, p3 };

            var shape = new UIGraphShape(points, color, thickness: 0f, isLine: false);
            list.Add(shape);
            return;

            float normalizeX(float x) => Mathf.InverseLerp(rect.xMin, rect.xMax, x);
            float normalizeY(float y) => Mathf.InverseLerp(rect.yMin, rect.yMax, y);
        }
    }
}