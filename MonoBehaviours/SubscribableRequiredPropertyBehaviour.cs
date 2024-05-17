using UnityEngine;

namespace DingoUnityExtensions.MonoBehaviours
{
    public abstract class SubscribableRequiredPropertyBehaviour<T> : RequiredPropertyBehaviour<T> where T : Component
    {
        private void Subscribe()
        {
            Unsubscribe();
            OnSubscribe();
        }

        protected abstract void OnSubscribe();

        private void Unsubscribe()
        {
            OnUnsubscribe();
        }

        protected abstract void OnUnsubscribe();

        private void OnEnable()
        {
            Subscribe();
        }
        
        private void OnDisable()
        {
            Unsubscribe();
        }
    }
}