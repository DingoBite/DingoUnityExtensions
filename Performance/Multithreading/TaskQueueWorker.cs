using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace DingoUnityExtensions.Performance.Multithreading
{
    public class TaskQueueWorker : IDisposable
    {
        private readonly ConcurrentQueue<(Func<Task> factory, Action disposeAction)> _taskQueue = new();
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private readonly List<Task> _backgroundTasks = new();

        private readonly ConcurrentBag<Task> _runningTasks = new();
        private readonly int _delayEachTask;

        public IReadOnlyCollection<Task> RunningTasks => _runningTasks;

        public TaskQueueWorker(int count = 4, int delayEachTask = 100)
        {
            _delayEachTask = delayEachTask;
            for (var i = 0; i < count; i++)
            {
                var task = Task.Factory.StartNew(ProcessQueueAsync, _cancellationTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
                _backgroundTasks.Add(task);
            }
        }

        public void EnqueueTask((Func<Task>, Action disposeAction) taskGenerator)
        {
            _taskQueue.Enqueue(taskGenerator);
        }
        
        public void EnqueueTask(Func<Task> taskGenerator)
        {
            _taskQueue.Enqueue((taskGenerator, null));
        }

        private async Task ProcessQueueAsync()
        {
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                while (_taskQueue.TryDequeue(out var pair))
                {
                    var disposeAction = pair.disposeAction;
                    try
                    {
                        var task = pair.factory();
                        _runningTasks.Add(task);
                        await task;
                    }
                    catch (Exception e)
                    {
                        disposeAction?.Invoke();
                        Debug.LogException(e);
                    }
                }

                await Task.Delay(_delayEachTask);
            }

            while (_taskQueue.TryDequeue(out var pair))
            {
                pair.disposeAction?.Invoke();
            }
        }

        public void Dispose()
        {
            if (!_cancellationTokenSource.IsCancellationRequested)
                _cancellationTokenSource.Cancel();
            try
            {
                _cancellationTokenSource.Dispose();
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}