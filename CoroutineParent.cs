using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DingoUnityExtensions.MonoBehaviours.Singletons;
using UnityEngine;
#if UNITASK_EXISTS
using Cysharp.Threading.Tasks;
#endif


namespace DingoUnityExtensions
{
    public struct TimeStamp
    {
        public readonly float TimeDelta;

        public TimeStamp(float timeDelta)
        {
            TimeDelta = timeDelta;
        }
    }
    
    public interface IUpdater
    {
        public void UpdateHandle(in TimeStamp timeStamp);
    }
    
    public interface ILateUpdater
    {
        public void LateUpdateHandle(in TimeStamp timeStamp);
    }
    
    public interface IFixedUpdater
    {
        public void FixedUpdateHandle(in TimeStamp timeStamp);
    }
    
    public class CoroutineParent : SingletonProtectedBehaviour<CoroutineParent>
    {
        private static readonly Dictionary<float, WaitForSeconds> WaitForSecondsMap = new ();

        private static readonly List<Action> CachedUpdatersDelegates = new ();
        private static readonly List<Action> CachedLateUpdatersDelegates = new ();
        private static readonly List<Action> CachedFixedUpdatersDelegates = new ();
        
        private static readonly List<IUpdater> CachedUpdaters = new ();
        private static readonly List<ILateUpdater> CachedLateUpdaters = new ();
        private static readonly List<IFixedUpdater> CachedFixedUpdaters = new ();
        
        private readonly Dictionary<object, (Action action, int order)> _updatersDelegates = new();
        private readonly Dictionary<object, (Action action, int order)> _lateUpdatersDelegates = new();
        private readonly Dictionary<object, (Action action, int order)> _fixedUpdatersDelegates = new();
        
        private readonly Dictionary<object, Coroutine> _actions = new();
        private readonly Dictionary<object, Coroutine> _coroutinesWithCanceling = new();

        private readonly Dictionary<object, (IUpdater updater, int order)> _updaters = new();
        private readonly Dictionary<object, (ILateUpdater updater, int order)> _lateUpdaters = new();
        private readonly Dictionary<object, (IFixedUpdater updater, int order)> _fixedUpdaters = new();

        private readonly List<(Action action, int order)> _singleUpdateActions = new();
        private readonly List<(Action action, int order)> _singleLateUpdateActions = new();
        private readonly List<(Action action, int order)> _singleFixedUpdateActions = new();
        
        public static WaitForSeconds CachedWaiter(float seconds)
        {
            seconds = (float) Math.Round(seconds, 6);
            if (WaitForSecondsMap.TryGetValue(seconds, out var waitForSeconds))
                return waitForSeconds;

            waitForSeconds = new WaitForSeconds(seconds);
            WaitForSecondsMap[seconds] = waitForSeconds;
            return waitForSeconds;
        }

        public static void AddUpdater(object obj, Action updateAction, int order = 0) => Instance._updatersDelegates[obj] = (updateAction, order);
        public static void RemoveUpdater(object obj) => Instance._updatersDelegates.Remove(obj);
        public static void AddLateUpdater(object obj, Action updateAction, int order = 0) => Instance._lateUpdatersDelegates[obj] = (updateAction, order);
        public static void RemoveLateUpdater(object obj) => Instance._lateUpdatersDelegates.Remove(obj);
        public static void AddFixedUpdater(object obj, Action updateAction, int order = 0) => Instance._fixedUpdatersDelegates[obj] = (updateAction, order);
        public static void RemoveFixedUpdater(object obj) => Instance._fixedUpdatersDelegates.Remove(obj);
        
        public static void AddUpdater(IUpdater updater) => AddUpdater(updater, 0);
        public static void AddUpdater(IUpdater updater, int order) => Instance._updaters[updater] = (updater, order);
        public static void RemoveUpdater(IUpdater updater) => Instance._updaters.Remove(updater);

        public static void AddLateUpdater(ILateUpdater updater) => AddLateUpdater(updater, 0);
        public static void AddLateUpdater(ILateUpdater updater, int order) => Instance._lateUpdaters[updater] = (updater, order);
        public static void RemoveLateUpdater(ILateUpdater updater) => Instance._lateUpdaters.Remove(updater);
        
        public static void AddFixedUpdater(IFixedUpdater updater) => AddFixedUpdater(updater, 0);
        public static void AddFixedUpdater(IFixedUpdater updater, int order) => Instance._fixedUpdaters[updater] = (updater, order);
        public static void RemoveFixedUpdater(IFixedUpdater updater) => Instance._fixedUpdaters.Remove(updater);

