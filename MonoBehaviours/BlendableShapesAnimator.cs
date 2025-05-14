using System;
using System.Collections.Generic;
using DG.Tweening;
using DingoUnityExtensions.Tweens;
using NaughtyAttributes;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DingoUnityExtensions.MonoBehaviours
{
    [Serializable]
    public class BlendableShapesParameter
    {
        [SerializeField] private float _startValue = 0;
        [SerializeField] private float _endValue = 1;
        [SerializeField] private float _durationMultiplier = 1;
        [SerializeField] private float _delayBetweenAnimation = 0;

        [SerializeField] private bool _isLoop;

        [SerializeField, ShowIf(nameof(_isLoop)), AllowNesting]
        private LoopType _loopType = LoopType.Yoyo;

        [SerializeField, ShowIf(nameof(_isLoop)), AllowNesting]
        private Vector2 _randomizeDelayBetweenRange = new(0, 0);

        [SerializeField] private TweenAnimation _tweenAnimation;

        private Tween _tween;

        private int _index = -1;
        private SkinnedMeshRenderer _meshRenderer;

        private float _progress;

        private float GetProgress() => _progress;

        private void SetProgress(float progress)
        {
            _progress = progress;
            if (_index < 0 || _meshRenderer == null)
                return;
            var length = _endValue - _startValue;
            var weight = _startValue + _progress * length;
            _meshRenderer.SetBlendShapeWeight(_index, weight);
        }

        public void Pause() => _tween?.Pause();
        public void Resume() => _tween?.Play();
        
        public void StartAnimate(int index, SkinnedMeshRenderer meshRenderer)
        {
            _meshRenderer = meshRenderer;
            _index = index;
            _meshRenderer = meshRenderer;
            _tween?.Kill();
            var tween = CollectMainAnimationTween();
            if (_isLoop)
            {
                var isRandomizeDelay = Math.Abs(_randomizeDelayBetweenRange.y - _randomizeDelayBetweenRange.x) > Vector2.kEpsilon;
                if (_delayBetweenAnimation <= Vector2.kEpsilon && !isRandomizeDelay)
                {
                    tween.SetLoops(-1, _loopType);
                }
                else
                {
                    var sequence = DOTween.Sequence();
                    sequence.SetLoops(-1, _loopType);
                    if (!isRandomizeDelay)
                    {
                        sequence.SetDelay(_delayBetweenAnimation);
                        sequence.PrependInterval(_delayBetweenAnimation);
                    }
                    else
                    {
                        var originalTween = tween;
                        sequence.OnStepComplete(() =>
                        {
                            var delay = GetRandomDelay();
                            originalTween.SetDelay(delay);
                        });
                        sequence.SetDelay(GetRandomDelay());
                    }
                    sequence.Append(tween);
                    tween = sequence;
                }
            }
            else
            {
                if (_delayBetweenAnimation > Vector2.kEpsilon)
                    tween.SetDelay(_delayBetweenAnimation);
            }

            _tween = tween;
            _tween.Play();
        }

        private float GetRandomDelay() => _delayBetweenAnimation + Mathf.Lerp(_randomizeDelayBetweenRange.x, _randomizeDelayBetweenRange.y, Random.value);

        private Tween CollectMainAnimationTween() => _tweenAnimation.Do(d => DOTween.To(GetProgress, SetProgress, 1, d * _durationMultiplier));

        public void Reset(int index, SkinnedMeshRenderer meshRenderer)
        {
            _tween?.Kill();
            _index = index;
            _meshRenderer = meshRenderer;
            SetProgress(0);
            _index = -1;
            _meshRenderer = null;
        }
    }

    public class BlendableShapesAnimator : MonoBehaviour
    {
        [SerializeField] private SkinnedMeshRenderer _meshRenderer;
        [SerializeField] private List<BlendableShapesParameter> _blendableShapesParameters;
        [SerializeField] private bool _playOnEnable = true;
        [SerializeField] private bool _resetBeforePlay = true;

        public void StartAnimations()
        {
            for (var i = 0; i < _blendableShapesParameters.Count; i++)
            {
                var parameter = _blendableShapesParameters[i];
                parameter.StartAnimate(i, _meshRenderer);
            }
        }

        public void Pause()
        {
            for (var i = 0; i < _blendableShapesParameters.Count; i++)
            {
                var parameter = _blendableShapesParameters[i];
                parameter.Pause();
            }
        }
        
        public void Resume()
        {
            for (var i = 0; i < _blendableShapesParameters.Count; i++)
            {
                var parameter = _blendableShapesParameters[i];
                parameter.Resume();
            }
        }
        
        public void ResetAll()
        {
            for (var i = 0; i < _blendableShapesParameters.Count; i++)
            {
                var parameter = _blendableShapesParameters[i];
                parameter.Reset(i, _meshRenderer);
            }
        }

        private void OnEnable()
        {
            if (_resetBeforePlay)
                ResetAll();
            if (_playOnEnable)
                StartAnimations();
        }

        private void OnDestroy()
        {
            ResetAll();
        }
    }
}