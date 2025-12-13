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
            _tween?.Kill();
            if (!Application.isPlaying || _animation == null || !gameObject.activeInHierarchy || FromStartObserving)
            {
                Component.BorderWidth = value;
                _targetValue = 0;
                return;
            }

            if (value < 0)
            {
                _targetValue = 0;
                var rectSize = Component.rectTransform.rect.size.magnitude * 0.5f;
                _tween = _animation.Do(d => DOTween.To(() => Component.BorderWidth, v => Component.BorderWidth = v, rectSize, d));
                // _tween.OnComplete(() => Component.BorderWidth = _targetValue.Value);
            }
            else
            {
                _targetValue = value;
                _tween = _animation.Do(d => DOTween.To(() => Component.BorderWidth, v => Component.BorderWidth = v, value, d));
            }
        }
    }
}