using UnityEngine;
using uPalette.Runtime.Core.Model;

namespace uPalette.Runtime.Core.Synchronizer.Float
{
    public sealed class FloatSynchronizeEvent : ValueSynchronizeEvent<float>
    {
        [SerializeField] private FloatEntryId _entryId = new FloatEntryId();

        public override EntryId EntryId => _entryId;

        public override Palette<float> GetPalette(PaletteStore store)
        {
            return store.FloatPalette;
        }
    }
}
