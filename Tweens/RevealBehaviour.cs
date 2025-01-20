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

    public abstract class RevealBehaviour : MonoBehaviour
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
    }
}