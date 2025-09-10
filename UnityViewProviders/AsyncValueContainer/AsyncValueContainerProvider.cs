using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DingoUnityExtensions.UnityViewProviders.Core;
using UnityEngine;

namespace DingoUnityExtensions.UnityViewProviders.AsyncValueContainer
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

        protected override void SetValueWithoutNotify(Func<CancellationTokenSource, Task<T>> value)
        {
            CancelLoading(false);
            _cts = new CancellationTokenSource();
            _stateContainers.UpdateValueWithoutNotify(AsyncValueState.PreLoading);
            var task = value(_cts);
            _stateContainers.UpdateValueWithoutNotify(AsyncValueState.Loading);
            try
            {
                _ = HandleTaskState(task);
            }
            catch (Exception e)
            {
                _stateContainers.UpdateValueWithoutNotify(AsyncValueState.Error);
                Debug.LogException(e, this);
            }
        }

        private async Task HandleTaskState(Task<T> task)
        {
            LoadedValue = await task;
            if (_cts.IsCancellationRequested)
                return;
            await UniTask.SwitchToMainThread();
            if (_cts.IsCancellationRequested)
                return;
            _stateContainers.UpdateValueWithoutNotify(AsyncValueState.Success);
            _valueContainer.UpdateValueWithoutNotify(LoadedValue);
            _stateContainers.UpdateValueWithoutNotify(AsyncValueState.ValueUpdated);
        }

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