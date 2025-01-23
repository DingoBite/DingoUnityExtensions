using System;
using UnityEngine;

namespace DingoUnityExtensions.MicroAnimations
{
    [Serializable]
    public class SoundMicroAnimation : MicroAnimation
    {
        [SerializeField] private SoundMicroAnimationClips _soundMicroAnimationClips;
        [SerializeField] private string _forwardClip;
        [SerializeField] private string _backwardClip;

        public override void ForwardAnimate()
        {
            var clip = _soundMicroAnimationClips.GetClip(_forwardClip);
            SoundMicroAnimationSource.PlayOneHot(clip);
        }

        public override void BackwardAnimate()
        {
            var clip = _soundMicroAnimationClips.GetClip(_backwardClip);
            SoundMicroAnimationSource.PlayOneHot(clip);
        }
        public override void ResetView() { }
    }
}