using uPalette.Runtime.Core.Synchronizer.Float;

namespace DingoUnityExtensions.uPaletteExtensions
{
    public class FloatPaletteMutatorTest : FloatPaletteMutator
    {
#if VINSPECTOR_EXISTS
        [VInspector.Button]
#endif
        private void Test(float value) => Mutate(value);
    }
}