using TMPro;
using UnityEngine;

namespace uPalette.Runtime.Core.Synchronizer.Color
{
    [RequireComponent(typeof(TMP_InputField))]
    [ColorSynchronizer(typeof(TMP_InputField), "Selection Color")]
    public sealed class TMPInputFieldSelectionColorSynchronizer : ColorSynchronizer<TMP_InputField>
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
