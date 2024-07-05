using System;

namespace DingoUnityExtensions.UnityViewProviders.Core
{
    public class TabsContainerGroup<TId> : ValueContainerGroup<TId, bool>
    {
        public event Action<TId> OnTabSelect;

        public void SetTabWithoutNotify(TId id) => UpdateContainerValues(id, true);

        protected override void UpdateContainerValues(TId id, bool value)
        {
            if (!value)
                return;
            foreach (var (key, container) in EventContainers)
            {
                container.UpdateValueWithoutNotify(key.Equals(id));
            }

            OnTabSelect?.Invoke(id);
        }
    }
}