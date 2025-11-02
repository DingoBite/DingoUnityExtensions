using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace uPalette.Editor.Core.PaletteEditor
{
    internal sealed class FloatPaletteEditorTreeView : PaletteEditorTreeView<float>
    {
        public FloatPaletteEditorTreeView(TreeViewState state) : base(state)
        {
        }

        protected override float DrawValueField(Rect rect, float value)
        {
            return EditorGUI.FloatField(rect, value);
        }
    }
}
