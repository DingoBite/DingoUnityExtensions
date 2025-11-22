using UnityEngine;
using UnityEngine.UI;

namespace uPalette.Runtime.Core.Synchronizer.Color
{
    [RequireComponent(typeof(InputField))]
    [ColorSynchronizer(typeof(InputField), "Selection Color")]
    public sealed class InputFieldSelectionColorSynchronizer : ColorSynchronizer<InputField>
    {
        public override UnityEngine.Color GetValue()
        {
            return Component.selectionColor;
        }

        public override void SetValue(UnityEngine.Color value)
        {
            Component.selectionColor = value;
        }
    }
}
