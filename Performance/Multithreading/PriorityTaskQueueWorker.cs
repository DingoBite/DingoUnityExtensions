using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DingoUnityExtensions.Performance.Multithreading
{
    public class PriorityTaskQueueWorker : IDisposable
    {
        private readonly ConcurrentDictionary<int, ConcurrentQueue<Func<Task>>> _priorityTaskQueues = new();
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private readonly List<Task> _backgroundTasks = new();
        private readonly ConcurrentBag<Task> _runningTasks = new();
        private readonly int _delayEachTask;

        public IReadOnlyCollection<Task> RunningTasks => _runningTasks;

        public PriorityTaskQueueWorker(int count = 4, int delayEachTask = 100)
        {
            _delayEachTask = delayEachTask;
            for (var i = 0; i < count; i++)
            {
                var task = Task.Factory.StartNew(ProcessQueueAsync, _cancellationTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
                _backgroundTasks.Add(task);
            }
        }

        public void EnqueueTask(Func<Task> taskGenerator, int priority = 0)
        {
            if (!_priorityTaskQueues.ContainsKey(priority))
            {
                _priorityTaskQueues[priority] = new ConcurrentQueue<Func<Task>>();
            }

            _priorityTaskQueues[priority].Enqueue(taskGenerator);
        }

        private async Task ProcessQueueAsync()
        {
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                var taskFound = false;
            
                foreach (var priority in _priorityTaskQueues.Keys.OrderByDescending(p => p))
                {
                    if (_priorityTaskQueues[priority].TryDequeue(out var taskGenerator))
                    {
                        taskFound = true;
                        try
                        {
                            var task = taskGenerator();
                            _runningTasks.Add(task);
                            await task;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"Exception: {e}");
                        }
                    }

                    if (taskFound)
                        break;
                }

                if (!taskFound)
                {
                    await Task.Delay(_delayEachTask);
                }
            }
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
        }
    }
}