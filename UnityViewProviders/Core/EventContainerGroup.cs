using System;
using System.Collections.Generic;
using System.Linq;

namespace DingoUnityExtensions.UnityViewProviders.Core
{
    public class ButtonContainerGroup<TId> : EventContainerGroup<TId, EventContainer> { }
    
    public class EventContainerGroup<TId, TEventContainer> where TEventContainer : EventContainer
    {
        public event Action<TId> OnClick;

        private readonly Dictionary<TId, TEventContainer> _eventContainers = new();
        private readonly Dictionary<TId, Action> _eventById = new();

        public IEnumerable<TEventContainer> GetButtons() => _eventContainers.Values;
        public TEventContainer GetButton(TId id) => _eventContainers.GetValueOrDefault(id, null);

        public void Initialize(IEnumerable<(TId, TEventContainer)> eventContainers)
        {
            Clear();
            foreach (var (id, eventContainer) in eventContainers)
            {
                _eventContainers.Add(id, eventContainer);
            }

            SubscribeOnly();
        }
        
        public void Initialize(IEnumerable<KeyValuePair<TId, TEventContainer>> eventContainers)
        {
            Clear();
            foreach (var (id, eventContainer) in eventContainers)
            {
                _eventContainers.Add(id, eventContainer);
            }

            SubscribeOnly();
        }
        
        public void Initialize(IReadOnlyDictionary<TId, TEventContainer> eventContainers)
        {
            Clear();
            foreach (var (id, eventContainer) in eventContainers)
            {
                _eventContainers.Add(id, eventContainer);
            }

            SubscribeOnly();
        }
        
        public void Clear()
        {
            UnsubscribeOnly();
            _eventContainers.Clear();
            _eventById.Clear();
        }
        
        private void OnButtonClick(TId id)
        {
            OnClick?.Invoke(id);
        }
        
        protected void SubscribeOnly()
        {
            foreach (var (id, eventContainer) in _eventContainers)
            {
                if (!_eventById.TryGetValue(id, out var e))
                {
                    e = () => OnButtonClick(id);
                    _eventById[id] = e;
                }
                eventContainer.OnEvent -= e;
                eventContainer.OnEvent += e;
            }
        }

        protected void UnsubscribeOnly()
        {
            foreach (var (id, eventContainer) in _eventContainers.Where(p => p.Value != null))
            {
                if (!_eventById.TryGetValue(id, out var e))
                {
                    continue;
                }
                eventContainer.OnEvent -= e;
            }
        }
    }
}