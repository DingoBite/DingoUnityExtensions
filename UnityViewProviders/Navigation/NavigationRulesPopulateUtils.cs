using System.Collections.Generic;
using UnityEngine;

namespace DingoUnityExtensions.UnityViewProviders.Navigation
{
    public static class NavigationRulesPopulateUtils
    {
        public static void FindNavigationNodes(this ContainerNavigationNode node)
        {
            if (node == null)
                return;

            var upNode = ResolveDirection(node, node.Up, node.Up.YAsDirection ? Vector2.up : Vector3.forward);
            var downNode = ResolveDirection(node, node.Down, node.Down.YAsDirection ? Vector2.down : Vector3.back);
            var leftNode = ResolveDirection(node, node.Left, node.Left.YAsDirection ? Vector2.left : Vector3.left);
            var rightNode = ResolveDirection(node, node.Right, node.Right.YAsDirection ? Vector2.right : Vector3.right);

            node.SetupNavigationNodes(upNode, downNode, leftNode, rightNode);
        }

        public static ContainerNavigationNode FindUpNavigationNode(this ContainerNavigationNode node)
        {
            if (node == null)
                return null;
            return ResolveDirection(node, node.Up, node.Up.YAsDirection ? Vector2.up : Vector3.forward);
        }
        
        public static ContainerNavigationNode FindDownNavigationNode(this ContainerNavigationNode node)
        {
            if (node == null)
                return null;
            return ResolveDirection(node, node.Down, node.Down.YAsDirection ? Vector2.down : Vector3.back);
        }
        
        public static ContainerNavigationNode FindLeftNavigationNode(this ContainerNavigationNode node)
        {
            if (node == null)
                return null;
            return ResolveDirection(node, node.Left, node.Left.YAsDirection ? Vector2.left : Vector3.left);
        }
        
        public static ContainerNavigationNode FindRightNavigationNode(this ContainerNavigationNode node)
        {
            if (node == null)
                return null;
            return ResolveDirection(node, node.Right, node.Right.YAsDirection ? Vector2.right : Vector3.right);
        }
        
        public static void PopulateRoute(ContainerNavigationRoute route)
        {
            if (route == null)
                return;

            route.RebuildNodes();
            var nodes = route.Nodes;
            foreach (var node in nodes)
            {
                if (node != null)
                    node.FindNavigationNodes();
            }
        }

        private static ContainerNavigationNode ResolveDirection(ContainerNavigationNode origin, NavigationNodeSelectRule rule, Vector2 direction)
        {
            if (rule != null)
            {
                var mode = rule.NavigationType.mode;

                if (rule.OverrideAuto && rule.PriorNode != null)
                    return rule.PriorNode;

                if (mode == UnityEngine.UI.Navigation.Mode.None)
                    return null;

                if (mode == UnityEngine.UI.Navigation.Mode.Explicit && rule.PriorNode != null)
                    return rule.PriorNode;
            }

            var candidates = GetCandidates(origin, rule);
            return FindBest(origin, rule, candidates, direction);
        }

        private static IEnumerable<ContainerNavigationNode> GetCandidates(ContainerNavigationNode origin, NavigationNodeSelectRule rule)
        {
            var route = ResolveRoute(origin, rule);
            return GetNodesInRoute(route);
        }

        private static IEnumerable<ContainerNavigationNode> GetNodesInRoute(ContainerNavigationRoute route)
        {
            if (route == null)
                return null;

            route.RebuildNodes();
            return route.Nodes;
        }

        private static ContainerNavigationRoute ResolveRoute(ContainerNavigationNode origin, NavigationNodeSelectRule rule)
        {
            var startRoute = origin.Route;
            if (rule == null || string.IsNullOrEmpty(rule.RoutePath))
                return startRoute;

            var currentRoute = startRoute;
            if (currentRoute == null)
                return null;

            var parts = rule.RoutePath.Split('/');
            for (var i = 0; i < parts.Length; i++)
            {
                var part = parts[i];
                if (string.IsNullOrEmpty(part) || part == ".")
                    continue;
                if (part == "..")
                {
                    currentRoute = currentRoute.Route;
                    if (currentRoute == null)
                        break;
                }
            }

            return currentRoute;
        }
        
        private static ContainerNavigationNode FindBest(ContainerNavigationNode origin, NavigationNodeSelectRule rule, IEnumerable<ContainerNavigationNode> candidates, Vector2 direction)
        {
            if (candidates == null)
                return null;

            var originPos = Project(origin.transform.position);
            var dirN = direction.sqrMagnitude > Mathf.Epsilon ? direction.normalized : direction;

            var bestScore = float.NegativeInfinity;
            ContainerNavigationNode best = null;

            foreach (var other in candidates)
            {
                if (other == null || other == origin)
                    continue;

                var otherPos = Project(other.transform.position);
                var delta = otherPos - originPos;
                if (Mathf.Abs(delta.magnitude) > rule.DeltaThreshold)
                    continue;
                var distSqr = delta.sqrMagnitude;
                if (distSqr < Mathf.Epsilon)
                    continue;

                var deltaN = delta / Mathf.Sqrt(distSqr);
                var dot = Vector2.Dot(dirN, deltaN);
                if (dot <= rule.DotThreshold)
                    continue;

                var score = dot / distSqr;
                if (score > bestScore)
                {
                    bestScore = score;
                    best = other;
                }
            }

            return best;
        }

        private static Vector2 Project(Vector3 worldPosition)
        {
            return new Vector2(worldPosition.x, worldPosition.y);
        }
    }
}