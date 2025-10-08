using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DingoUnityExtensions.UnityViewProviders.Core;
using UnityEngine;

namespace DingoUnityExtensions.UnityViewProviders.AsyncValueContainer.Core
{
    public enum AsyncValueState
    {
        None,
        PreLoading,
        Loading,
        Error,
        Cancelled,
        Aborted,
        Success,
        ValueUpdated,
    }

    public abstract class AsyncValueContainerProvider<T> : ValueContainer<Func<CancellationTokenSource, Task<T>>>
    {
        [SerializeField] private ValueContainer<T> _valueContainer;
        [SerializeField] private ValueContainer<AsyncValueState> _stateContainers;
        
        private CancellationTokenSource _cts;
        
        public T LoadedValue { get; private set; }

        protected ValueContainer<T> ValueContainer => _valueContainer;

        protected override void SetValueWithoutNotify(Func<CancellationTokenSource, Task<T>> value)
        {
            CancelLoading(false);
            _cts = new CancellationTokenSource();
            _stateContainers.UpdateValueWithoutNotify(AsyncValueState.PreLoading);
            var task = value(_cts);
            _stateContainers.UpdateValueWithoutNotify(AsyncValueState.Loading);
            try
            {
                _ = HandleTaskState(task, _cts.Token);
            }
            catch (Exception e)
            {
                _stateContainers.UpdateValueWithoutNotify(AsyncValueState.Error);
                Debug.LogException(e, this);
            }
        }

        private async Task HandleTaskState(Task<T> task, CancellationToken ctsToken)
        {
            LoadedValue = await task;
            if (ctsToken.IsCancellationRequested)
                return;
            await UniTask.SwitchToMainThread();
            if (ctsToken.IsCancellationRequested)
                return;
            _stateContainers.UpdateValueWithoutNotify(AsyncValueState.Success);
            SetValueWithoutNotify(LoadedValue);
            _stateContainers.UpdateValueWithoutNotify(AsyncValueState.ValueUpdated);
        }

        protected virtual void SetValueWithoutNotify(T value) => _valueContainer.UpdateValueWithoutNotify(value);
        protected virtual void ValueCancelLoading(bool abort) { }
        
        public void CancelLoading(bool abort)
        {
            if (_cts != null && !_cts.IsCancellationRequested)
            {
                try
                {
                    _cts.Cancel();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
                _stateContainers.UpdateValueWithoutNotify(abort ? AsyncValueState.Aborted : AsyncValueState.Cancelled);
            }
            ValueCancelLoading(abort);
        }

        public void ResetContainer()
        {
            CancelLoading(false);
            _stateContainers.UpdateValueWithoutNotify(AsyncValueState.None);
            _valueContainer.UpdateValueWithoutNotify(default);
            LoadedValue = default;
        }
    }
}