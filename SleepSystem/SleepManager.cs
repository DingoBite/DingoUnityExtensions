using System;
using System.Collections.Generic;
using System.Linq;
using DingoUnityExtensions.MonoBehaviours;
using UnityEngine;

namespace DingoUnityExtensions.SleepSystem
{
    public abstract class SleepManager<T> : MonoBehaviour where T : Sleepable
    {
        [SerializeField] private List<PopulateParent> _populateParents;
        [SerializeField] private List<T> _staticVisibilityHandlers;
        [SerializeField] private List<T> _visibilityHandlers;
        [SerializeField] private float _visibilityUpdatePeriod;

        private float _time;

        public void FullRefresh()
        {
            SetDirty();
            PopulateComponents();
            UpdateSleep();
        }
        
        public void SetDirty()
        {
            foreach (var populateParent in _populateParents)
            {
                populateParent.SetDirty();
            }
        }
        
        public void PopulateComponents()
        {
            _visibilityHandlers.Clear();
            foreach (var populateParent in _populateParents)
            {
                populateParent.Repopulate<T>();
                _visibilityHandlers.AddRange(populateParent.Get<T>());
            }
        }
        
        public void UpdateSleep()
        {
            _time = 0;
            var sleepables = _staticVisibilityHandlers
                .Concat(_visibilityHandlers)
                .Where(s => s != null)
                .Select(s => (s, Selector(s)));
            SetSleepable(sleepables);
        }
        
        public void ForceSetSleep(bool value)
        {
            _time = 0;
            var sleepables = _staticVisibilityHandlers
                .Concat(_visibilityHandlers)
                .Where(s => s != null)
                .Select(s => (s, value));
            SetSleepable(sleepables);
        }

        public void SetSleepable(IEnumerable<(T sleepable, bool value)> sleepables)
        {
            foreach (var (sleepable, value) in sleepables)
            {
                BeforeSetSleep(sleepable, value);
                sleepable.SetSlippingValue(value);
            }
        }
        
        private void PeriodUpdateVisibility()
        {
            if (_visibilityUpdatePeriod < -Vector2.kEpsilon)
                return;
            
            if (Math.Abs(_visibilityUpdatePeriod) < Vector2.kEpsilon)
            {
                UpdateSleep();
                return;
            }

            _time += Time.deltaTime;
            if (_time > _visibilityUpdatePeriod)
                UpdateSleep();
        }

        protected abstract bool Selector(T visibilityHandler);
        protected virtual void BeforeSetSleep(T visibilityHandler, bool sleeping){}
        
        private void OnEnable()
        {
            PopulateComponents();
            UpdateSleep();
            CoroutineParent.AddLateUpdater(this, PeriodUpdateVisibility);
        }

        private void OnDisable()
        {
            ForceSetSleep(true);
            CoroutineParent.RemoveLateUpdater(this);
        }
    }
}