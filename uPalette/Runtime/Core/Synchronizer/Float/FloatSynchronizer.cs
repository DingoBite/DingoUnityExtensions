using UnityEngine;
using uPalette.Runtime.Core.Model;

namespace uPalette.Runtime.Core.Synchronizer.Float
{
    public abstract class FloatSynchronizer : ValueSynchronizer<float>
    {
        [SerializeField] private FloatEntryId _entryId = new FloatEntryId();

        public override EntryId EntryId => _entryId;
        
        public override Palette<float> GetPalette(PaletteStore store)
        {
            return store.FloatPalette;
        }
    }

    public abstract class FloatSynchronizer<T> : FloatSynchronizer where T : Component
    {
        [SerializeField] [HideInInspector] private T _component;

        protected T Component
        {
            get
            {
                if (_component == null)
                {
                    _component = GetComponent<T>();
                }

                return _component;
            }
        }
    }
}
