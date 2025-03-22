using System;
using System.Collections.Generic;
using DG.Tweening;
using DingoUnityExtensions.Extensions;
using UnityEngine;

namespace DingoUnityExtensions.Tweens
{
    [Serializable]
    public struct RectTransformData
    {
        public Vector2 Pivot;
        public Vector2 MinAnchors;
        public Vector2 MaxAnchors;
        
        public Vector2 AnchoredPositionNormalized; // Normalized anchored position
        public Quaternion Rotation;
        public Vector2 Scale; // Normalized scale relative to the parent size
        public Vector4 SizeDelta;

        public static RectTransformData FromRectTransform(RectTransform rectTransform)
        {
            var parent = rectTransform.parent.GetComponent<RectTransform>();
            return FromRectTransform(rectTransform, parent);
        }

        public static RectTransformData FromRectTransform(RectTransform rectTransform, RectTransform parent)
        {
            if (parent == null)
                parent = rectTransform.parent as RectTransform;
            var parentSize = parent != null ? parent.rect.size : Vector2.one;

            return new RectTransformData
            {
                Pivot = rectTransform.pivot,
                MinAnchors = rectTransform.anchorMin,
                MaxAnchors = rectTransform.anchorMax,
                AnchoredPositionNormalized = rectTransform.anchoredPosition / parentSize,
                Rotation = rectTransform.localRotation,
                Scale = rectTransform.localScale,
                SizeDelta = rectTransform.sizeDelta
            };
        }

        public void ApplyTo(RectTransform rectTransform)
        {
            var parent = rectTransform.parent as RectTransform;
            ApplyTo(rectTransform, parent);
        }
        
        public void ApplyTo(RectTransform rectTransform, RectTransform parent)
        {
            if (parent == null)
                parent = rectTransform.parent as RectTransform;
            var parentSize = parent != null ? parent.rect.size : Vector2.one;

            ApplyPositionAnchors(rectTransform);
            rectTransform.anchoredPosition = AnchoredPositionNormalized * parentSize;
            rectTransform.localRotation = Rotation;
            rectTransform.localScale = new Vector3(
                Scale.x,
                Scale.y,
                rectTransform.localScale.z
            );
            rectTransform.sizeDelta = SizeDelta;
        }

        public void ApplyPositionAnchors(RectTransform rectTransform)
        {
            return;
            // TODO Pivot handling
            // rectTransform.SetPivot(Pivot);
            // rectTransform.anchorMin = MinAnchors;
            // rectTransform.anchorMax = MaxAnchors;
        }
    }

    [Serializable, UnityEngine.Scripting.Preserve]
    public class RectTransformAnimationEndpoints
    {
        [SerializeField] protected EnableDisableTweenAnimationPair Animation;

        [SerializeField] private RectTransformData _defaultTransform;
        [SerializeField] private RectTransformData _targetTransform;
        [SerializeField] private RectTransformData _disableTransform;
        [SerializeField] private float _addictiveEnableDelay;
        [SerializeField] private float _addictiveDisableDelay;
        
        [SerializeField] public RectTransform OverrideParent;
        
        public bool ValidAnimation => Animation != null;

        public void SetDefaultValues(RectTransform rectTransform) => _defaultTransform.ApplyTo(rectTransform, OverrideParent);
        public void SetTargetValues(RectTransform rectTransform) => _targetTransform.ApplyTo(rectTransform, OverrideParent);
        public void SetDisableValues(RectTransform rectTransform) => _disableTransform.ApplyTo(rectTransform, OverrideParent);

        public IEnumerable<Tween> Enable(RectTransform rectTransform, float addDelay = 0) => Enable(Animation, rectTransform, addDelay);

        public IEnumerable<Tween> Enable(EnableDisableTweenAnimationPair animation, RectTransform rectTransform, float addDelay = 0)
        {
            var parent = OverrideParent;
            if (parent == null)
                parent = rectTransform.parent as RectTransform;
            var parentSize = parent != null ? parent.rect.size : Vector2.one;
            _targetTransform.ApplyPositionAnchors(rectTransform);
            
            yield return MakeEnableAndAdd(animation, d => rectTransform.DOAnchorPos(_targetTransform.AnchoredPositionNormalized * parentSize, d), addDelay);
            yield return MakeEnableAndAdd(animation, d => rectTransform.DOLocalRotate(_targetTransform.Rotation.eulerAngles, d), addDelay);
            yield return MakeEnableAndAdd(animation, d => rectTransform.DOSizeDelta(_targetTransform.SizeDelta, d), addDelay);
            yield return MakeEnableAndAdd(animation, d => rectTransform.DOScale(new Vector3(
                _targetTransform.Scale.x,
                _targetTransform.Scale.y,
                rectTransform.localScale.z), d), addDelay);
        }

        public IEnumerable<Tween> Disable(RectTransform rectTransform, float addDelay = 0) => Disable(Animation, rectTransform, addDelay);

        public IEnumerable<Tween> Disable(EnableDisableTweenAnimationPair animation, RectTransform rectTransform, float addDelay = 0)
        {
            var parent = OverrideParent;
            if (parent == null)
                parent = rectTransform.parent as RectTransform;
            var parentSize = parent != null ? parent.rect.size : Vector2.one;
            _disableTransform.ApplyPositionAnchors(rectTransform);

            yield return MakeDisableAndAdd(animation, d => rectTransform.DOAnchorPos(_disableTransform.AnchoredPositionNormalized * parentSize, d), addDelay);
            yield return MakeDisableAndAdd(animation, d => rectTransform.DOLocalRotate(_disableTransform.Rotation.eulerAngles, d), addDelay);
            yield return MakeDisableAndAdd(animation, d => rectTransform.DOSizeDelta(_disableTransform.SizeDelta, d), addDelay);
            yield return MakeDisableAndAdd(animation, d => rectTransform.DOScale(new Vector3(
                _disableTransform.Scale.x,
                _disableTransform.Scale.y,
                rectTransform.localScale.z), d), addDelay);
        }

        public void BakeDefaultValues(RectTransform rectTransform) => _defaultTransform = RectTransformData.FromRectTransform(rectTransform, OverrideParent);
        public void BakeTargetValues(RectTransform rectTransform) => _targetTransform = RectTransformData.FromRectTransform(rectTransform, OverrideParent);
        public void BakeDisableValues(RectTransform rectTransform) => _disableTransform = RectTransformData.FromRectTransform(rectTransform, OverrideParent);
        
        private Tween MakeDisableAndAdd(EnableDisableTweenAnimationPair animation, TweenUtils.Factory tweenFactoryMethod, float addDelay = 0)
        {
            return animation.MakeDisableTween(tweenFactoryMethod, out _, _addictiveDisableDelay + addDelay);
        }

        private Tween MakeEnableAndAdd(EnableDisableTweenAnimationPair animation, TweenUtils.Factory tweenFactoryMethod, float addDelay = 0)
        {
            return animation.MakeEnableTween(tweenFactoryMethod, out _, _addictiveEnableDelay + addDelay);
        }
    }
}