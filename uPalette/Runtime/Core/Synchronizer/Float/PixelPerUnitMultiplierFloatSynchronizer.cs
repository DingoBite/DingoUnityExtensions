using UnityEngine;
using UnityEngine.UI;

namespace uPalette.Runtime.Core.Synchronizer.PixelPerUnit
{
    [RequireComponent(typeof(Image))]
    [FloatSynchronizer(typeof(Image), "Float")]
    public class PixelPerUnitMultiplierFloatSynchronizer : FloatSynchronizer<Image>
    {
        protected internal override float GetValue()
        {
            return Component.pixelsPerUnitMultiplier;
        }
        protected internal override void SetValue(float value)
        {
            Component.pixelsPerUnitMultiplier = value;
        }
    }
}