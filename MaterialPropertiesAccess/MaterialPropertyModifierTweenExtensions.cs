
using UnityEngine;
#if DOTWEEN
using System;
using DG.Tweening;
#endif

namespace DingoUnityExtensions.MaterialPropertiesAccess
{
    public static class MaterialPropertyModifierTweenExtensions
    {
#if DOTWEEN
        public static Tween DoPropertyBlock(this MaterialPropertyModifierRoot materialPropertyModifierRoot, float duration, float start, float target)
        {
            var progress = start;
            return DOTween.To(get, set, target, duration);
                
            float get() => progress;

            void set(float t)
            {
                progress = t;
                try
                {
                    materialPropertyModifierRoot.SetTime(progress);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }
#endif
    }
}