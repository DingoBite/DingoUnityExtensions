using DingoUnityExtensions.UnityViewProviders.Core;
using UnityEngine;
using UnityEngine.UI;

namespace DingoUnityExtensions.UnityViewProviders.Buttons
{
    [RequireComponent(typeof(Button))]
    public class ButtonProvider : UnityViewProvider<Button>
    {
        protected override void SubscribeOnly() => View.onClick.AddListener(EventInvoke);
        protected override void UnsubscribeOnly() => View.onClick.RemoveListener(EventInvoke);
        protected override void OnSetInteractable(bool value)
        {
            View.interactable = value;
        }

#if UNITY_EDITOR
        [ContextMenu("Collect colors from graphics")]
        private void ButtonColorsFromGraphics()
        {
            var graphics = View.targetGraphic;
            if (graphics == null)
                return;
            var c = graphics.color;
            var viewColors = View.colors;
            View.colors = new ColorBlock
            {
                colorMultiplier = viewColors.colorMultiplier,
                fadeDuration = viewColors.fadeDuration,
                normalColor = c * viewColors.normalColor,
                disabledColor = c * viewColors.disabledColor,
                highlightedColor = c * viewColors.highlightedColor,
                pressedColor = c * viewColors.pressedColor,
                selectedColor = c * viewColors.selectedColor
            };
        }
#endif
    }
}