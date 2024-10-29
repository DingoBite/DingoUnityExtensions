using System;
using System.Collections;

namespace DingoUnityExtensions.Utils
{
    public interface IPlayTimeScheduler
    {
        public delegate void UpdateAction(in float time, in float delta, in int frame);
        public delegate void TimeChange(in float startTime, in float lastResumeTime, in float applicationTime, in float timeDelta);

        public event TimeChange OnTimeChange;

        public void AddUpdate(object obj, UpdateAction updateAction, bool isLate);
        public void RemoveUpdate(object obj, bool isLate);
        public void ScheduleCoroutineThreadSafe(string key, Func<IEnumerator> factory, out Action stopAction);
        public void ScheduleActionThreadSafe(string key, Action action, out Action stopAction);
        public void ClearAllScheduleActions();
        public void ClearScheduleActions(string key);
        
        public void ResetSessionTimer();
        public void Pause();
        public void Resume();
    }
}