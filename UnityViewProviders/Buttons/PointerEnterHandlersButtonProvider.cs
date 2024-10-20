using System.Collections.Generic;
using DingoUnityExtensions.MicroAnimations;
using DingoUnityExtensions.UnityViewProviders.PointerHandlerWrappers;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DingoUnityExtensions.UnityViewProviders.Buttons
{
    public abstract class PointerEnterHandlersButtonProvider<T> : PointerHandlersButtonProvider<T>
        where T : MonoBehaviour, IPointerDownEventWrapper, IPointerUpEventWrapper, IPointerClickEventWrapper, IPointerEnterEventWrapper, IPointerExitEventWrapper
    {
        [SerializeReference, SubclassSelector] private List<TweenMicroAnimation> _enterAnimations;

        protected override void SubscribeOnly()
        {
            base.SubscribeOnly();
            View.PointerEnterEvent += OnEnter;
            View.PointerExitEvent += OnExit;
        }

        protected override void UnsubscribeOnly()
        {
            base.UnsubscribeOnly();
            View.PointerEnterEvent -= OnEnter;
            View.PointerExitEvent -= OnExit;
        }

        private void OnEnter(PointerEventData data, float time)
        {
            foreach (var microAnimation in _enterAnimations)
            {
                microAnimation.ForwardAnimate();
            }
        }

        private void OnExit(PointerEventData data, float time)
        {
            foreach (var microAnimation in _enterAnimations)
            {
                microAnimation.BackwardAnimate();
            }
        }

        protected override void ResetView()
        {
            base.ResetView();
            foreach (var enterAnimation in _enterAnimations)
            {
                enterAnimation.ResetView();
            }
        }
    }
}