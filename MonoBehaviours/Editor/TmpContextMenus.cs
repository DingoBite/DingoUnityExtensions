using UnityEditor;
using UnityEngine;
using TMPro;

namespace DingoUnityExtensions.MonoBehaviours.Editor
{
    [InitializeOnLoad]
    public static class TmpContextMenus
    {
        static TmpContextMenus()
        {
            EditorApplication.contextualPropertyMenu -= OnPropertyContextMenu;
            EditorApplication.contextualPropertyMenu += OnPropertyContextMenu;
        }

        private static void OnPropertyContextMenu(GenericMenu menu, SerializedProperty property)
        {
            if (property.propertyType != SerializedPropertyType.Vector4)
                return;

            if (property.name != "m_margin")
                return;

            var target = property.serializedObject.targetObject as TMP_Text;
            if (target == null)
                return;

            menu.AddItem(
                new GUIContent("Align SizeDelta to margins"),
                false,
                () => AlignSizeDeltaToMargins(target)
            );
        }

        private static void AlignSizeDeltaToMargins(TMP_Text tmp)
        {
            var rect = tmp.rectTransform;

            var margin = tmp.margin;

            var size = rect.sizeDelta;

            var newWidth  = margin.x + margin.z;
            var newHeight = margin.y + margin.w;

            rect.sizeDelta = new Vector2(newWidth, newHeight);

            EditorUtility.SetDirty(rect);
            EditorUtility.SetDirty(tmp);
        }
    }
}
