using System;
using DingoUnityExtensions.MonoBehaviours;

namespace DingoUnityExtensions.UnityViewProviders.Core
{
    public abstract class ContainerBase : SubscribableBehaviour
    {
        public virtual void SetDefaultView() {}
        protected override void SubscribeOnly() { }
        protected override void UnsubscribeOnly() { }
        public virtual bool Interactable { get; set; }
        public virtual bool Selectable { get; }
        public virtual bool Selected { get; set; }
        public virtual Type ValueType { get; }
        public virtual void SetActiveContainer(bool value) { }
        public virtual void UpdateBoxedValueWithoutNotify(object value) {}

        protected override void OnDisable()
        {
            Selected = false;
            base.OnDisable();
        }
    }
}