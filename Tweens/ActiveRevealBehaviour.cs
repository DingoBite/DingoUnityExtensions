using System;

namespace DingoUnityExtensions.Tweens
{
    public class ActiveRevealBehaviour : RevealBehaviour
    {
        public override void AnimatableSetActive(bool value) => gameObject.SetActive(value);
        public override void SetActiveImmediately(bool value) => gameObject.SetActive(value);
        public override void EnableNoParams() => gameObject.SetActive(true);
        
        public override float Enable(float addDelay = 0, Action onComplete = null)
        {
            CoroutineParent.CancelCoroutine(this);
            if (gameObject.activeSelf)
                return 0;
            CoroutineParent.InvokeAfterSecondsWithCanceling(this, addDelay, () => gameObject.SetActive(true));
            return 0;
        }

        public override void DisableNoParams() => gameObject.SetActive(false);
        public override float Disable(float addDelay = 0, Action onComplete = null)
        {
            CoroutineParent.CancelCoroutine(this);
            if (!gameObject.activeSelf)
                return 0;
            CoroutineParent.InvokeAfterSecondsWithCanceling(this, addDelay, () => gameObject.SetActive(false));
            return 0;
        }

        public override void EnableImmediately() => gameObject.SetActive(true);
        public override void DisableImmediately() => gameObject.SetActive(false);
        public override void SetActive(bool value, bool isImmediately) => gameObject.SetActive(value);
    }
}