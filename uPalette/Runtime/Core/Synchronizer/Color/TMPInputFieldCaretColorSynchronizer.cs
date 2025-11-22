using TMPro;
using UnityEngine;

namespace uPalette.Runtime.Core.Synchronizer.Color
{
    [RequireComponent(typeof(TMP_InputField))]
    [ColorSynchronizer(typeof(TMP_InputField), "Caret Color")]
    public sealed class TMPInputFieldCaretColorSynchronizer : ColorSynchronizer<TMP_InputField>
    {
        public override UnityEngine.Color GetValue()
        {
            return Component.caretColor;
        }

        public override void SetValue(UnityEngine.Color value)
        {
            Component.customCaretColor = true;
            Component.caretColor = value;
        }
    }
}
