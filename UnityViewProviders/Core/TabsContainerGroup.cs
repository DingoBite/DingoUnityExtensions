using System;

namespace DingoUnityExtensions.UnityViewProviders.Core
{
    public class TabsContainerGroup<TId> : ValueContainerGroup<TId, bool>
    {
        public event Action<TId> OnTabSelect;
        public event Action OnFullDeselect;

        public void SetTabWithoutNotify(TId id) => UpdateContainerValuesWithoutNotify(id, true);
        
        protected override void UpdateContainerValues(TId id, bool value)
        {
            if (!UpdateContainerValuesWithoutNotify(id, value))
            {
                OnFullDeselect?.Invoke();
                return;
            }
            OnTabSelect?.Invoke(id);
        }

        public bool UpdateContainerValuesWithoutNotify(TId id, bool value)
        {
            if (!value)
                return false;
            foreach (var (key, container) in Containers)
            {
                container.UpdateValueWithoutNotify(key.Equals(id));
            }

            return true;
        }

        public void DeselectAll()
        {
            foreach (var (key, container) in Containers)
            {
                container.UpdateValueWithoutNotify(false);
            }
        }
    }
}