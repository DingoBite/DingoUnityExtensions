using UnityEngine;
using UnityEngine.UI;

namespace uPalette.Runtime.Core.Synchronizer.Color
{
    [RequireComponent(typeof(Outline))]
    [ColorSynchronizer(typeof(Outline), "Color")]
    public sealed class OutlineColorSynchronizer : ColorSynchronizer<Outline>
    {
        public override UnityEngine.Color GetValue()
        {
            return Component.effectColor;
        }

        public override void SetValue(UnityEngine.Color value)
        {
            Component.effectColor = value;
        }
    }
}
