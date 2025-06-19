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
        
        public static void BindTwoSide<TValue>(this ValueContainer<TValue> view, Action<TValue> onViewChange, IReadonlyBind<TValue> model)
        {
            view.SafeSubscribe(onViewChange);
            model.OnValueChange -= view.UpdateValueWithoutNotify;
            model.OnValueChange += view.UpdateValueWithoutNotify;
            
            view.ValueChangeFromExternalSource = true;
        }
        
        public static void UnBindTwoSide<TValue>(this ValueContainer<TValue> view, Action<TValue> onViewChange, IReadonlyBind<TValue> model)
        {
            view.UnSubscribe(onViewChange);
            model.OnValueChange -= view.UpdateValueWithoutNotify;
            
            view.ValueChangeFromExternalSource = false;
        }
        
        public static void BindTwoSideAndUpdate<TValue>(this ValueContainer<TValue> view, Action<TValue> onViewChange, IReadonlyBind<TValue> model)
        {
            view.BindTwoSide(onViewChange, model);
            view.UpdateValueWithoutNotify(model.V);
        }

        public static void BindTwoSide<TValue, TViewValue>(this ValueContainer<TViewValue> view, Action<TViewValue> onViewChange, Action<TValue> onModelChange, IReadonlyBind<TValue> model)
        {
            view.SafeSubscribe(onViewChange);
            model.OnValueChange -= onModelChange;
            model.OnValueChange += onModelChange;
            
            view.ValueChangeFromExternalSource = true;
        }
        
        public static void UnBindTwoSide<TValue, TViewValue>(this ValueContainer<TViewValue> view, Action<TViewValue> onViewChange, Action<TValue> onModelChange, IReadonlyBind<TValue> model)
        {
            view.UnSubscribe(onViewChange);
            model.OnValueChange -= onModelChange;
            
            view.ValueChangeFromExternalSource = false;
        }

        public static void BindTwoSideAndUpdate<TValue>(this ValueContainer<TValue> view, Action<TValue> onViewChange, Action<TValue> onModelChange, IReadonlyBind<TValue> model)
        {
            view.BindTwoSide(onViewChange, onModelChange, model);
            onModelChange(model.V);
        }
        
        public static void BindTwoSideAndUpdate<TValue, TViewValue>(this ValueContainer<TViewValue> view, Action<TViewValue> onViewChange, Action<TValue> onModelChange, IReadonlyBind<TValue> model, TViewValue viewValue)
        {
            view.BindTwoSide(onViewChange, onModelChange, model);
            onViewChange(viewValue);
        }

        public static void UnSubscribe<TValue>(this ValueContainer<TValue> view, Action<TValue> onViewChange)
        {
            view.OnValueChange -= onViewChange;
        }
        
        public static void SafeSubscribe<TValue>(this ValueContainer<TValue> view, Action<TValue> onViewChange)
        {
            view.OnValueChange -= onViewChange;
            view.OnValueChange += onViewChange;
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