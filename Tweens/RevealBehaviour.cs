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

        public virtual void SetActive(bool value, bool isImmediately)
        {
            if (isImmediately)
                SetActiveImmediately(value);
            else 
                AnimatableSetActive(value);
        }
        
        public virtual void AnimatableSetActive(bool value)
        {
            if (value)
                EnableNoParams();
            else 
                DisableNoParams();
        }
        
        public virtual void SetActiveImmediately(bool value)
        {
            if (value)
                EnableImmediately();
            else 
                DisableImmediately();
        }
        
        public virtual void EnableNoParams()
        {
            if (!Application.isPlaying && Application.isEditor)
                EnableImmediately();
            else 
                Enable();
        }

        public virtual void DisableNoParams()
        {
            if (!Application.isPlaying && Application.isEditor)
                DisableImmediately();
            else 
                Disable();
        }
        
        public abstract float Enable(float addDelay = 0, Action onComplete = null);
        public abstract float Disable(float addDelay = 0, Action onComplete = null);
        public abstract void EnableImmediately();
        public abstract void DisableImmediately();
    }
}