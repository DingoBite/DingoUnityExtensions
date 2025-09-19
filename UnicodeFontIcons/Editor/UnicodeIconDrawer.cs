using System.Linq;
using DingoUnityExtensions.UnicodeFontIcons;
using RotaryHeart.Lib.AutoComplete;
using UnicodeFontIcons;
using UnityEditor;
using UnityEngine;

namespace RealEstateMap.Demo.UnicodeFontIcons.Editor
{
    [CustomPropertyDrawer(typeof(UnicodeIcon))]
    public class UnicodeIconDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var unicodeTMP = Selection.activeGameObject?.GetComponent<UnicodeTMP_Text>();
            if (unicodeTMP == null)
                return;

            EditorGUI.BeginProperty(position, label, property);

            var iconKeyProp = property.FindPropertyRelative(nameof(UnicodeIcon.IconKey));
            var fontWeightProp = property.FindPropertyRelative(nameof(UnicodeIcon.FontWeight));

            var lineHeight = EditorGUIUtility.singleLineHeight;
            var spacing = EditorGUIUtility.standardVerticalSpacing;
            var iconSize = lineHeight * 2.5f;
            var padding = 4f;

            var rectLabel = new Rect(position.x, position.y, position.width, lineHeight);
            EditorGUI.LabelField(rectLabel, label);

            var yOffset = rectLabel.y + lineHeight + spacing;
            var rectIconPreview = new Rect(position.x, yOffset, iconSize, iconSize);
            var rectKey = new Rect(rectIconPreview.xMax + padding, yOffset, position.width - iconSize - padding * 2 - 100, lineHeight);
            var rectWeight = new Rect(rectKey.xMax + padding, yOffset, 100, lineHeight);

            iconKeyProp.stringValue = EditorGUI.TextField(rectKey, iconKeyProp.stringValue);
            DrawIconPreview(rectIconPreview, iconKeyProp.stringValue);

            // var strings = unicodeTMP.GetAllKeys();
            // AutoCompleteDropDown.EditorGUI.AutoCompleteDropDown(
            //     rectKey,
            //     iconKeyProp.stringValue,
            //     strings,
            //     s =>
            //     {
            //         if (s.EndsWith("-"))
            //             s = s.Remove(s.Length - 1);
            //         iconKeyProp.stringValue = s;
            //         property.serializedObject.ApplyModifiedProperties();
            //     },
            //     true, true, true, "-");
            
            EditorGUI.PropertyField(rectWeight, fontWeightProp, GUIContent.none);
            EditorGUI.EndProperty();
        }

        private static int LexicographicValue(string input) => input.Aggregate(0, (current, c) => current + c);

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var lineHeight = EditorGUIUtility.singleLineHeight;
            var spacing = EditorGUIUtility.standardVerticalSpacing;
            var iconSize = lineHeight * 2.5f;
            return lineHeight + spacing + iconSize + spacing;
        }
        
        private void DrawIconPreview(Rect rect, string iconKey)
        {
            var unicodeTMP = Selection.activeGameObject?.GetComponent<UnicodeTMP_Text>();
            if (unicodeTMP == null)
                return;

            var mapping = unicodeTMP.GetUnicodeMapping();
            if (!unicodeTMP.CaseSensitive)
                iconKey = iconKey.ToLower();
            if (mapping.TryGetValue(iconKey, out var unicode))
            {
                var style = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    font = unicodeTMP.GetTMPFontAsset().sourceFontFile,
                    fontSize = Mathf.RoundToInt(rect.height * 0.8f),
                    normal = { textColor = Color.white }
                };

                var bgColor = EditorGUIUtility.isProSkin ? new Color(0.2f, 0.2f, 0.2f) : new Color(0.85f, 0.85f, 0.85f);
                EditorGUI.DrawRect(rect, bgColor);
                GUI.Box(rect, GUIContent.none, EditorStyles.helpBox);

                var shadowRect = new Rect(rect.x + 1, rect.y + 1, rect.width, rect.height);
                var shadowStyle = new GUIStyle(style)
                {
                    normal = { textColor = new Color(0, 0, 0, 0.3f) }
                };
                EditorGUI.LabelField(shadowRect, unicode, shadowStyle);

                EditorGUI.LabelField(rect, unicode, style);
            }
            else
            {
                var bgColor = EditorGUIUtility.isProSkin ? new Color(0.15f, 0.15f, 0.15f) : new Color(0.9f, 0.9f, 0.9f);
                EditorGUI.DrawRect(rect, bgColor);
                GUI.Box(rect, GUIContent.none, EditorStyles.helpBox);
                EditorGUI.LabelField(rect, "?", EditorStyles.centeredGreyMiniLabel);
            }
        }

    }
}