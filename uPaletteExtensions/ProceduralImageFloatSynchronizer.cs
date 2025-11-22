using DG.Tweening;
using DingoUnityExtensions.Tweens;
using UnityEngine;
using UnityEngine.UI.ProceduralImage;
using uPalette.Runtime.Core.Synchronizer.Float;

namespace DingoUnityExtensions.uPaletteExtensions
{
    [RequireComponent(typeof(ProceduralImage))]
    [FloatSynchronizer(typeof(ProceduralImage), "Border Width - ANIMATABLE")]
    public class ProceduralImageFloatSynchronizer_A : FloatSynchronizer<ProceduralImage>
    {
        [SerializeField] private TweenAnimation _animation;

        private Tween _tween;
        private float? _targetValue;

        public override float GetValue()
        {
            return _targetValue ?? Component.BorderWidth;
        }

        public override void SetValue(float value)
        {
            if (!Application.isPlaying || _animation == null)
            {
                Component.BorderWidth = value;
                return;
            }
            _targetValue = value;
            _tween?.Kill();
            _animation.Do(d => DOTween.To(() => Component.BorderWidth, v => Component.BorderWidth = v, value, d));
        }
    }
}