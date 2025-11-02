using UnityEditor;
using uPalette.Runtime.Core.Synchronizer.PixelPerUnit;

namespace uPalette.Editor.Core.Synchronizer
{
    [CustomEditor(inspectedType: typeof(FloatSynchronizeEvent), editorForChildClasses: true)]
    public sealed class FloatSynchronizeEventEditor : ValueSynchronizeEventEditor<float>
    {
    }
}
