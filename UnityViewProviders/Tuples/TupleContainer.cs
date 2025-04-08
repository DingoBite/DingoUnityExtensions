using DingoUnityExtensions.UnityViewProviders.Core;
using UnityEngine;

namespace DingoUnityExtensions.UnityViewProviders.Tuples
{
    public abstract class TupleContainer<T1, T2> : ValueContainer<(T1, T2)>
    {
        [SerializeField] private ValueContainer<T1> _t1Container;
        [SerializeField] private ValueContainer<T2> _t2Container;

        protected override void SetValueWithoutNotify((T1, T2) value)
        {
            _t1Container.UpdateValueWithoutNotify(value.Item1);
            _t2Container.UpdateValueWithoutNotify(value.Item2);
        }
    }
    
    public abstract class TupleContainer<T1, T2, T3> : ValueContainer<(T1, T2, T3)>
    {
        [SerializeField] private ValueContainer<T1> _t1Container;
        [SerializeField] private ValueContainer<T2> _t2Container;
        [SerializeField] private ValueContainer<T3> _t3Container;

        protected override void SetValueWithoutNotify((T1, T2, T3) value)
        {
            _t1Container.UpdateValueWithoutNotify(value.Item1);
            _t2Container.UpdateValueWithoutNotify(value.Item2);
            _t3Container.UpdateValueWithoutNotify(value.Item3);
        }
    }
    
    public abstract class TupleContainer<T1, T2, T3, T4> : ValueContainer<(T1, T2, T3, T4)>
    {
        [SerializeField] private ValueContainer<T1> _t1Container;
        [SerializeField] private ValueContainer<T2> _t2Container;
        [SerializeField] private ValueContainer<T3> _t3Container;
        [SerializeField] private ValueContainer<T4> _t4Container;

        protected override void SetValueWithoutNotify((T1, T2, T3, T4) value)
        {
            _t1Container.UpdateValueWithoutNotify(value.Item1);
            _t2Container.UpdateValueWithoutNotify(value.Item2);
            _t3Container.UpdateValueWithoutNotify(value.Item3);
            _t4Container.UpdateValueWithoutNotify(value.Item4);
        }
    }
}