using System;
using AYellowpaper.SerializedCollections;
using UnityEngine;

namespace DingoUnityExtensions.UnityViewProviders.Core
{
    public abstract class TabsContainer<TTabs> : ValueContainer<TTabs>
    {
        public event Action<TTabs> OnTabSelect
        {
            add => _tabsContainerGroup.OnTabSelect += value;
            remove => _tabsContainerGroup.OnTabSelect -= value;
        } 
        
        public event Action OnFullDeselect
        {
            add => _tabsContainerGroup.OnFullDeselect += value;
            remove => _tabsContainerGroup.OnFullDeselect -= value;
        }
        
        [SerializeField] private SerializedDictionary<TTabs, ValueContainer<bool>> _tabsToggles;
        
        private readonly TabsContainerGroup<TTabs> _tabsContainerGroup = new();
        
        private bool _initialized;

        protected override void SetValueWithoutNotify(TTabs value)
        {
            if (!_initialized)
                Initialize();
            _tabsContainerGroup.SetTabWithoutNotify(value);
        }

        protected override void OnAwake() => Initialize();

        private void Initialize()
        {
            if (_initialized)
                return;

            _tabsContainerGroup.Initialize(_tabsToggles);
            _initialized = true;
        }
    }
}