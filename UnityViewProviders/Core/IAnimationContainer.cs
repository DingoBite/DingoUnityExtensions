using System;
using DG.Tweening;

namespace DingoUnityExtensions.UnityViewProviders.Core
{
    public interface IAnimationContainer<out TView, out TValue>
    {
        public void SetUpdateFunc(Action<TView, TValue> beforeStart = null, Action<TView, TValue, Tween> tweenModification = null);
    }
}