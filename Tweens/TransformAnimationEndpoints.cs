using System;
using System.Collections.Generic;
using DG.Tweening;
using DingoUnityExtensions.Extensions;
using UnityEngine;

namespace DingoUnityExtensions.Tweens
{
    [Serializable]
    public class TransformAnimationEndpoints
    {
        [SerializeField] protected EnableDisableTweenAnimationPair Animation;

        [SerializeField] private Matrix4x4 _defaultTransform;
        [SerializeField] private Matrix4x4 _targetTransform;
        [SerializeField] private Matrix4x4 _disableTransform;
        [SerializeField] private float _addictiveEnableDelay;
        [SerializeField] private float _addictiveDisableDelay;

        public bool ValidAnimation => Animation != null;
        
        public void SetDefaultValues(Transform transform)
        {
            transform.FromMatrix(_defaultTransform);
        }

        public void SetTargetValues(Transform transform)
        {
            transform.FromMatrix(_targetTransform);
        }

        public void SetDisableValues(Transform transform)
        {
            transform.FromMatrix(_disableTransform);
        }

        public IEnumerable<Tween> Enable(Transform transform, float addDelay = 0) => Enable(Animation, transform, addDelay);
        public IEnumerable<Tween> Enable(EnableDisableTweenAnimationPair animation, Transform transform, float addDelay = 0)
        {
            var (position, rotation, scale) = _targetTransform.FromMatrix();
            yield return MakeEnableAndAdd(animation, d => transform.DOLocalMove(position, d), addDelay);
            yield return MakeEnableAndAdd(animation, d => transform.DOLocalRotate(rotation.eulerAngles, d), addDelay);
            yield return MakeEnableAndAdd(animation, d => transform.DOScale(scale, d), addDelay);
        }

        public IEnumerable<Tween> Disable(Transform transform, float addDelay = 0) => Disable(Animation, transform, addDelay);
        public IEnumerable<Tween> Disable(EnableDisableTweenAnimationPair animation, Transform transform, float addDelay = 0)
        {
            var (position, rotation, scale) = _disableTransform.FromMatrix();
            yield return MakeDisableAndAdd(animation, d => transform.DOLocalMove(position, d), addDelay);
            yield return MakeDisableAndAdd(animation, d => transform.DOLocalRotate(rotation.eulerAngles, d), addDelay);
            yield return MakeDisableAndAdd(animation, d => transform.DOScale(scale, d), addDelay);
        }
        
        public void BakeDefaultValues(Transform transform) => _defaultTransform = Matrix4x4.TRS(transform.localPosition, transform.localRotation, transform.localScale);
        public void BakeTargetValues(Transform transform) => _targetTransform = Matrix4x4.TRS(transform.localPosition, transform.localRotation, transform.localScale);
        public void BakeDisableValues(Transform transform) => _disableTransform = Matrix4x4.TRS(transform.localPosition, transform.localRotation, transform.localScale);

        private Tween MakeDisableAndAdd(EnableDisableTweenAnimationPair animation, Func<float, Tween> tweenFactoryMethod, float addDelay = 0)
        {
            return animation.MakeDisableTween(tweenFactoryMethod, out _, _addictiveDisableDelay + addDelay);
        }
        
        private Tween MakeEnableAndAdd(EnableDisableTweenAnimationPair animation, Func<float, Tween> tweenFactoryMethod, float addDelay = 0)
        {
            return animation.MakeEnableTween(tweenFactoryMethod, out _, _addictiveEnableDelay + addDelay);
        }
    }
}