using UnityEngine;
using UnityEngine.UI;

namespace uPalette.Runtime.Core.Synchronizer.Float
{
    [RequireComponent(typeof(Image))]
    [FloatSynchronizer(typeof(Image), "Pixel Per Unit Multiplier")]
    public class PixelPerUnitMultiplierFloatSynchronizer : FloatSynchronizer<Image>
    {
        public override float GetValue()
        {
            return Component.pixelsPerUnitMultiplier;
        }

        public override void SetValue(float value)
        {
            Component.pixelsPerUnitMultiplier = value;
        }
    }
}