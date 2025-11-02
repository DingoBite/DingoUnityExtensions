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
        where T : MonoBehaviour, IPointerDownEventWrapper, IPointerUpEventWrapper, IPointerClickEventWrapper, ISelectWrapper, IDeselectWrapper
    {
        [SerializeField] private ToggleSwapInfoBase _interactableToggle;
        [SerializeField] private bool _interactableImmediately;
        
        [SerializeField] private ToggleSwapInfoBase _selectToggle;
        [SerializeField] private bool _selectImmediately;
        
        [SerializeReference, SubclassSelector] private List<MicroAnimation> _downUpOnlyClickAnimations;
        [SerializeReference, SubclassSelector] private List<MicroAnimation> _clickAnimations;

        public T PointerHandler => View;
        
        private readonly TweenList _tweens = new();
        
        protected override void SubscribeOnly()
        {
            View.PointerClickEvent += OnClick;
            View.PointerDownEvent += OnDown;
            View.PointerUpEvent += OnUp;
            View.SelectEvent += OnSelect;
            View.DeselectEvent += OnDeselect;
        }

        protected override void UnsubscribeOnly()
        {
            ResetView();
            View.PointerClickEvent -= OnClick;
            View.PointerDownEvent -= OnDown;
            View.PointerUpEvent -= OnUp;
            View.SelectEvent -= OnSelect;
            View.DeselectEvent -= OnDeselect;
        }

        private void OnDeselect(BaseEventData data, float time)
        {
            if (!Interactable)
                return;
            
            Selected = false;
            EventSystem.current.SetSelectedGameObject(gameObject, data);
        }

        private void OnSelect(BaseEventData data, float time)
        {
            if (!Interactable)
                return;
            
            Selected = true;
            EventSystem.current.SetSelectedGameObject(gameObject, data);
        }

        protected override void OnSelected(bool value)
        {
            if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != gameObject)
                EventSystem.current.SetSelectedGameObject(gameObject);
            _selectToggle.SetViewActive(value.TimeContext(_selectImmediately));
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