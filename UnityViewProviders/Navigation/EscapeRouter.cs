using System;
using System.Collections.Generic;
using Bind;
using DingoUnityExtensions.MonoBehaviours.Singletons;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DingoUnityExtensions.UnityViewProviders.Navigation
{

    public enum EscapeResult
    {
        None,
        Closed,
        Block,
        Ignore,
        Skip
    }

    public class EscapeRouter : ProtectedSingletonBehaviour<EscapeRouter>
    {
        private struct Entry
        {
            public Func<EscapeResult> Action;
            public bool Removed;
        }

        private readonly List<Entry> _stack = new();
        private bool _processing;
        private Bind<InputActionPhase> _escape;

        public void SubscribeToEscape(Bind<InputActionPhase> escape)
        {
            _escape = escape;
            _escape.SafeSubscribe(OnEscapePerformed);
        }

        public void UnSubscribeFromEscape()
        {
            _escape?.UnSubscribe(OnEscapePerformed);
        }

        public static void AddAction(Func<EscapeResult> action) => GetNoCheck()?.AddActionInternal(action);
        public static void RemoveAction(Func<EscapeResult> action) => GetNoCheck()?.RemoveActionInternal(action);
        
        private void AddActionInternal(Func<EscapeResult> action)
        {
            if (action == null)
                return;
            _stack.Add(new Entry { Action = action, Removed = false });
        }

        private void RemoveActionInternal(Func<EscapeResult> action)
        {
            if (action == null)
                return;

            for (var i = _stack.Count - 1; i >= 0; --i)
            {
                if (_stack[i].Removed)
                    continue;
                if (_stack[i].Action != action)
                    continue;

                var e = _stack[i];
                e.Removed = true;
                _stack[i] = e;
                break;
            }

            if (!_processing)
                Compact();
        }

        public void TriggerEscape()
        {
            ProcessStack();
        }

        private void OnEscapePerformed(InputActionPhase inputActionPhase)
        {
            if (inputActionPhase == InputActionPhase.Canceled)
                ProcessStack();
        }

        private void ProcessStack()
        {
            if (_stack.Count == 0)
                return;

            _processing = true;

            for (var i = _stack.Count - 1; i >= 0; --i)
            {
                var entry = _stack[i];
                if (entry.Removed || entry.Action == null)
                    continue;

                EscapeResult r;
                try
                {
                    r = entry.Action();
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                    MarkRemoved(i);
                    continue;
                }

                if (r is EscapeResult.Ignore)
                    continue;
                
                if (r == EscapeResult.Skip)
                {
                    MarkRemoved(i);
                    continue;
                }

                if (r == EscapeResult.Closed)
                {
                    MarkRemoved(i);
                    break;
                }

                if (r == EscapeResult.Block)
                    break;
            }

            _processing = false;
            Compact();
        }

        private void MarkRemoved(int index)
        {
            var e = _stack[index];
            e.Removed = true;
            _stack[index] = e;
        }

        private void Compact()
        {
            for (var i = _stack.Count - 1; i >= 0; --i)
            {
                if (_stack[i].Removed || _stack[i].Action == null)
                    _stack.RemoveAt(i);
            }
        }
    }
}