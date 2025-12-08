using System;
using System.Collections.Generic;
using DingoUnityExtensions.MonoBehaviours.GizmosUtility;
using DingoUnityExtensions.UnityViewProviders.Core;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DingoUnityExtensions.UnityViewProviders.Navigation
{
    [Flags]
    public enum EdgeNavigationCase
    {
        None,
        First,
        Last,
        Top,
        Bottom
    }
    
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ContainerBase))]
    public class ContainerNavigationNode : MonoBehaviour, IMoveHandler
    {
        public const string FIRST = "__First"; 
        public const string LAST = "__Last"; 
        public const string TOP = "__Top"; 
        public const string BOTTOM = "__Bottom";

        private static readonly Dictionary<object, bool> DefaultFilterTagsByNavigationType = new();
        private static readonly Dictionary<object, TagFindType> DefaultTagFindType = new();
        
        [SerializeField] private ContainerBase _container;

        [SerializeField] private ContainerNavigationRoute _route;
        [SerializeField, ReadOnly] private ContainerNavigationNode _upNode;
        [SerializeField, ReadOnly] private ContainerNavigationNode _downNode;
        [SerializeField, ReadOnly] private ContainerNavigationNode _leftNode;
        [SerializeField, ReadOnly] private ContainerNavigationNode _rightNode;
        [SerializeField] private bool _alwaysRebuildNodes;
        [SerializeField] private bool _isDirty = true;

        [Tooltip("Use \"" + FIRST + "\", \"" + LAST + "\", , \"" + TOP + "\", \"" + BOTTOM + "\" as predefined tags")]
        [field: SerializeField] public List<string> Tags { get; private set; }
        [field: SerializeField] public NavigationNodeSelectRule Up { get; private set; }
        [field: SerializeField] public NavigationNodeSelectRule Down { get; private set; }
        [field: SerializeField] public NavigationNodeSelectRule Left { get; private set; }
        [field: SerializeField] public NavigationNodeSelectRule Right { get; private set; }

        public ContainerBase ContainerBase => _container;
        
        public ContainerNavigationRoute Route
        {
            get
            {
                if (_route != null)
                    return _route;
                FindRoute();
                return _route;
            }
        }
        
        private void Reset()
        {
            _container = GetComponent<ContainerBase>();
            FindRoute();
        }

        private void OnTransformParentChanged()
        {
            _isDirty = true;
            FindRoute();
        }
        
        private void FindRoute()
        {
            if (transform.parent == null)
                return;
            _route = transform.parent.GetComponentInParent<ContainerNavigationRoute>();
            _isDirty = false;
        }

        private void Awake()
        {
            _isDirty = true;
            if (_container == null)
                _container = GetComponent<ContainerBase>();
        }

        public void TagEdgeNavigationCase(EdgeNavigationCase edgeNavigationCase)
        {
            ResolveEdgeCaseTag(edgeNavigationCase, EdgeNavigationCase.First, FIRST);
            ResolveEdgeCaseTag(edgeNavigationCase, EdgeNavigationCase.Last, LAST);
            ResolveEdgeCaseTag(edgeNavigationCase, EdgeNavigationCase.Bottom, BOTTOM);
            ResolveEdgeCaseTag(edgeNavigationCase, EdgeNavigationCase.Top, TOP);
        }

        public void LoopFindEdgeNavigationCase(EdgeNavigationCase edgeNavigationCase, bool manageNavigationType = false, bool manageNavigationRule = false)
        {
            ResolveEdgeCaseFind(Left, edgeNavigationCase, EdgeNavigationCase.First, LAST, manageNavigationType, manageNavigationRule);
            ResolveEdgeCaseFind(Right, edgeNavigationCase, EdgeNavigationCase.Last, FIRST, manageNavigationType, manageNavigationRule);
            ResolveEdgeCaseFind(Up, edgeNavigationCase, EdgeNavigationCase.Bottom, TOP, manageNavigationType, manageNavigationRule);
            ResolveEdgeCaseFind(Down, edgeNavigationCase, EdgeNavigationCase.Top, BOTTOM, manageNavigationType, manageNavigationRule);
        }

        private void ResolveEdgeCaseTag(EdgeNavigationCase edgeNavigationCase, EdgeNavigationCase checkEdgeCase, string edgeCaseTag)
        {
            if (edgeNavigationCase.HasFlag(checkEdgeCase))
                Tags.Add(edgeCaseTag);
            else
                Tags.Remove(edgeCaseTag);
        }
        
        private void ResolveEdgeCaseFind(NavigationNodeSelectRule rule, EdgeNavigationCase newCase, EdgeNavigationCase check, string tags, bool manageNavigationType, bool manageNavigationRule)
        {
            if (newCase.HasFlag(check))
            {
                rule.FindByTags.Add(tags);
                DefaultFilterTagsByNavigationType[rule] = rule.FilterTagsByNavigationType;
                if (manageNavigationType)
                    rule.FilterTagsByNavigationType = false;
                DefaultTagFindType[rule] = rule.TagFindType;
                if (manageNavigationRule)
                    rule.TagFindType = TagFindType.All;
            }
            else
            {
                rule.FindByTags.Remove(tags);
                if (DefaultFilterTagsByNavigationType.Remove(rule, out var filter))
                    rule.FilterTagsByNavigationType = filter;
                if (DefaultTagFindType.Remove(rule, out var tagFindType))
                    rule.TagFindType = tagFindType;
            }
        }

        public void OnMove(AxisEventData eventData)
        {
            if (EventSystem.current == null || _container == null)
                return;

            if (!_container.Interactable || !_container.Selectable)
                return;

            var targetNode = GetTargetNode(eventData.moveDir);
            if (targetNode == null || targetNode._container == null)
                return;

            var target = targetNode._container;

            if (!target.Interactable || !target.Selectable)
                return;

            target.SelectViaUnity(eventData);
            eventData.Use();
        }

        private ContainerNavigationNode GetTargetNode(MoveDirection dir)
        {
            if (_isDirty && !_alwaysRebuildNodes)
                PopulateNavigationNodes();
            
            return dir switch
            {
                MoveDirection.Up => _alwaysRebuildNodes ? this.FindUpNavigationNode() : _upNode,
                MoveDirection.Down => _alwaysRebuildNodes ? this.FindDownNavigationNode() : _downNode,
                MoveDirection.Left => _alwaysRebuildNodes ? this.FindLeftNavigationNode() : _leftNode,
                MoveDirection.Right => _alwaysRebuildNodes ? this.FindRightNavigationNode() : _rightNode,
                _ => default
            };
        }

        public void SetupNavigationRules(NavigationNodeSelectRule up = default, NavigationNodeSelectRule down = default, NavigationNodeSelectRule left = default, NavigationNodeSelectRule right = default)
        {
            Up = up;
            Down = down;
            Left = left;
            Right = right;
        }
        
        public void SetupNavigationNodes(ContainerNavigationNode up = default, ContainerNavigationNode down = default, ContainerNavigationNode left = default, ContainerNavigationNode right = default)
        {
            _upNode = up;
            _downNode = down;
            _leftNode = left;
            _rightNode = right;
        }

#if VINSPECTOR_EXISTS
        [VInspector.Button]
#else
        [NaughtyAttributes.Button]
#endif
        public void PopulateNavigationNodes() => this.FindNavigationNodes();
        
        public void SelectSelf()
        {
            if (EventSystem.current == null)
                return;

            EventSystem.current.SetSelectedGameObject(gameObject);
        }

        private void OnDrawGizmos()
        {
            DrawLink(_upNode, "up", Color.mediumSeaGreen);
            DrawLink(_downNode, "down", Color.green);
            DrawLink(_leftNode, "left", Color.orange);
            DrawLink(_rightNode, "right", Color.red);
        }
        
        private void DrawLink(Component target, string title, Color color)
        {
            if (target == null)
                return;
            var from = transform.position;
            var to = target.transform.position;
            var center = GizmosUtils.DrawNavigationCurveArrowHandles(from, to, color);
            GizmosUtils.DrawTextOutline(center, title, Color.white);
        }
    }
}