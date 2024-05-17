using UnityEngine;

namespace DingoUnityExtensions.MonoBehaviours
{
    public abstract class SubscribableBehaviour : MonoBehaviour
    {
        protected abstract void SubscribeOnly();

        protected abstract void UnsubscribeOnly();

        protected virtual void OnEnable()
        {
            UnsubscribeOnly();
            SubscribeOnly();
        }

        protected virtual void OnDisable()
        {
            UnsubscribeOnly();
        }
    }
}