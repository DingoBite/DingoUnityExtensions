using System;
#if BIND_EXISTS
using Bind;
using DingoUnityExtensions.UnityViewProviders.Core;

namespace DingoUnityExtensions.Extensions
{
    public static class BindExtensions
    {
        
        public static void BindTwoSideModel<TValue>(this ValueContainer<TValue> view, Bind<TValue> model)
        {
            view.SafeSubscribe(model.SetValue);
            model.OnValueChange -= view.SetValueWithoutNotify;
            model.OnValueChange += view.SetValueWithoutNotify;
        }
        
        public static void BindTwoSideModel<TValue>(this ValueContainer<TValue> view, Action<TValue> modelAction, IReadonlyBind<TValue> model)
        {
            view.SafeSubscribe(modelAction);
            model.OnValueChange -= view.SetValueWithoutNotify;
            model.OnValueChange += view.SetValueWithoutNotify;
        }

        public static void BindTwoSideModel<TValue, TViewValue>(this ValueContainer<TViewValue> view, Action<TViewValue> viewChangeAction, Action<TValue> modelChangeAction, IReadonlyBind<TValue> model)
        {
            view.SafeSubscribe(viewChangeAction);
            model.OnValueChange -= modelChangeAction;
            model.OnValueChange += modelChangeAction;
        }
        
        public static void UnSubscribe<TValue>(this ValueContainer<TValue> view, Action<TValue> callback)
        {
            view.OnValueChange -= callback;
        }
        
        public static void SafeSubscribe<TValue>(this ValueContainer<TValue> view, Action<TValue> callback)
        {
            view.OnValueChange -= callback;
            view.OnValueChange += callback;
        }

        public static void UnSubscribe(this EventContainer view, Action action)
        {
            view.OnEvent -= action;
        }

        public static void SafeSubscribe(this EventContainer view, Action action)
        {
            view.OnEvent -= action;
            view.OnEvent += action;
        }
    }
}
#endif