        public static Action AddSingleUpdate(Action action) => AddSingleUpdate(action, 0);
        public static Action AddSingleUpdate(Action action, int order)
        {
            var tuple = (action, order);
            Instance._singleUpdateActions.Add(tuple);
            return () => Instance._singleUpdateActions.Remove(tuple);
        }
        
        public static Action AddSingleLateUpdate(Action action) => AddSingleLateUpdate(action, 0);
        public static Action AddSingleLateUpdate(Action action, int order)
        {
            var tuple = (action, order);
            Instance._singleLateUpdateActions.Add(tuple);
            return () => Instance._singleLateUpdateActions.Remove(tuple);
        }
        
        public static Action AddSingleFixedUpdate(Action action) => AddSingleFixedUpdate(action, 0);
        public static Action AddSingleFixedUpdate(Action action, int order)
        {
            var tuple = (action, order);
            Instance._singleFixedUpdateActions.Add(tuple);
            return () => Instance._singleFixedUpdateActions.Remove(tuple);
        }
        
#if UNITASK_EXISTS
        public static Coroutine YieldTaskCoroutine<T>(UniTask<T> task, Action<T> resultHandler = null, Action<Exception> exceptionHandler = null) => Instance.StartCoroutine(task.ToCoroutine(resultHandler, exceptionHandler));
        public static Coroutine YieldTaskCoroutine(UniTask task, Action<Exception> exceptionHandler = null) => Instance.StartCoroutine(task.ToCoroutine(exceptionHandler));
        public static Coroutine YieldTaskCoroutine(Task task, Action<Exception> exceptionHandler = null) => YieldTaskCoroutine(task.AsUniTask(), exceptionHandler);
        public static Coroutine YieldTaskCoroutine<T>(Task<T> task, Action<T> resultHandler = null, Action<Exception> exceptionHandler = null) => YieldTaskCoroutine(task.AsUniTask(), resultHandler, exceptionHandler);
#endif

        public static Coroutine StartCoroutineWithCanceling(object key, Func<IEnumerator> factory)
        {
            if (Instance._coroutinesWithCanceling.TryGetValue(key, out var coroutine) && coroutine != null)
                Instance.StopCoroutine(coroutine);
            coroutine = Instance.StartCoroutine(factory());
            Instance._coroutinesWithCanceling[key] = coroutine;
            return coroutine;
        }

        public static void CancelCoroutine(object key)
        {
            if (Instance._coroutinesWithCanceling.TryGetValue(key, out var coroutine) && coroutine != null)
                Instance.StopCoroutine(coroutine);
        }
        
        public static Coroutine InvokeAfterSecondsWithCanceling(object sender, float seconds, Action action)
        {
            if (Instance._actions.TryGetValue(sender, out var coroutine) && coroutine != null)
                Instance.StopCoroutine(coroutine);
            coroutine = InvokeAfterSeconds(seconds, action);
            Instance._actions[sender] = coroutine;
            return coroutine;
        }
        
        public static Coroutine InvokeAfterAsyncMethodWithCanceling<T>(object sender, Func<Task<T>> asyncAction, Action<Task<T>> action)
        {
            if (Instance._actions.TryGetValue(sender, out var coroutine) && coroutine != null)
                Instance.StopCoroutine(coroutine);
            coroutine = InvokeAfterAsyncMethod(asyncAction, action);
            Instance._actions[sender] = coroutine;
            return coroutine;
        }
        
        public static Coroutine InvokeAfterAsyncMethodWithCanceling(object sender, Func<Task> asyncAction, Action action)
        {
            if (Instance._actions.TryGetValue(sender, out var coroutine) && coroutine != null)
                Instance.StopCoroutine(coroutine);
            coroutine = InvokeAfterAsyncMethod(asyncAction, action);
            Instance._actions[sender] = coroutine;
            return coroutine;
        }
        
        public static Coroutine InvokeAfterAsyncMethod<T>(Func<Task<T>> asyncAction, Action<Task<T>> action)
        {
            var task = asyncAction.Invoke();
            return Instance.StartCoroutine(WaitAndInvokeC(new WaitUntil(() => task.IsCompleted), () => action(task)));
        }
        
        public static Coroutine InvokeAfterAsyncMethod(Func<Task> asyncAction, Action action)
        {
            var task = asyncAction.Invoke();
            return Instance.StartCoroutine(WaitAndInvokeC(new WaitUntil(() => task.IsCompleted), action));
        }

