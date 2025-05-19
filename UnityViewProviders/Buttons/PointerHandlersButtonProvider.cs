using System.Collections.Generic;
using DingoUnityExtensions.MicroAnimations;
using DingoUnityExtensions.Tweens;
using DingoUnityExtensions.UnityViewProviders.Core;
using DingoUnityExtensions.UnityViewProviders.Core.Data;
using DingoUnityExtensions.UnityViewProviders.PointerHandlerWrappers;
using DingoUnityExtensions.UnityViewProviders.Toggle.Core;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DingoUnityExtensions.UnityViewProviders.Buttons
{
    public abstract class PointerHandlersButtonProvider<T> : UnityViewProvider<T>
        where T : MonoBehaviour, IPointerDownEventWrapper, IPointerUpEventWrapper, IPointerClickEventWrapper
    {
        [SerializeField] private ToggleSwapInfoBase _interactableToggle;
        [SerializeField] private bool _interactableImmediately;
        
        [SerializeReference, SubclassSelector] private List<MicroAnimation> _downUpOnlyClickAnimations;
        [SerializeReference, SubclassSelector] private List<MicroAnimation> _clickAnimations;

        public T PointerHandler => View;
        
        private readonly TweenList _tweens = new();
        
        protected override void SubscribeOnly()
        {
            View.PointerClickEvent += OnClick;
            View.PointerDownEvent += OnDown;
            View.PointerUpEvent += OnUp;
        }

        protected override void UnsubscribeOnly()
        {
            ResetView();
            View.PointerClickEvent -= OnClick;
            View.PointerDownEvent -= OnDown;
            View.PointerUpEvent -= OnUp;
        }

        protected override void OnSetInteractable(bool value)
        {
            View.enabled = value;
            if (_interactableToggle != null)
                _interactableToggle.SetViewActive(value.TimeContext(_interactableImmediately));
        }

        private void OnClick(PointerEventData data, float time)
        {
            foreach (var microAnimation in _downUpOnlyClickAnimations)
            {
                if (microAnimation != null)
                    microAnimation.ForwardAnimate();
            }
            EventInvoke();
        }

        private void OnDown(PointerEventData data, float time)
        {
            foreach (var microAnimation in _clickAnimations)
            {
                if (microAnimation != null)
                    microAnimation.ForwardAnimate();
            }
        }

        private void OnUp(PointerEventData data, float time)
        {
            foreach (var microAnimation in _clickAnimations)
            {
                if (microAnimation != null)
                    microAnimation.BackwardAnimate();
            }
        }

        protected virtual void ResetView()
        {
            foreach (var clickAnimation in _clickAnimations)
            {
                if (clickAnimation != null)
                    clickAnimation.ResetView();
            }
        }
    }
}