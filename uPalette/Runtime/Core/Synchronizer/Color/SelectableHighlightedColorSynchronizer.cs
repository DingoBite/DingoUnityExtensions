using UnityEngine;
using UnityEngine.UI;

namespace uPalette.Runtime.Core.Synchronizer.Color
{
    [RequireComponent(typeof(Selectable))]
    [ColorSynchronizer(typeof(Selectable), "Transition Highlighted Color")]
    public sealed class SelectableHighlightedColorSynchronizer : ColorSynchronizer<Selectable>
    {
        public override UnityEngine.Color GetValue()
        {
            return Component.colors.highlightedColor;
        }

        public override void SetValue(UnityEngine.Color value)
        {
            var colors = Component.colors;
            colors.highlightedColor = value;
            Component.colors = colors;
        }
    }
}
