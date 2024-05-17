using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DingoUnityExtensions.Utils
{
    public class CoroutinesList
    {
        private readonly List<Coroutine> _coroutines;
        private readonly MonoBehaviour _parent;

        public CoroutinesList(MonoBehaviour parent)
        {
            _parent = parent;
            _coroutines = new List<Coroutine>();
        }

        public void AddAndStart(IEnumerator enumerator)
        {
            _coroutines.Add(_parent.StartCoroutine(enumerator));
        }

        public void AddCoroutineWithoutParentCheck(Coroutine coroutine)
        {
            if (coroutine == null)
                return;

            _coroutines.Add(coroutine);
        }

        public void InterruptCoroutines()
        {
            foreach (var coroutine in _coroutines.Where(coroutine => coroutine != null))
            {
                _parent.StopCoroutine(coroutine);
            }

            _coroutines.Clear();
        }
    }
}