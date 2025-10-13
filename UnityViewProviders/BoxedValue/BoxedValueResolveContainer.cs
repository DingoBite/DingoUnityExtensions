using System.Collections.Generic;
using DingoUnityExtensions.UnityViewProviders.Core;
using UnityEngine;

namespace DingoUnityExtensions.UnityViewProviders.BoxedValue
{
    public class BoxedValueResolveContainer : BoxedValueContainer
    {
        [SerializeField] private List<ContainerBase> _solvers;
        [SerializeField] private ValueUpdateBehaviour _valueUpdateBehaviour;

        private ContainerBase _lastInstance;

        protected override void SetValueWithoutNotify(BoxedValueWrapper value)
        {
            if (value.Type == null)
            {
                NotFoundHandle();
                return;
            }

            foreach (var solver in _solvers)
            {
                var found = solver.ValueType == value.Type;
                if (_valueUpdateBehaviour is ValueUpdateBehaviour.ActiveManage)
                {
                    solver.SetActiveContainer(found);
                }
                else if (_valueUpdateBehaviour is ValueUpdateBehaviour.CreateChild)
                {
                    if (_lastInstance != null)
                    {
                        _lastInstance.UpdateBoxedValueWithoutNotify(null);
                        Destroy(_lastInstance.gameObject);
                        _lastInstance = null;
                    }
                    
                    if (found)
                    {
                        _lastInstance = Instantiate(solver, transform);
                        _lastInstance.gameObject.layer = gameObject.layer;
                        _lastInstance.UpdateBoxedValueWithoutNotify(value.BoxedValue);
                        break;
                    }
                }
                
                if (found)
                    solver.UpdateBoxedValueWithoutNotify(value.BoxedValue);
            }
        }
        
        private void NotFoundHandle()
        {
            if (_valueUpdateBehaviour is ValueUpdateBehaviour.None)
                return;
            
            if (_valueUpdateBehaviour is ValueUpdateBehaviour.CreateChild)
            {
                if (_lastInstance != null)
                {
                    _lastInstance.UpdateBoxedValueWithoutNotify(null);
                    Destroy(_lastInstance.gameObject);
                    _lastInstance = null;
                }
                return;
            }
            
            foreach (var solver in _solvers)
            {
                solver.UpdateBoxedValueWithoutNotify(default);
                solver.SetActiveContainer(false);
            }
        }
    }
}