#if BIND_EXISTS
using System;
using System.Collections.Generic;
using Bind;

namespace DingoUnityExtensions.UnityViewProviders.Core
{
    public static class TabsBindExtensions
    {
        public static void SubscribeTabGroupAndUpdate<TId>(this TabsContainer<TId> container, IReadonlyBind<TId> model, Action<TId> viewTabSelect, Action<TId> onTabSelect, Action onFullDeselect = null)
        {
            container.OnTabSelect += onTabSelect;
            if (onFullDeselect != null)
                container.OnFullDeselect += onFullDeselect;
            model.SafeSubscribe(viewTabSelect);
            viewTabSelect(model.V);
        }
        
        public static void UnSubscribeTabGroup<TId>(this TabsContainer<TId> container, IReadonlyBind<TId> model, Action<TId> viewTabSelect, Action<TId> onTabSelect, Action onFullDeselect = null)
        {
            container.OnTabSelect -= onTabSelect;
            if (onFullDeselect != null)
                container.OnFullDeselect -= onFullDeselect;
            model.UnSubscribe(viewTabSelect);
        }
    }

    public static class DictionaryGroupBindExtensions
    {
        public static void SubscribeCheckboxGroupAndUpdate<TId, TValue>(this DictionaryContainer<TId, TValue> dictionaryContainer, IReadonlyBind<IReadOnlyDictionary<TId, TValue>> model, Action<IReadOnlyDictionary<TId, TValue>> viewTabSelect, Action<IReadOnlyDictionary<TId, TValue>> onValueChange)
        {
            dictionaryContainer.OnValueChange += onValueChange;
            model.SafeSubscribe(viewTabSelect);
            viewTabSelect(model.V);
        }
        
        public static void UnSubscribeCheckboxGroup<TId, TValue>(this DictionaryContainer<TId, TValue> dictionaryContainer, IReadonlyBind<IReadOnlyDictionary<TId, TValue>> model, Action<IReadOnlyDictionary<TId, TValue>> viewTabSelect, Action<IReadOnlyDictionary<TId, TValue>> onValueChange)
        {
            dictionaryContainer.OnValueChange -= onValueChange;
            model.UnSubscribe(viewTabSelect);
        }
    }
}
#endif