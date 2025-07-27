#if UNITY_EDITOR
#endif
using System;
using System.Collections.Generic;
using DingoUnityExtensions.MonoBehaviours.Singletons;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DingoUnityExtensions.MonoBehaviours.GizmosUtility
{
    [ExecuteAlways]
    public class GizmosDrawer : SingletonBehaviour<GizmosDrawer>
    {
        private readonly Dictionary<object, Action> _drawPersistentActions = new();
        private readonly List<Action> _drawActions = new();
        
        private readonly Dictionary<Object, Action> _drawPersistentSelectedActions = new();
        private readonly Dictionary<Object, Action> _drawSelectedActions = new();

        private void Awake()
        {
            _drawPersistentActions.Clear();
            _drawActions.Clear();
            
            _drawPersistentSelectedActions.Clear();
            _drawSelectedActions.Clear();
        }
        
        public static void AddCall(Action drawAction)
        {
#if UNITY_EDITOR
            Instance._drawActions.Add(drawAction);
#endif
        }

        public static void AddPersistentCall(object key, Action drawAction)
        {
#if UNITY_EDITOR
            Instance._drawPersistentActions[key] = drawAction;
#endif
        }

        public static void RemovePersistentCall(object key)
        {
#if UNITY_EDITOR
            Instance._drawPersistentActions.Remove(key);
#endif
        }
        
        public static void AddCallWhenSelected(Object key, Action drawAction)
        {
#if UNITY_EDITOR
            Instance._drawSelectedActions[key] = drawAction;
#endif
        }

        public static void AddPersistentCallWhenSelected(Object key, Action drawAction)
        {
#if UNITY_EDITOR
            Instance._drawPersistentSelectedActions[key] = drawAction;
#endif
        }

        public static void RemovePersistentCallWhenSelected(Object key)
        {
#if UNITY_EDITOR
            Instance._drawPersistentSelectedActions.Remove(key);
#endif
        }
        
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            foreach (var drawAction in _drawPersistentActions.Values)
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
            
            foreach (var (obj, drawAction) in _drawPersistentSelectedActions)
            {
                if (Selection.activeObject != obj)
                    continue;
                
                try
                {
                    drawAction();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
            
            foreach (var (obj, drawAction) in _drawSelectedActions)
            {
                if (Selection.activeObject != obj)
                    continue;
                
                try
                {
                    drawAction();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
            _drawSelectedActions.Clear();
        }
#endif
    }
}