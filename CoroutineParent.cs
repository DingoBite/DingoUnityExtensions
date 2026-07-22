using System;
using System.Collections;
using System.Collections.Generic;
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
    
    public static class CoroutineOrderLayers
    {
        public const int MIN_PRIORITY_SPECIAL = int.MinValue + 1;
        public const int MIN_PRIORITY = int.MinValue + 10;
        public const int DEFAULT_PRIORITY = 0;
        public const int MAX_PRIORITY = int.MaxValue - 10;
        public const int MAX_PRIORITY_SPECIAL = int.MaxValue - 1;
    }
    
    public class CoroutineParent : ProtectedSingletonBehaviour<CoroutineParent>
    {
        private enum UpdatePhase : byte
        {
            Update,
            LateUpdate,
            FixedUpdate,
        }

        private enum CallbackKind : byte
        {
            Delegate,
            Interface,
            Single,
        }

        private readonly struct RegistrationKey : IEquatable<RegistrationKey>
        {
            public readonly CallbackKind Kind;
            public readonly object Owner;

            public RegistrationKey(CallbackKind kind, object owner)
            {
                Kind = kind;
                Owner = owner;
            }

            public bool Equals(RegistrationKey other)
            {
                return Kind == other.Kind && EqualityComparer<object>.Default.Equals(Owner, other.Owner);
            }

            public override bool Equals(object obj)
            {
                return obj is RegistrationKey other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((int)Kind * 397) ^ (Owner == null ? 0 : EqualityComparer<object>.Default.GetHashCode(Owner));
                }
            }
        }

        private sealed class OrderedUpdateLane
        {
            private sealed class Entry
            {
                public RegistrationKey Key;
                public Action Action;
                public object Updater;
                public int Order;
                public long Sequence;
                public CallbackKind Kind;
                public bool IsPersistent;
                public bool Active;
            }

            private sealed class EntryComparer : IComparer<Entry>
            {
                public static readonly EntryComparer Instance = new();

                public int Compare(Entry x, Entry y)
                {
                    if (ReferenceEquals(x, y))
                        return 0;
                    if (x == null)
                        return 1;
                    if (y == null)
                        return -1;

                    var byOrder = y.Order.CompareTo(x.Order);
                    if (byOrder != 0)
                        return byOrder;

                    var byKind = x.Kind.CompareTo(y.Kind);
                    if (byKind != 0)
                        return byKind;

                    return x.Sequence.CompareTo(y.Sequence);
                }
            }

            private readonly UpdatePhase _phase;
            private readonly Dictionary<RegistrationKey, Entry> _persistent = new();
            private readonly List<Entry> _persistentCache = new();
            private readonly List<Entry> _pendingSingles = new();
            private readonly List<Entry> _singleBuffer = new();
            private readonly List<Entry> _executionQueue = new();
            private readonly HashSet<RegistrationKey> _executedPersistentKeys = new();

            private bool _persistentCacheDirty;
            private bool _executing;
            private bool _executingSingle;
            private int _executionIndex;
            private int _currentOrder;
            private long _nextSequence;

            public OrderedUpdateLane(UpdatePhase phase)
            {
                _phase = phase;
            }

            public void AddDelegate(object owner, Action action, int order)
            {
                if (owner == null)
                    throw new ArgumentNullException(nameof(owner));
                AddPersistent(new RegistrationKey(CallbackKind.Delegate, owner), action, null, order);
            }

            public void AddInterface(object updater, int order)
            {
                if (updater == null)
                    throw new ArgumentNullException(nameof(updater));
                AddPersistent(new RegistrationKey(CallbackKind.Interface, updater), null, updater, order);
            }

            public void RemoveDelegate(object owner)
            {
                if (owner == null)
                    throw new ArgumentNullException(nameof(owner));
                RemovePersistent(new RegistrationKey(CallbackKind.Delegate, owner));
            }

            public void RemoveInterface(object updater)
            {
                if (updater == null)
                    throw new ArgumentNullException(nameof(updater));
                RemovePersistent(new RegistrationKey(CallbackKind.Interface, updater));
            }

            public Action AddSingle(Action action, int order)
            {
                var entry = new Entry
                {
                    Action = action,
                    Order = order,
                    Sequence = NextSequence(),
                    Kind = CallbackKind.Single,
                    IsPersistent = false,
                    Active = true,
                };

                if (_executing && !_executingSingle && order <= _currentOrder)
                    InsertIntoCurrentTail(entry);
                else
                    _pendingSingles.Add(entry);

                return () => entry.Active = false;
            }

            public void Tick(float deltaTime)
            {
                PreparePersistentCache();
                PrepareExecutionQueue();

                _executedPersistentKeys.Clear();
                _executing = true;
                _executionIndex = 0;

                try
                {
                    while (_executionIndex < _executionQueue.Count)
                    {
                        var entry = _executionQueue[_executionIndex++];
                        if (!entry.Active)
                            continue;

                        if (entry.IsPersistent && !_executedPersistentKeys.Add(entry.Key))
                            continue;

                        _currentOrder = entry.Order;
                        _executingSingle = !entry.IsPersistent;
                        if (_executingSingle)
                            entry.Active = false;

                        try
                        {
                            Invoke(entry, deltaTime);
                        }
                        catch (Exception exception)
                        {
                            Debug.LogException(exception);
                        }
                        finally
                        {
                            _executingSingle = false;
                        }
                    }
                }
                finally
                {
                    _executing = false;
                    _executingSingle = false;
                    _executionQueue.Clear();
                    _singleBuffer.Clear();
                }
            }

            private void AddPersistent(RegistrationKey key, Action action, object updater, int order)
            {
                long sequence;
                if (_persistent.TryGetValue(key, out var current))
                {
                    var sameCallback = key.Kind == CallbackKind.Delegate
                        ? Equals(current.Action, action)
                        : ReferenceEquals(current.Updater, updater);
                    if (current.Active && current.Order == order && sameCallback)
                        return;

                    current.Active = false;
                    sequence = current.Sequence;
                }
                else
                {
                    sequence = NextSequence();
                }

                var entry = new Entry
                {
                    Key = key,
                    Action = action,
                    Updater = updater,
                    Order = order,
                    Sequence = sequence,
                    Kind = key.Kind,
                    IsPersistent = true,
                    Active = true,
                };

                _persistent[key] = entry;
                _persistentCacheDirty = true;

                if (_executing && order <= _currentOrder && !_executedPersistentKeys.Contains(key))
                    InsertIntoCurrentTail(entry);
            }

            private void RemovePersistent(RegistrationKey key)
            {
                if (!_persistent.TryGetValue(key, out var entry))
                    return;

                _persistent.Remove(key);
                entry.Active = false;
                _persistentCacheDirty = true;
            }

            private void PreparePersistentCache()
            {
                if (!_persistentCacheDirty)
                    return;

                _persistentCache.Clear();
                foreach (var entry in _persistent.Values)
                {
                    if (entry.Active)
                        _persistentCache.Add(entry);
                }

                if (_persistentCache.Count > 1)
                    _persistentCache.Sort(EntryComparer.Instance);
                _persistentCacheDirty = false;
            }

            private void PrepareExecutionQueue()
            {
                _executionQueue.Clear();
                _singleBuffer.Clear();

                for (var i = 0; i < _pendingSingles.Count; i++)
                {
                    var entry = _pendingSingles[i];
                    if (entry.Active)
                        _singleBuffer.Add(entry);
                }
                _pendingSingles.Clear();

                if (_singleBuffer.Count == 0)
                {
                    _executionQueue.AddRange(_persistentCache);
                    return;
                }

                if (_singleBuffer.Count > 1)
                    _singleBuffer.Sort(EntryComparer.Instance);

                var persistentIndex = 0;
                var singleIndex = 0;
                while (persistentIndex < _persistentCache.Count && singleIndex < _singleBuffer.Count)
                {
                    if (EntryComparer.Instance.Compare(_persistentCache[persistentIndex], _singleBuffer[singleIndex]) <= 0)
                        _executionQueue.Add(_persistentCache[persistentIndex++]);
                    else
                        _executionQueue.Add(_singleBuffer[singleIndex++]);
                }

                while (persistentIndex < _persistentCache.Count)
                    _executionQueue.Add(_persistentCache[persistentIndex++]);
                while (singleIndex < _singleBuffer.Count)
                    _executionQueue.Add(_singleBuffer[singleIndex++]);
            }

            private void InsertIntoCurrentTail(Entry entry)
            {
                var index = _executionIndex;
                while (index < _executionQueue.Count && EntryComparer.Instance.Compare(_executionQueue[index], entry) <= 0)
                    index++;
                _executionQueue.Insert(index, entry);
            }

            private void Invoke(Entry entry, float deltaTime)
            {
                if (entry.Kind == CallbackKind.Delegate)
                {
                    entry.Action();
                    return;
                }
                if (entry.Kind == CallbackKind.Single)
                {
                    entry.Action?.Invoke();
                    return;
                }

                var timeStamp = new TimeStamp(deltaTime);
                switch (_phase)
                {
                    case UpdatePhase.Update:
                        ((IUpdater)entry.Updater).UpdateHandle(in timeStamp);
                        break;
                    case UpdatePhase.LateUpdate:
                        ((ILateUpdater)entry.Updater).LateUpdateHandle(in timeStamp);
                        break;
                    case UpdatePhase.FixedUpdate:
                        ((IFixedUpdater)entry.Updater).FixedUpdateHandle(in timeStamp);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            private long NextSequence()
            {
                return ++_nextSequence;
            }
        }

        private static readonly Dictionary<float, WaitForSeconds> WaitForSecondsMap = new ();

        private readonly OrderedUpdateLane _updateLane = new(UpdatePhase.Update);
        private readonly OrderedUpdateLane _lateUpdateLane = new(UpdatePhase.LateUpdate);
        private readonly OrderedUpdateLane _fixedUpdateLane = new(UpdatePhase.FixedUpdate);
        
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

        public static void AddUpdater(object obj, Action updateAction, int order = 0) => Instance._updateLane.AddDelegate(obj, updateAction, order);
        public static void RemoveUpdater(object obj) => GetNoCheck()?._updateLane.RemoveDelegate(obj);
        public static void AddLateUpdater(object obj, Action updateAction, int order = 0) => Instance._lateUpdateLane.AddDelegate(obj, updateAction, order);
        public static void RemoveLateUpdater(object obj) => GetNoCheck()?._lateUpdateLane.RemoveDelegate(obj);
        public static void AddFixedUpdater(object obj, Action updateAction, int order = 0) => Instance._fixedUpdateLane.AddDelegate(obj, updateAction, order);
        public static void RemoveFixedUpdater(object obj) => GetNoCheck()?._fixedUpdateLane.RemoveDelegate(obj);
        
        public static void AddUpdater(IUpdater updater) => AddUpdater(updater, 0);
        public static void AddUpdater(IUpdater updater, int order) => Instance._updateLane.AddInterface(updater, order);
        public static void RemoveUpdater(IUpdater updater) => GetNoCheck()?._updateLane.RemoveInterface(updater);

        public static void AddLateUpdater(ILateUpdater updater) => AddLateUpdater(updater, 0);
        public static void AddLateUpdater(ILateUpdater updater, int order) => Instance._lateUpdateLane.AddInterface(updater, order);
        public static void RemoveLateUpdater(ILateUpdater updater) => GetNoCheck()?._lateUpdateLane.RemoveInterface(updater);
        
        public static void AddFixedUpdater(IFixedUpdater updater) => AddFixedUpdater(updater, 0);
        public static void AddFixedUpdater(IFixedUpdater updater, int order) => Instance._fixedUpdateLane.AddInterface(updater, order);
        public static void RemoveFixedUpdater(IFixedUpdater updater) => GetNoCheck()?._fixedUpdateLane.RemoveInterface(updater);

        public static Action AddSingleUpdate(Action action) => AddSingleUpdate(action, 0);
        public static Action AddSingleUpdate(Action action, int order)
        {
            return Instance._updateLane.AddSingle(action, order);
        }
        
        public static Action AddSingleLateUpdate(Action action) => AddSingleLateUpdate(action, 0);
        public static Action AddSingleLateUpdate(Action action, int order)
        {
            return Instance._lateUpdateLane.AddSingle(action, order);
        }
        
        public static Action AddSingleFixedUpdate(Action action) => AddSingleFixedUpdate(action, 0);
        public static Action AddSingleFixedUpdate(Action action, int order)
        {
            return Instance._fixedUpdateLane.AddSingle(action, order);
        }
        
#if UNITASK_EXISTS
        public static Coroutine YieldTaskCoroutine<T>(UniTask<T> task, Action<T> resultHandler = null, Action<Exception> exceptionHandler = null) => StartCoroutineOnInstance(task.ToCoroutine(resultHandler, exceptionHandler));
        public static Coroutine YieldTaskCoroutine(UniTask task, Action<Exception> exceptionHandler = null) => StartCoroutineOnInstance(task.ToCoroutine(exceptionHandler));
        public static Coroutine YieldTaskCoroutine(Task task, Action<Exception> exceptionHandler = null) => YieldTaskCoroutine(task.AsUniTask(), exceptionHandler);
        public static Coroutine YieldTaskCoroutine<T>(Task<T> task, Action<T> resultHandler = null, Action<Exception> exceptionHandler = null) => YieldTaskCoroutine(task.AsUniTask(), resultHandler, exceptionHandler);
#endif

        public static Coroutine StartCoroutineOnInstance(IEnumerator coroutineEnumerator) => Instance.StartCoroutine(coroutineEnumerator);

        public static void CancelCoroutine(Coroutine coroutine)
        {
            if (Instance == null)
                return;
            if (coroutine != null)  
                Instance.StopCoroutine(coroutine);
        }

        public static Coroutine StartCoroutineWithCanceling(object key, IEnumerator coroutineEnumerator)
        {
            if (Instance == null)
                return default;
            if (Instance._coroutinesWithCanceling.TryGetValue(key, out var coroutine) && coroutine != null)
                Instance.StopCoroutine(coroutine);
            coroutine = StartCoroutineOnInstance(coroutineEnumerator);
            Instance._coroutinesWithCanceling[key] = coroutine;
            return coroutine;
        }

        public static Coroutine StartCoroutineWithCanceling(object key, Func<IEnumerator> factory)
        {
            if (Instance == null)
                return default;
            if (Instance._coroutinesWithCanceling.TryGetValue(key, out var coroutine) && coroutine != null)
                Instance.StopCoroutine(coroutine);
            coroutine = StartCoroutineOnInstance(factory());
            Instance._coroutinesWithCanceling[key] = coroutine;
            return coroutine;
        }

        public static void CancelCoroutine(object key)
        {
            if (Instance == null)
                return;
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

        public static Coroutine InvokeAfterFrameWithCancelling(object sender, int frames, Action action)
        {
            if (Instance._actions.TryGetValue(sender, out var coroutine) && coroutine != null)
                Instance.StopCoroutine(coroutine);
            coroutine = WaitFramesAndInvoke(frames, action);
            Instance._actions[sender] = coroutine;
            return coroutine;
        }
        
        public static Coroutine InvokeAfterAsyncMethodWithCanceling<T>(object sender, Func<Task<T>> asyncAction, Action<T> action)
        {
            if (Instance._actions.TryGetValue(sender, out var coroutine) && coroutine != null)
                Instance.StopCoroutine(coroutine);
            coroutine = InvokeAfterAsyncMethod(asyncAction, action);
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
        
        public static Coroutine InvokeAfterAsyncMethod<T>(Func<Task<T>> asyncAction, Action<T> action)
        {
            var task = asyncAction.Invoke();
            return StartCoroutineOnInstance(WaitAndInvokeC(new WaitUntil(() => task.IsCompleted), () => action(task.Result)));
        }
        
        public static Coroutine InvokeAfterAsyncMethod<T>(Func<Task<T>> asyncAction, Action<Task<T>> action)
        {
            var task = asyncAction.Invoke();
            return StartCoroutineOnInstance(WaitAndInvokeC(new WaitUntil(() => task.IsCompleted), () => action(task)));
        }
        
        public static Coroutine InvokeAfterAsyncMethod(Func<Task> asyncAction, Action action)
        {
            var task = asyncAction.Invoke();
            return StartCoroutineOnInstance(WaitAndInvokeC(new WaitUntil(() => task.IsCompleted), action));
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
            return StartCoroutineOnInstance(WaitAndInvokeC(CachedWaiter(seconds), action));
        }

        public static Coroutine WaitAndInvoke(IEnumerator yieldInstruction, Action action)
        {
            return StartCoroutineOnInstance(WaitAndInvokeC(yieldInstruction, action));
        }
        
        public static Coroutine WaitFramesAndInvoke(int frames, Action action)
        {
            return StartCoroutineOnInstance(WaitFramesAndInvokeC(frames, action));
        }

        public static Coroutine WaitAndInvoke(YieldInstruction yieldInstruction, Action action)
        {
            return StartCoroutineOnInstance(WaitAndInvokeC(yieldInstruction, action));
        }

        public static IEnumerator WaitFramesAndInvokeC(int frames, Action action)
        {
            for (var i = 0; i < frames; i++)
            {
                yield return null;
            }
            action?.Invoke();
        }       
        
        public static IEnumerator WaitAndInvokeC(YieldInstruction yieldInstruction, Action action)
        {
            yield return yieldInstruction;
            action?.Invoke();
        }

        public static IEnumerator WaitAndInvokeC(IEnumerator yieldInstruction, Action action)
        {
            yield return StartCoroutineOnInstance(yieldInstruction);
            action?.Invoke();
        }

        private void Update()
        {
            _updateLane.Tick(Time.deltaTime);
        }

        private void LateUpdate()
        {
            _lateUpdateLane.Tick(Time.deltaTime);
        }
        
        private void FixedUpdate()
        {
            _fixedUpdateLane.Tick(Time.deltaTime);
        }
    }
}
