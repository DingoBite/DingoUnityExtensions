#if DOTWEEN
using DG.Tweening;
#endif

namespace DingoUnityExtensions.MaterialPropertyBlockAnimations
{
    public static class MaterialPropertyBlockModifierTweenExtensions
    {
#if DOTWEEN
        public static Tween DoPropertyBlock(this MaterialPropertyBlockModifierRoot materialPropertyBlockModifier, float duration, float start, float target)
        {
            var progress = start;
            return DOTween.To(get, set, target, duration);
                
            float get() => progress;

            void set(float t)
            {
                progress = t;
                materialPropertyBlockModifier.SetTime(progress);
            }
        }
#endif
    }
}