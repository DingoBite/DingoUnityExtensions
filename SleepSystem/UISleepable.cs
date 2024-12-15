using UnityEngine;

namespace DingoUnityExtensions.SleepSystem
{
    public class UISleepable : Sleepable
    {
        [field: SerializeField] public RectTransform RectTransform { get; private set; }

        private void Reset() => RectTransform = GetComponent<RectTransform>();
    }
}