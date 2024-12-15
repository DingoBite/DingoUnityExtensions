using System;
using UnityEngine;

namespace DingoUnityExtensions.SleepSystem
{
    public class UISleepManager : SleepManager<UISleepable>
    {
        [SerializeField] private RectTransform _mask;
        
        public void SetupMask(RectTransform mask)
        {
            if (mask == null)
            {
                Debug.LogException(new NullReferenceException(nameof(mask)));
                return;
            }

            _mask = mask;
            UpdateSleep();
        }

        protected override bool Selector(UISleepable sleepable)
        {
            if (_mask == null || sleepable == null || sleepable.RectTransform == null || !sleepable.gameObject.activeInHierarchy)
                return true;

            var maskCorners = new Vector3[4];
            var sleepableCorners = new Vector3[4];

            _mask.GetWorldCorners(maskCorners);
            sleepable.RectTransform.GetWorldCorners(sleepableCorners);

            var maskMinX = Mathf.Min(maskCorners[0].x, maskCorners[1].x, maskCorners[2].x, maskCorners[3].x);
            var maskMaxX = Mathf.Max(maskCorners[0].x, maskCorners[1].x, maskCorners[2].x, maskCorners[3].x);
            var maskMinY = Mathf.Min(maskCorners[0].y, maskCorners[1].y, maskCorners[2].y, maskCorners[3].y);
            var maskMaxY = Mathf.Max(maskCorners[0].y, maskCorners[1].y, maskCorners[2].y, maskCorners[3].y);

            var sleepableMinX = Mathf.Min(sleepableCorners[0].x, sleepableCorners[1].x, sleepableCorners[2].x, sleepableCorners[3].x);
            var sleepableMaxX = Mathf.Max(sleepableCorners[0].x, sleepableCorners[1].x, sleepableCorners[2].x, sleepableCorners[3].x);
            var sleepableMinY = Mathf.Min(sleepableCorners[0].y, sleepableCorners[1].y, sleepableCorners[2].y, sleepableCorners[3].y);
            var sleepableMaxY = Mathf.Max(sleepableCorners[0].y, sleepableCorners[1].y, sleepableCorners[2].y, sleepableCorners[3].y);

            var maskRect = new Rect(maskMinX, maskMinY, maskMaxX - maskMinX, maskMaxY - maskMinY);
            var sleepableRect = new Rect(sleepableMinX, sleepableMinY, sleepableMaxX - sleepableMinX, sleepableMaxY - sleepableMinY);

            return !maskRect.Overlaps(sleepableRect);
        }
    }
}