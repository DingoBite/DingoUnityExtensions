#if BIND_EXISTS
using System;
using Bind;

namespace DingoUnityExtensions.UnityViewProviders.Core
{
    public static class TabsBindExtensions
    {
        public static void SubscribeTabGroupAndUpdate<TTabs>(this TabsContainer<TTabs> tabs, IReadonlyBind<TTabs> model, Action<TTabs> viewTabSelect, Action<TTabs> onTabSelect, Action onFullDeselect = null)
        {
            tabs.OnTabSelect += onTabSelect;
            if (onFullDeselect != null)
                tabs.OnFullDeselect += onFullDeselect;
            model.SafeSubscribe(viewTabSelect);
            viewTabSelect(model.V);
        }
        
        public static void UnSubscribeTabGroup<TTabs>(this TabsContainer<TTabs> tabs, IReadonlyBind<TTabs> model, Action<TTabs> viewTabSelect, Action<TTabs> onTabSelect, Action onFullDeselect = null)
        {
            tabs.OnTabSelect -= onTabSelect;
            if (onFullDeselect != null)
                tabs.OnFullDeselect -= onFullDeselect;
            model.UnSubscribe(viewTabSelect);
        }
    }
}
#endif