using System;
using System.Collections.Generic;
using DingoUnityExtensions.MonoBehaviours.Singletons;
using UnityEngine;

namespace DingoUnityExtensions.MonoBehaviours.Gizmos
{
    public class GizmosDrawer : SingletonBehaviour<GizmosDrawer>
    {
        private readonly List<Action> _drawActions = new();
        
        public static void AddCall(Action drawAction)
        {
#if UNITY_EDITOR
            Instance._drawActions.Add(drawAction);
#endif
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            foreach (var drawAction in _drawActions)
            {
                try
                {
                    drawAction();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
            _drawActions.Clear();
        }
#endif
    }
}