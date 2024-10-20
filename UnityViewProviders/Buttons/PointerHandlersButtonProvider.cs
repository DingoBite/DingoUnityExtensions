using System.Collections.Generic;
using DingoUnityExtensions.MicroAnimations;
using DingoUnityExtensions.Tweens;
using DingoUnityExtensions.UnityViewProviders.Core;
using DingoUnityExtensions.UnityViewProviders.PointerHandlerWrappers;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DingoUnityExtensions.UnityViewProviders.Buttons
{
    public abstract class PointerHandlersButtonProvider<T> : UnityViewProvider<T>
        where T : MonoBehaviour, IPointerDownEventWrapper, IPointerUpEventWrapper, IPointerClickEventWrapper
    {
        [SerializeReference, SubclassSelector] private List<TweenMicroAnimation> _clickAnimations;

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

        protected override void OnSetInteractable(bool value) => View.enabled = value;

        private void OnClick(PointerEventData data, float time) => EventInvoke();
        
        private void OnDown(PointerEventData data, float time)
        {
            foreach (var microAnimation in _clickAnimations)
            {
                microAnimation.ForwardAnimate();
            }
        }

        private void OnUp(PointerEventData data, float time)
        {
            foreach (var microAnimation in _clickAnimations)
            {
                microAnimation.BackwardAnimate();
            }
        }

        protected virtual void ResetView()
        {
            foreach (var clickAnimation in _clickAnimations)
            {
                clickAnimation.ResetView();
            }
        }
    }
}