using System;

namespace uPalette.Runtime.Core.Synchronizer.PixelPerUnit
{
    public sealed class FloatSynchronizerAttribute : ValueSynchronizerAttribute
    {
        public FloatSynchronizerAttribute(Type attachTargetType, string targetPropertyDisplayName)
            : base(typeof(float), attachTargetType, targetPropertyDisplayName)
        {
        }
    }
}
