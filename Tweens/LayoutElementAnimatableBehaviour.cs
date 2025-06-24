using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace DingoUnityExtensions.Tweens
{
    [Flags]
    public enum LayoutElementFlags
    {
        None = 0,
        Min = 1 << 0,
        Preferred = 1 << 1,
        Flexible = 1 << 2,
        Everything = ~None,
    }

    public class LayoutElementAnimatableBehaviour : AnimatableBehaviour
    {
        [SerializeField] private LayoutElement _layoutElement;
        [SerializeField] private RectTransform _rectProvider;
        [SerializeField] private Vector2 _noRectSize;
        
        [SerializeField] private Vector2 _widthHeightMultiplier = new (1, 1);
        [SerializeField] private Vector2 _defaultMin;
        [SerializeField] private Vector2 _defaultPreferred;
        [SerializeField] private Vector2 _defaultFlexible;
        [SerializeField] private Vector2 _targetFlexible;

        [SerializeField] private LayoutElementFlags _flags;
        
        [SerializeField] private EnableDisableTweenAnimationPair _animation;

        private Vector2 RectSize => _rectProvider == null ? _noRectSize : _rectProvider.rect.size;
        
        protected override bool ValidAnimationParams => _layoutElement != null;

        public void SetTargetFlexibleSize(Vector2 size)
        {
            _targetFlexible = size;
        }
        
        protected override IEnumerable<Tween> CollectEnableTweens(bool isPlaying, float addDelay)
        {
            if (_flags == LayoutElementFlags.None)
                yield break;
            var animationDelay = isPlaying ? 0 : _animation.EnableDelay + addDelay;
            if (_flags.HasFlag(LayoutElementFlags.Min))
                yield return _animation.MakeEnableTween(d => _layoutElement.DOMinSize(RectSize, d).SetDelay(animationDelay));
            if (_flags.HasFlag(LayoutElementFlags.Preferred))
                yield return _animation.MakeEnableTween(d => _layoutElement.DOPreferredSize(RectSize, d).SetDelay(animationDelay));
            if (_flags.HasFlag(LayoutElementFlags.Flexible))
               yield return _animation.MakeEnableTween(d => _layoutElement.DOFlexibleSize(_targetFlexible, d).SetDelay(animationDelay));
        }

        protected override IEnumerable<Tween> CollectDisableTweens(bool isPlaying, float addDelay)
        {
            if (_flags == LayoutElementFlags.None)
                yield break;
            var animationDelay = isPlaying ? 0 : _animation.DisableDelay + addDelay;
            if (_flags.HasFlag(LayoutElementFlags.Min))
                yield return _animation.MakeDisableTween(d => _layoutElement.DOMinSize(_defaultMin, d).SetDelay(animationDelay));
            if (_flags.HasFlag(LayoutElementFlags.Preferred))
                yield return _animation.MakeDisableTween(d => _layoutElement.DOPreferredSize(_defaultPreferred, d).SetDelay(animationDelay));
            if (_flags.HasFlag(LayoutElementFlags.Flexible))
                yield return _animation.MakeDisableTween(d => _layoutElement.DOFlexibleSize(_defaultFlexible, d).SetDelay(animationDelay));
        }

        protected override void SetFullActive(AnimateState state, bool force = false)
        {
            base.SetFullActive(state, force);
            if (state is AnimateState.Enabled)
            {
                var rectSize = RectSize;
                _layoutElement.minWidth = rectSize.x;
                _layoutElement.minHeight = rectSize.y;
                _layoutElement.preferredWidth = rectSize.x;
                _layoutElement.preferredHeight = rectSize.y;
                _layoutElement.flexibleWidth = _targetFlexible.x;
                _layoutElement.flexibleHeight = _targetFlexible.y;
            }
            else if (state is AnimateState.Disabled)
            {
                _layoutElement.minWidth = _defaultMin.x;
                _layoutElement.minHeight = _defaultMin.y;
                _layoutElement.preferredWidth = _defaultPreferred.x;
                _layoutElement.preferredHeight = _defaultPreferred.y;
                _layoutElement.flexibleWidth = _defaultFlexible.x;
                _layoutElement.flexibleHeight = _defaultFlexible.y;

            }
        }
    }
}