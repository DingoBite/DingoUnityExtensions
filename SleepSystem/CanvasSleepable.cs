using UnityEngine;

namespace DingoUnityExtensions.SleepSystem
{
    public class CanvasSleepable : Sleepable
    {
        [field: SerializeField] public Canvas Canvas { get; private set; }

        private void Reset() => Canvas = GetComponent<Canvas>();

        protected override void OnSetSlippingValue(bool value)
        {
            Canvas.enabled = !value;
            Visible = Canvas.enabled;
        }
    }
}