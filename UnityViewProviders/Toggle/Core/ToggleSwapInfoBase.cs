using DingoUnityExtensions.UnityViewProviders.Core.Data;
using NaughtyAttributes;
using UnityEngine;

namespace DingoUnityExtensions.UnityViewProviders.Toggle.Core
{
    public abstract class ToggleSwapInfoBase : MonoBehaviour
    {
        public abstract void SetViewActive(BoolTimeContext value);

#if VINSPECTOR_EXISTS
        [VInspector.Button]
#else
        [NaughtyAttributes.Button]
#endif
        private void TestEnabled() => SetViewActive(true.TimeContext(!Application.isPlaying));

#if VINSPECTOR_EXISTS
        [VInspector.Button]
#else
        [NaughtyAttributes.Button]
#endif
        private void TestDisabled() => SetViewActive(false.TimeContext(!Application.isPlaying));
    }
}