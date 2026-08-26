using System;
using UnityEngine;

namespace DingoUnityExtensions.MicroAnimations
{
    [Serializable]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceAssembly: "Assembly-CSharp")]
    public class SoundMicroAnimation : MicroAnimation
    {
        [SerializeField] private string _forwardClip;
        [SerializeField] private string _backwardClip;

        public override void ForwardAnimate() => SoundMicroAnimationSource.PlayOneHot(_forwardClip);
        public override void BackwardAnimate() => SoundMicroAnimationSource.PlayOneHot(_backwardClip);
        
        public override void ResetView() { }
    }
}
