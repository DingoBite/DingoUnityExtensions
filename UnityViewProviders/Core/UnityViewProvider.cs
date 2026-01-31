using System.Linq;
using UnityEngine;

namespace DingoUnityExtensions.UnityViewProviders.Core
{
    public abstract class UnityViewProvider<TView, TValue> : ValueContainer<TValue> where TView : Component
    {
        [SerializeField] private TView _view;

        protected TView View
        {
            get
            {
                if (_view == null)
                    _view = GetComponent<TView>();
                return _view;
            }
        }

        protected abstract override void SubscribeOnly();
        protected abstract override void UnsubscribeOnly();

        protected virtual void Reset() => _view = GetComponentInChildren<TView>();
    }

    public abstract class UnityViewProvider<TView> : EventContainer where TView : MonoBehaviour
    {
        [SerializeField] private TView _view;

        protected TView View
        {
            get
            {
                if (_view == null)
                    _view = GetComponent<TView>();
                return _view;
            }
        }

        protected abstract override void SubscribeOnly();
        protected abstract override void UnsubscribeOnly();
        
        protected virtual void Reset() => _view = GetComponentInChildren<TView>();
    }
}