        public static Coroutine InvokeAfterSeconds(float seconds, Action action)
        {
            if (seconds < 0)
                return null;
            if (seconds <= Vector2.kEpsilon)
            {
                action?.Invoke();
                return null;
            }
            return Instance.StartCoroutine(WaitAndInvokeC(CachedWaiter(seconds), action));
        }

        public static Coroutine WaitAndInvoke(IEnumerator yieldInstruction, Action action)
        {
            return Instance.StartCoroutine(WaitAndInvokeC(yieldInstruction, action));
        }
        
        public static Coroutine WaitAndInvoke(YieldInstruction yieldInstruction, Action action)
        {
            return Instance.StartCoroutine(WaitAndInvokeC(yieldInstruction, action));
        }       
        
        public static IEnumerator WaitAndInvokeC(YieldInstruction yieldInstruction, Action action)
        {
            yield return yieldInstruction;
            action?.Invoke();
        }       
        
        public static IEnumerator WaitAndInvokeC(IEnumerator yieldInstruction, Action action)
        {
            yield return Instance.StartCoroutine(yieldInstruction);
            action?.Invoke();
        }

        private void Update()
        {
            if (_updatersDelegates.Count != 0)
            {
                CachedUpdatersDelegates.Clear();
                CachedUpdatersDelegates.AddRange(_updatersDelegates.Values.OrderBy(e => e.order).Select(e => e.action));
                foreach (var updater in CachedUpdatersDelegates)
                {
                    try
                    {
                        updater();
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
            }
            
            if (_updaters.Count != 0)
            {
                CachedUpdaters.Clear();
                CachedUpdaters.AddRange(_updaters.Values.OrderBy(e => e.order).Select(e => e.updater));
                var timeStamp = new TimeStamp(Time.deltaTime);
                foreach (var updater in CachedUpdaters)
                {
                    try
                    {
                        updater.UpdateHandle(timeStamp);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
            }

            if (_singleUpdateActions.Count != 0)
            {
                foreach (var updateAction in _singleUpdateActions.OrderBy(p => p.order).Select(p => p.action))
                {
                    try
                    {
                        updateAction?.Invoke();
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
                _singleUpdateActions.Clear();
            }
        }

        private void LateUpdate()
        {
            if (_lateUpdatersDelegates.Count != 0)
            {
                CachedLateUpdatersDelegates.Clear();
                CachedLateUpdatersDelegates.AddRange(_lateUpdatersDelegates.Values.OrderBy(e => e.order).Select(e => e.action));
                foreach (var lateUpdater in CachedLateUpdatersDelegates)
                {
                    try
                    {
                        lateUpdater();
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
            }

            if (_lateUpdaters.Count != 0)
            {
                CachedLateUpdaters.Clear();
                CachedLateUpdaters.AddRange(_lateUpdaters.Values.OrderBy(e => e.order).Select(e => e.updater));
                var timeStamp = new TimeStamp(Time.deltaTime);
                foreach (var updater in CachedLateUpdaters)
                {
                    try
                    {
                        updater.LateUpdateHandle(timeStamp);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
            }
            
            if (_singleLateUpdateActions.Count != 0)
            {
                foreach (var updateAction in _singleLateUpdateActions.OrderBy(p => p.order).Select(p => p.action))
                {
                    try
                    {
                        updateAction?.Invoke();
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
                _singleLateUpdateActions.Clear();
            }
        }
        
        private void FixedUpdate()
        {
            if (_fixedUpdatersDelegates.Count != 0)
            {
                CachedFixedUpdatersDelegates.Clear();
                CachedFixedUpdatersDelegates.AddRange(_fixedUpdatersDelegates.Values.OrderBy(e => e.order).Select(e => e.action));
                foreach (var fixedUpdater in CachedFixedUpdatersDelegates)
                {
                    try
                    {
                        fixedUpdater();
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
            }
            
            if (_fixedUpdaters.Count != 0)
            {
                CachedFixedUpdaters.Clear();
                CachedFixedUpdaters.AddRange(_fixedUpdaters.Values.OrderBy(e => e.order).Select(e => e.updater));
                var timeStamp = new TimeStamp(Time.deltaTime);
                foreach (var updater in CachedFixedUpdaters)
                {
                    try
                    {
                        updater.FixedUpdateHandle(timeStamp);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
            }
            
            if (_singleFixedUpdateActions.Count != 0)
            {
                foreach (var updateAction in _singleFixedUpdateActions.OrderBy(p => p.order).Select(p => p.action))
                {
                    try
                    {
                        updateAction?.Invoke();
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
                _singleFixedUpdateActions.Clear();
            }
        }
    }
}