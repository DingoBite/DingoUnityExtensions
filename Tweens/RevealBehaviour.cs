using System;
using UnityEngine;

namespace DingoUnityExtensions.Tweens
{
    public enum AnimateState
    {
        None,
        Enabling,
        Enabled,
        Disabling,
        Disabled,
    }

    public interface IRevealBehaviour
    {
        public AnimateState State { get; }
        public void AnimatableSetActive(bool value);
        public void SetActiveImmediately(bool value);
        public void EnableNoParams();
        public float Enable(float addDelay = 0, Action onComplete = null);
        public void DisableNoParams();
        public float Disable(float addDelay = 0, Action onComplete = null);
        public void EnableImmediately();
        public void DisableImmediately();
        public void SetActive(bool value, bool isImmediately);
    }

    public abstract class RevealBehaviour : MonoBehaviour, IRevealBehaviour
    {
        public AnimateState State { get; protected set; }

        public abstract void AnimatableSetActive(bool value);
        public abstract void SetActiveImmediately(bool value);
        public abstract void EnableNoParams();
        public abstract float Enable(float addDelay = 0, Action onComplete = null);
        public abstract void DisableNoParams();
        public abstract float Disable(float addDelay = 0, Action onComplete = null);
        public abstract void EnableImmediately();
        public abstract void DisableImmediately();
        public abstract void SetActive(bool value, bool isImmediately);
    }
}