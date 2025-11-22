using UnityEditor;
using uPalette.Runtime.Core.Synchronizer.Float;

namespace uPalette.Editor.Core.Synchronizer
{
    [CustomEditor(inspectedType: typeof(FloatSynchronizer), editorForChildClasses: true)]
    public sealed class FloatSynchronizerEditor : ValueSynchronizerEditor<float>
    {
    }
}
