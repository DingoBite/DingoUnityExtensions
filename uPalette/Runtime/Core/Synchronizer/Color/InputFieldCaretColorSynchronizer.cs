using UnityEngine;
using UnityEngine.UI;

namespace uPalette.Runtime.Core.Synchronizer.Color
{
    [RequireComponent(typeof(InputField))]
    [ColorSynchronizer(typeof(InputField), "Caret Color")]
    public sealed class InputFieldCaretColorSynchronizer : ColorSynchronizer<InputField>
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
