using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Bind;
using DG.Tweening;
using DingoUnityExtensions.Tweens;
using UnityEngine;
using UnityEngine.UI;

namespace DingoUnityExtensions.MonoBehaviours.UI.Scrolls
{
    [RequireComponent(typeof(ScrollRect))]
    public class ScrollRectEnsureVisible : MonoBehaviour
    {
        [SerializeField] private TweenAnimation _tweenAnimation;
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private List<ScrollRect> _stack;
        [SerializeField] private Vector2 _offset;
        
        private Tween _tween;
        private bool _initialized;
        private float _elasticity;
        private List<float> _elasticityStack;

        public ScrollRect ScrollRect => _scrollRect;
        
        private void Reset()
        {
            _scrollRect = GetComponent<ScrollRect>();
        }

        private void Initialize()
        {
            if (_initialized)
                return;
            _elasticity = _scrollRect.elasticity;
            if (_stack.Count > 0)
                _elasticityStack = _stack.Select(e => e.elasticity).ToList();
            _initialized = true;
        }
        
        public void CancelSelect() => _tween?.Kill();

        public void CenterOnItemFrameWait(RectTransform target, bool isImmediately = false, TweenAnimation overrideAnimation = null, Action onComplete = null)
        {
            CoroutineParent.StartCoroutineWithCanceling(this, () => Factory(target, isImmediately, overrideAnimation, onComplete));
        }

        private IEnumerator Factory(RectTransform target, bool isImmediately = false, TweenAnimation overrideAnimation = null, Action onComplete = null)
        {
            yield return null;
            CenterOnItem(target, isImmediately, overrideAnimation, onComplete);
        }

        public void CenterOnItem(RectTransform target, bool isImmediately = false, TweenAnimation overrideAnimation = null, Action onComplete = null, bool preserveOnVisible = false)
        {
            if (preserveOnVisible && IsFullyVisible(target))
            {
                _tween?.Kill();
                onComplete?.Invoke();
                return;
            }

            var viewport = _scrollRect.viewport;
            var content = _scrollRect.content;

            var contentSize = content.rect.size;
            var viewportSize = viewport.rect.size;
            var normalizedPosition = _scrollRect.normalizedPosition;

            if (_scrollRect.horizontal)
            {
                var viewportWidth = viewportSize.x;
                var contentWidth = contentSize.x;
                var viewportCenter = viewportWidth * viewport.pivot.x + _offset.x;
                var targetPositionInContent = target.anchoredPosition.x;
                var viewportConventDiff = contentWidth - viewportWidth;
                var desiredX = (targetPositionInContent - viewportCenter) / viewportConventDiff;
                normalizedPosition.x = Mathf.Clamp01(desiredX);
            }

            if (_scrollRect.vertical)
            {
                var viewportHeight = viewportSize.y;
                var contentHeight = contentSize.y;
                var viewportCenter = viewportHeight * (1 - viewport.pivot.y) + _offset.y;
                var targetPositionInContent = target.anchoredPosition.y + target.pivot.y * target.rect.height;
                var viewportConventDiff = contentHeight - viewportHeight;
                var desiredY = 1 - (viewportCenter - targetPositionInContent) / viewportConventDiff;
                normalizedPosition.y = Mathf.Clamp01(desiredY);
            }

            _tween?.Kill();
            if (isImmediately)
            {
                _scrollRect.normalizedPosition = normalizedPosition;
                onComplete?.Invoke();
            }
            else
            {
                var tweenAnimation = overrideAnimation == null ? _tweenAnimation : overrideAnimation;
                _tween = tweenAnimation.Do(d => _scrollRect.DONormalizedPos(normalizedPosition, d));
                BlockMovement(true);
                _tween.OnComplete(() =>
                {
                    BlockMovement(false);
                    onComplete?.Invoke();
                });
                _tween.OnKill(() => { BlockMovement(false); });
                _tween.Play();
            }
        }

        private bool IsFullyVisible(Transform target)
        {
            var viewport = _scrollRect.viewport;
            var bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(viewport, target);
            var r = viewport.rect;

            var hOk = !_scrollRect.horizontal || (bounds.min.x >= r.xMin && bounds.max.x <= r.xMax);
            var vOk = !_scrollRect.vertical || (bounds.min.y >= r.yMin && bounds.max.y <= r.yMax);

            return hOk && vOk;
        }
        
        public void BlockMovement(bool value, bool stackOnly = false)
        {
            Initialize();
            if (value)
            {
                if (!stackOnly)
                    _scrollRect.elasticity = float.MaxValue;
                foreach (var scrollRect in _stack)
                {
                    scrollRect.elasticity = float.MaxValue;
                }
            }
            else
            {
                if (!stackOnly)
                    _scrollRect.elasticity = _elasticity;
                for (var i = 0; i < _stack.Count; i++)
                {
                    var scrollRect = _stack[i];
                    scrollRect.elasticity = _elasticityStack[i];
                }
            }
        }

        private void UpdateStack(Vector2 normalizedPosition)
        {
            foreach (var scrollRect in _stack)
            {
                scrollRect.normalizedPosition = normalizedPosition;
            }
        }
 
        private void OnEnable()
        {
            BlockMovement(false);
            _scrollRect.onValueChanged.SafeSubscribe(UpdateStack);
        }

        private void OnDisable()
        {
            _scrollRect.onValueChanged.RemoveListener(UpdateStack);
        }
        
        /// <summary>
        /// Takes a float value from a [0f,1f] range and translates it to a [-1f,1f] range
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private float Translate01RangeToMinus11Range(float value)
        {
            return value + ((1f - value)*-1f);
        }
    }
}