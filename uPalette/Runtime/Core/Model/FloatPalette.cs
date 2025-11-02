using System;
using UnityEngine;

namespace uPalette.Runtime.Core.Model
{
    [Serializable]
    public sealed class FloatPalette : Palette<float>
    {
        protected override float GetDefaultValue()
        {
            return 1f;
        }
    }
}