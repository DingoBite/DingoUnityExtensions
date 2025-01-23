using System.Collections.Generic;
using UnityEngine;

namespace DingoUnityExtensions.MicroAnimations
{
    public class MicroAnimationComponent : MonoBehaviour
    {
        [SerializeReference, SubclassSelector] private List<MicroAnimation> _enterAnimations;

        public void PlayForward()
        {
            foreach (var enterAnimation in _enterAnimations)
            {
                if (enterAnimation != null)
                    enterAnimation.ForwardAnimate();
            }
        }

        public void PlayBackward()
        {
            foreach (var enterAnimation in _enterAnimations)
            {
                if (enterAnimation != null)
                    enterAnimation.BackwardAnimate();
            }
        }
    }
}