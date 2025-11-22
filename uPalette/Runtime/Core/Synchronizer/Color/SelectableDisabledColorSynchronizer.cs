using UnityEngine;
using UnityEngine.UI;

namespace uPalette.Runtime.Core.Synchronizer.Color
{
    [RequireComponent(typeof(Selectable))]
    [ColorSynchronizer(typeof(Selectable), "Transition Disabled Color")]
    public sealed class SelectableDisabledColorSynchronizer : ColorSynchronizer<Selectable>
    {
        public override UnityEngine.Color GetValue()
        {
            return Component.colors.disabledColor;
        }

        public override void SetValue(UnityEngine.Color value)
        {
            var colors = Component.colors;
            colors.disabledColor = value;
            Component.colors = colors;
        }
    }
}
