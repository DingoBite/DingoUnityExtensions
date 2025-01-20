using System;
using UnityEngine;

namespace DingoUnityExtensions.Tweens
{
    public class AnimationEventDispatcher : MonoBehaviour
    {
        public event Action<string> OnAnimationStart;
        public event Action<string> OnAnimationComplete;
        
        [SerializeField] private Animator _animator;
        [SerializeField] private bool _awakeInitialize = true;

        private void Awake()
        {
            if (!_awakeInitialize)
                return;
            Initialize();
        }

        public void Initialize(Animator animator)
        {
            _animator = animator;
            Initialize();
        }
        
        public void Initialize()
        {
            if (_animator == null && !TryGetComponent(out _animator))
                return;
            foreach (var clip in _animator.runtimeAnimatorController.animationClips)
            {
                var animationStartEvent = new AnimationEvent();
                animationStartEvent.time = 0;
                animationStartEvent.functionName = "AnimationStartHandler";
                animationStartEvent.stringParameter = clip.name;
            
                var animationEndEvent = new AnimationEvent();
                animationEndEvent.time = clip.length;
                animationEndEvent.functionName = "AnimationCompleteHandler";
                animationEndEvent.stringParameter = clip.name;
            
                clip.AddEvent(animationStartEvent);
                clip.AddEvent(animationEndEvent);
            }
        }

        public void AnimationStartHandler(string stateName) => OnAnimationStart?.Invoke(stateName);
        public void AnimationCompleteHandler(string stateName) => OnAnimationComplete?.Invoke(stateName);
    }
}