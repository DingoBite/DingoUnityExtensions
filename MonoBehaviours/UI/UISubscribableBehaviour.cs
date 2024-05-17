namespace DingoUnityExtensions.MonoBehaviours.UI
{
    public abstract class UISubscribableBehaviour : UIBehaviour
    {
        protected void Subscribe() => SubscribeOnly();

        protected abstract void SubscribeOnly();
        
        protected abstract void UnsubscribeOnly();

        protected virtual void OnEnable()
        {
            UnsubscribeOnly();
            Subscribe();
        }

        protected virtual void OnDisable()
        {
            UnsubscribeOnly();
        }
    }
}