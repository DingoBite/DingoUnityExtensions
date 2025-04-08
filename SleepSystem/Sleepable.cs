using System.Collections.Generic;
using Bind;
using UnityEngine;

namespace DingoUnityExtensions.SleepSystem
{
    public enum Sleep
    {
        None,
        Sleep,
        Active
    }
    
    public abstract class Sleepable : MonoBehaviour
    {
        [SerializeField] private List<Sleepable> _stack;
        
        private Sleep _sleep = Sleep.None;
        private readonly Bind<bool> _sleeping = new (true, false);
        
        public IReadonlyBind<bool> Sleeping => _sleeping;

        public bool Visible { get; protected set; } = true;
        
        protected virtual void OnSetSlippingValue(bool value){}
        
        public void SetSlippingValue(bool value)
        {
            if (value && _sleep == Sleep.Sleep || !value && _sleep == Sleep.Active)
                return;
            _sleep = value ? Sleep.Sleep : Sleep.Active;
            _sleeping.V = value;
            foreach (var sleepable in _stack)
            {
                sleepable.SetSlippingValue(value);
            }

            OnSetSlippingValue(value);
        }
    }
}