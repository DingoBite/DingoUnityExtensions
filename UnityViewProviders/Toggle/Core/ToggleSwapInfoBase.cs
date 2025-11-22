using DingoUnityExtensions.UnityViewProviders.Core.Data;
using NaughtyAttributes;
using UnityEngine;

namespace DingoUnityExtensions.UnityViewProviders.Toggle.Core
{
    public abstract class ToggleSwapInfoBase : MonoBehaviour
    {
        public abstract void SetViewActive(BoolTimeContext value);

        [Button]
        private void TestEnabled() => SetViewActive(true.TimeContext(!Application.isPlaying));

        [Button]
        private void TestDisabled() => SetViewActive(false.TimeContext(!Application.isPlaying));
    }
}