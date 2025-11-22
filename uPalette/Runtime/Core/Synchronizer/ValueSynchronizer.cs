using UnityEngine;

namespace uPalette.Runtime.Core.Synchronizer
{
    [ExecuteAlways]
    public abstract class ValueSynchronizer<T> : ValueSynchronizerBase<T>
    {
        public abstract T GetValue();

        public abstract void SetValue(T value);

        protected override void OnValueChanged(T value)
        {
            SetValue(value);
        }
    }
}
