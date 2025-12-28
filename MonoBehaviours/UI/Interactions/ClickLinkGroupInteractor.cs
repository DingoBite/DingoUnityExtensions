using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using DingoUnityExtensions.UnityViewProviders.Core;
using UnityEngine;

namespace DingoUnityExtensions.MonoBehaviours.UI.Interactions
{
    public class ClickLinkGroupInteractor : ValueContainer<(string, string)>
    {
        [SerializeField] private SerializedDictionary<string, ClickLinkHandler> _clickLinkHandlers;

        private readonly ValueContainerGroup<string, string> _clickLinkGroup = new();

        public IReadOnlyDictionary<string, ClickLinkHandler> ClickLinkHandlers => _clickLinkHandlers;
        
        protected override void SetValueWithoutNotify((string, string) value)
        {
        }
        
#if VINSPECTOR_EXISTS
        [VInspector.Button]
#else
        [NaughtyAttributes.Button]
#endif
        public void Reinitialize()
        {
            _clickLinkGroup.Initialize(_clickLinkHandlers.Select(p => (p.Key, p.Value)));
        }

        private void ClickLinkGroupHandle(string key, string url)
        {
            Debug.Log($"{key} : {url}");
        }

        protected override void SubscribeOnly()
        {
            _clickLinkGroup.Initialize(_clickLinkHandlers.Select(p => (p.Key, p.Value)));
            _clickLinkGroup.OnValueChange += ClickLinkGroupHandle;
        }

        protected override void UnsubscribeOnly()
        {
            _clickLinkGroup.OnValueChange -= ClickLinkGroupHandle;
            _clickLinkGroup.Clear();
        }
    }
}