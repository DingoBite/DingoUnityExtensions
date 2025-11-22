using DG.Tweening;
using DingoUnityExtensions.Tweens;
using UnityEngine;
using UnityEngine.UI;
using uPalette.Runtime.Core.Synchronizer.Color;

namespace DingoUnityExtensions.uPaletteExtensions
{
    [RequireComponent(typeof(Graphic))]
    [ColorSynchronizer(typeof(Graphic), "Color - ANIMATABLE")]
    public sealed class GraphicColorSynchronizer_A : ColorSynchronizer<Graphic>
    {
        [SerializeField] private TweenAnimation _animation;

        private Tween _tween;
        private Color? _targetValue;

        public override Color GetValue()
        {
            return _targetValue ?? Component.color;
        }

        public override void SetValue(Color value)
        {
            if (!Application.isPlaying || _animation == null)
            {
                Component.color = value;
                return;
            }
            _targetValue = value;
            _tween?.Kill();
            _animation.Do(d => DOTween.To(() => Component.color, v => Component.color = v, value, d));
        }
    }
}