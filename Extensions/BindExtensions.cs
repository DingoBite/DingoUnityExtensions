using System;
#if BIND_EXISTS
using Bind;
using DingoUnityExtensions.UnityViewProviders.Core;

namespace DingoUnityExtensions.Extensions
{
    public static class BindExtensions
    {
        public static void BindViewOnly<TValue>(this ValueContainer<TValue> view, IReadonlyBind<TValue> model)
        {
            model.OnValueChange -= view.UpdateValueWithoutNotify;
            model.OnValueChange += view.UpdateValueWithoutNotify;
        }

        public static void UnBindViewOnly<TValue>(this ValueContainer<TValue> view, IReadonlyBind<TValue> model)
        {
            model.OnValueChange -= view.UpdateValueWithoutNotify;
        }

        public static void BindViewOnlyAndUpdate<TValue>(this ValueContainer<TValue> view, IReadonlyBind<TValue> model)
        {
            view.BindViewOnly(model);
            view.UpdateValueWithoutNotify(model.V);
        }
        
        public static void BindTwoSide<TValue>(this ValueContainer<TValue> view, Bind<TValue> model)
        {
            view.SafeSubscribe(model.SetValue);
            model.OnValueChange -= view.UpdateValueWithoutNotify;
            model.OnValueChange += view.UpdateValueWithoutNotify;

            view.ValueChangeFromExternalSource = true;
        }

        public static void UnBindTwoSide<TValue>(this ValueContainer<TValue> view, Bind<TValue> model)
        {
            view.UnSubscribe(model.SetValue);
            model.OnValueChange -= view.UpdateValueWithoutNotify;
            
            view.ValueChangeFromExternalSource = false;
        }
        
        public static void BindTwoSideAndUpdate<TValue>(this ValueContainer<TValue> view, Bind<TValue> model)
        {
            view.BindTwoSide(model);
            view.UpdateValueWithoutNotify(model.V);
        }
        
        public static void BindTwoSide<TValue>(this ValueContainer<TValue> view, Action<TValue> modelAction, IReadonlyBind<TValue> model)
        {
            view.SafeSubscribe(modelAction);
            model.OnValueChange -= view.UpdateValueWithoutNotify;
            model.OnValueChange += view.UpdateValueWithoutNotify;
            
            view.ValueChangeFromExternalSource = true;
        }
        
        public static void UnBindTwoSide<TValue>(this ValueContainer<TValue> view, Action<TValue> modelAction, IReadonlyBind<TValue> model)
        {
            view.UnSubscribe(modelAction);
            model.OnValueChange -= view.UpdateValueWithoutNotify;
            
            view.ValueChangeFromExternalSource = false;
        }
        
        public static void BindTwoSideAndUpdate<TValue>(this ValueContainer<TValue> view, Action<TValue> modelAction, IReadonlyBind<TValue> model)
        {
            view.BindTwoSide(modelAction, model);
            view.UpdateValueWithoutNotify(model.V);
        }

        public static void BindTwoSide<TValue, TViewValue>(this ValueContainer<TViewValue> view, Action<TViewValue> viewChangeAction, Action<TValue> modelChangeAction, IReadonlyBind<TValue> model)
        {
            view.SafeSubscribe(viewChangeAction);
            model.OnValueChange -= modelChangeAction;
            model.OnValueChange += modelChangeAction;
            
            view.ValueChangeFromExternalSource = true;
        }
        
        public static void UnBindTwoSide<TValue, TViewValue>(this ValueContainer<TViewValue> view, Action<TViewValue> viewChangeAction, Action<TValue> modelChangeAction, IReadonlyBind<TValue> model)
        {
            view.UnSubscribe(viewChangeAction);
            model.OnValueChange -= modelChangeAction;
            
            view.ValueChangeFromExternalSource = false;
        }

        public static void BindTwoSideAndUpdate<TValue>(this ValueContainer<TValue> view, Action<TValue> viewChangeAction, Action<TValue> modelChangeAction, IReadonlyBind<TValue> model)
        {
            view.BindTwoSide(viewChangeAction, modelChangeAction, model);
            modelChangeAction(model.V);
        }
        
        public static void BindTwoSideAndUpdate<TValue, TViewValue>(this ValueContainer<TViewValue> view, Action<TViewValue> viewChangeAction, Action<TValue> modelChangeAction, IReadonlyBind<TValue> model, TViewValue viewValue)
        {
            view.BindTwoSide(viewChangeAction, modelChangeAction, model);
            viewChangeAction(viewValue);
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