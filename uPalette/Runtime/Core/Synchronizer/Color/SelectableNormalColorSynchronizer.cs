using UnityEngine;
using UnityEngine.UI;

namespace uPalette.Runtime.Core.Synchronizer.Color
{
    [RequireComponent(typeof(Selectable))]
    [ColorSynchronizer(typeof(Selectable), "Transition Normal Color")]
    public sealed class SelectableNormalColorSynchronizer : ColorSynchronizer<Selectable>
    {
        public override UnityEngine.Color GetValue()
        {
            return Component.colors.normalColor;
        }

        public override void SetValue(UnityEngine.Color value)
        {
            var colors = Component.colors;
            colors.normalColor = value;
            Component.colors = colors;
        }
    }
}
