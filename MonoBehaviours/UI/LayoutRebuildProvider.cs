using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace DingoUnityExtensions.MonoBehaviours.UI
{
    [RequireComponent(typeof(HorizontalOrVerticalLayoutGroup))]
    public class LayoutRebuildProvider : UIRequiredPropertyBehaviour<HorizontalOrVerticalLayoutGroup>
    {
        public void Rebuild()
        {
            if (gameObject.activeSelf)
                LayoutRebuilder.ForceRebuildLayoutImmediate(RectTransform);
        }

        public void RebuildOnNextFrame() => RebuildOnNextFrame(null);
        
        public void RebuildOnNextFrame(Action callback)
        {
            if (gameObject.activeSelf)
                CoroutineParent.Instance.StartCoroutine(Rebuild_C(callback));
        }

        public void RebuildOnNextFrameAndDisable() => RebuildOnNextFrameAndDisable(null);
        
        public void RebuildOnNextFrameAndDisable(Action callback)
        {
            if (gameObject.activeSelf)
                CoroutineParent.Instance.StartCoroutine(RebuildAndDisable_C(callback));
        }
        
        private IEnumerator Rebuild_C(Action callback = null)
        {
            yield return null;
            Rebuild();
            callback?.Invoke();
        }
        
        private IEnumerator RebuildAndDisable_C(Action callback = null)
        {
            yield return null;
            if (TryGetComponent<ContentSizeFitter>(out var sizeFitter))
            {
                sizeFitter.enabled = false;
            }
            yield return null;
            Rebuild();
            if (TryGetComponent<HorizontalOrVerticalLayoutGroup>(out var layout))
            {
                layout.enabled = false;
            }
            callback?.Invoke();
        }
    }
}