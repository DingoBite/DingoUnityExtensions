using DingoUnityExtensions.UnityViewProviders.Core;
using DingoUnityExtensions.UnityViewProviders.Toggle.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace DingoUnityExtensions.UnityViewProviders.Toggle
{
    public abstract class ToggleBehaviourProvider<TView> : UnityViewProvider<TView, bool> where TView : Component
    {
        [SerializeField] private bool _nonInteractablePlaceholder;
        [FormerlySerializedAs("_toggleSwapInfo")] [SerializeField] protected ToggleSwapInfoBase ToggleSwapInfo;

        protected override bool NonInteractablePlaceholder => _nonInteractablePlaceholder;

        protected abstract void SetViewValueWithoutNotify(bool value);

        protected override void SetValueWithoutNotify(bool value)
        {
            SetViewValueWithoutNotify(value);
            if (ToggleSwapInfo != null)
                ToggleSwapInfo.SetViewActive(Value);
        }
    }
}