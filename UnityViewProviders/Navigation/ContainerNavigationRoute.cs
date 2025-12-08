using System;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;

namespace DingoUnityExtensions.UnityViewProviders.Navigation
{
    public class ContainerNavigationRoute : MonoBehaviour
    {
        [SerializeField] private ContainerNavigationRoute _route;
        [SerializeField, ReadOnly] private List<ContainerNavigationNode> _nodes = new();
        [SerializeField, ReadOnly] private List<ContainerNavigationRoute> _routes = new();

        [SerializeField] private bool _alwaysRebuildNodes;
        [SerializeField] private bool _isDirty = true;
        
        public IEnumerable<ContainerNavigationNode> Nodes => _nodes.Concat(_routes.SelectMany(r => r.Nodes));

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
        
        private void Awake()
        {
            _isDirty = true;
        }

        private void OnEnable()
        {
            RebuildNodes();
        }

        private void OnDisable()
        {
            if (_alwaysRebuildNodes)
            {
                _nodes.Clear();
                _routes.Clear();
            }
        }

        private void OnTransformChildrenChanged()
        {
            _isDirty = true;
            RebuildNodes();
        }

        private void Reset()
        {
            FindRoute();
        }

        private void FindRoute()
        {
            if (transform == null)
                return;
            _route = transform.parent.GetComponentInParent<ContainerNavigationRoute>();
        }

#if VINSPECTOR_EXISTS
        [VInspector.Button]
#else
        [NaughtyAttributes.Button]
#endif
        public void RebuildNodes()
        {
            if (!_alwaysRebuildNodes && !_isDirty && Application.isPlaying)
                return;
            FindRoute();
            _nodes.Clear();
            _routes.Clear();

            CollectNodesAndRoutes(transform);
        }

#if VINSPECTOR_EXISTS
        [VInspector.Button]
#else
        [NaughtyAttributes.Button]
#endif
        public void BuildFullStructure()
        {
            RebuildNodes();
            foreach (var navigationNode in Nodes.ToList())
            {
                try
                {
                    navigationNode.PopulateNavigationNodes();
                }
                catch (Exception e)
                {
                    Debug.LogException(e, navigationNode);
                }
            }
        }

        private void CollectNodesAndRoutes(Transform root)
        {
            if (root == null)
                return;

            if (root != transform)
            {
                var route = root.GetComponent<ContainerNavigationRoute>();
                if (route != null)
                {
                    if (!_routes.Contains(route))
                    {
                        route.RebuildNodes();
                        _routes.Add(route);
                    }
                    return;
                }
            }

            var node = root.GetComponent<ContainerNavigationNode>();
            if (node != null)
                _nodes.Add(node);

            var childCount = root.childCount;
            for (var i = 0; i < childCount; i++)
            {
                var child = root.GetChild(i);
                CollectNodesAndRoutes(child);
            }
        }
    }
}