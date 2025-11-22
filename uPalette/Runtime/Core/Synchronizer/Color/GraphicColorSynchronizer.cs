using UnityEngine;
using UnityEngine.UI;

namespace uPalette.Runtime.Core.Synchronizer.Color
{
    [RequireComponent(typeof(Graphic))]
    [ColorSynchronizer(typeof(Graphic), "Color")]
    public sealed class GraphicColorSynchronizer : ColorSynchronizer<Graphic>
    {
        public override UnityEngine.Color GetValue()
        {
            return Component.color;
        }

        public override void SetValue(UnityEngine.Color value)
        {
            Component.color = value;
        }
    }
}
