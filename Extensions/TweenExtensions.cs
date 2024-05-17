#if DOTWEEN
using System.Collections;
using DG.Tweening;

namespace DingoUnityExtensions.Extensions
{
    public static class TweenExtensions
    {
        public static IEnumerator WaitForCompletion_C(this Tween tween)
        {
            yield return tween.WaitForCompletion();
        }

        public static IEnumerator WaitForKill_C(this Tween tween)
        {
            yield return tween.WaitForKill();
        }

        public static IEnumerator WaitForElapsedLoops_C(this Tween tween, int elapsedLoops)
        {
            yield return tween.WaitForElapsedLoops(elapsedLoops);
        }

        public static IEnumerator WaitForPosition_C(this Tween tween, float position)
        {
            yield return tween.WaitForPosition(position);
        }

        public static IEnumerator WaitForStart_C(this Tween tween)
        {
            yield return tween.WaitForStart();
        }
    }
}
#endif
