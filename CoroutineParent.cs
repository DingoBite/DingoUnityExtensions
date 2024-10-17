using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using DingoUnityExtensions.MonoBehaviours;
using DingoUnityExtensions.MonoBehaviours.Singletons;
using UnityEngine;
#if UNITASK_EXISTS
using Cysharp.Threading.Tasks;
#endif


namespace DingoUnityExtensions
{
    public class CoroutineParent : SingletonBehaviour<CoroutineParent>
    {
        private static readonly Dictionary<float, WaitForSeconds> WaitForSecondsMap = new ();

        private static readonly List<Action> CachedUpdaters = new ();
        private static readonly List<Action> CachedLateUpdaters = new ();
        
        private readonly Dictionary<object, Action> _updaters = new();
        private readonly Dictionary<object, Action> _lateUpdaters = new();
        private readonly Dictionary<object, Coroutine> _actions = new();
        private readonly Dictionary<object, Coroutine> _coroutinesWithCanceling = new();
        
        public static WaitForSeconds CachedWaiter(float seconds)
        {
            seconds = (float) Math.Round(seconds, 6);
            if (WaitForSecondsMap.TryGetValue(seconds, out var waitForSeconds))
                return waitForSeconds;

            waitForSeconds = new WaitForSeconds(seconds);
            WaitForSecondsMap[seconds] = waitForSeconds;
            return waitForSeconds;
        }

        public static void AddLateUpdater(object obj, Action updateAction) => Instance._lateUpdaters[obj] = updateAction;
        public static void RemoveLateUpdater(object obj) => Instance._lateUpdaters.Remove(obj);
        public static void AddUpdater(object obj, Action updateAction) => Instance._updaters[obj] = updateAction;
        public static void RemoveUpdater(object obj) => Instance._updaters.Remove(obj);

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
            if (_updaters.Count == 0)
                return;
            CachedUpdaters.Clear();
            CachedUpdaters.AddRange(_updaters.Values);
            foreach (var updater in CachedUpdaters)
            {
                updater();
            }
        }

        private void LateUpdate()
        {
            if (_updaters.Count == 0)
                return;
            CachedLateUpdaters.Clear();
            CachedLateUpdaters.AddRange(_lateUpdaters.Values);
            foreach (var lateUpdater in CachedLateUpdaters)
            {
                lateUpdater();
            }
        }
    }
}