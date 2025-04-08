using System;
using System.Collections.Generic;
using DingoUnityExtensions.Extensions;
using DingoUnityExtensions.Tweens;
using DingoUnityExtensions.UnityViewProviders.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Scripting;
using UnityEngine.UI;

namespace DingoUnityExtensions.MonoBehaviours.UI.Scrolls
{
    [Serializable, Preserve]
    public class RectTransformPage
    {
        public RectTransform RectTransform;
        public ValueContainer<bool> PassiveToggle;
    }
    
    public abstract class AccordionScroll : SubscribableBehaviour
    {
        [SerializeField] private EventContainer _back;
        [SerializeField] private EventContainer _continue;
        [SerializeField] private EventContainer _start;
        
        [SerializeField] private List<RectTransformPage> _pages;
        [SerializeField] private double _stopThreshold = 0.01f;
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private ScrollRectEnsureVisible _scrollRectEnsureVisible;
        [SerializeField] private RectTransform _centerRevealRect;
        [SerializeField] private ScrollRectDragWrapper _scrollRectDragWrapper;
        [SerializeField] private TweenAnimation _reselectAnimation;

        private int _currentIndex;

        public void ResetScroll()
        {
            _currentIndex = 0;
            SetActiveProgress(0, true);
            _continue.gameObject.SetActive(true);
            _start.gameObject.SetActive(false);
        }

        private void GoBack() 
        {
            _currentIndex--;
            UpdateIndex();
        }

        private void Continue()
        {
            _currentIndex++;
            UpdateIndex();
        }

        private void UpdateIndex()
        {
            if (_currentIndex < 0)
            {
                _currentIndex = 0;
                FirstElementBack();
                return;
            }

            _currentIndex = Math.Min(_currentIndex, _pages.Count - 1);
            SetActiveProgress(_currentIndex, false);
        }

        private void SetActiveProgress(int index, bool immediately)
        {
            if (_currentIndex >= _pages.Count - 1)
            {
                _continue.gameObject.SetActive(false);
                _start.gameObject.SetActive(true);
            }
            else
            {
                _continue.gameObject.SetActive(true);
                _start.gameObject.SetActive(false);
            }

            for (var i = 0; i < _pages.Count; i++)
            {
                _pages[i].PassiveToggle.UpdateValueWithoutNotify(i == index);
            }

            var rect = _pages[index];
            _scrollRectEnsureVisible.CenterOnItemFrameWait(rect.RectTransform, immediately);
        }

        private void SnapToCurrent()
        {
            CoroutineParent.CancelCoroutine(this);
            _scrollRect.StopMovement();
            _scrollRectEnsureVisible.CancelSelect();
            var currentId = CalculateCurrent(out var closest);
            _currentIndex = currentId;
            SetActiveProgress(_currentIndex, false);
        }
        
        private int CalculateCurrent(out RectTransform closest)
        {
            var closestDist = float.MaxValue;
            closest = null;
            var content = _scrollRect.content;
            var currentId = 0;
            for (var i = 0; i < _pages.Count; i++)
            {
                var exerciseContainer = _pages[i];
                var elementCenterX = content.anchoredPosition.x - content.pivot.x * content.rect.width + exerciseContainer.RectTransform.anchoredPosition.x;
                var dist = Mathf.Abs(elementCenterX - _centerRevealRect.anchoredPosition.x);
                if (dist < closestDist)
                {
                    currentId = i;
                    closestDist = dist;
                    closest = exerciseContainer.RectTransform;
                }
            }

            return currentId;
        }

        private void StartUserDrag(PointerEventData data, float time)
        {
            _scrollRect.StopMovement();
            _scrollRectEnsureVisible.CancelSelect();
        }

        private void EndUserDrag(PointerEventData data, float time)
        {
            var rt = _pages[_currentIndex].RectTransform;
            var currentId = CalculateCurrent(out var closest);
            if (currentId == _currentIndex)
                _scrollRectEnsureVisible.CenterOnItemFrameWait(rt, false, _reselectAnimation);
            else
                SnapToCurrent();
        }
        
        protected abstract void LastElementStart();
        protected abstract void FirstElementBack();

        protected override void SubscribeOnly()
        {
            _start.SafeSubscribe(LastElementStart);
            _continue.SafeSubscribe(Continue);
            _back.SafeSubscribe(GoBack);
            _scrollRectDragWrapper.BeginDragEvent += StartUserDrag;
            _scrollRectDragWrapper.EndDragEvent += EndUserDrag;
        }
        
        protected override void UnsubscribeOnly()
        {
            _start.UnSubscribe(LastElementStart);
            _continue.UnSubscribe(Continue);
            _back.UnSubscribe(GoBack);
            _scrollRectDragWrapper.BeginDragEvent -= StartUserDrag;
            _scrollRectDragWrapper.EndDragEvent -= EndUserDrag;
        }
    }
}