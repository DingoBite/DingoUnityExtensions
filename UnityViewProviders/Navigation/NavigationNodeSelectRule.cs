using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace DingoUnityExtensions.UnityViewProviders.Navigation
{
    public enum NavigationRuleType
    {
        Auto,
        PriorNode,
        ByTag
    }

    public enum TagFindType
    {
        Any,
        All,
        Order // TODO
    }
    
    [Serializable]
    public record NavigationNodeSelectRule
    {
        public UnityEngine.UI.Navigation NavigationType;
        public string RoutePath;
        public bool YAsDirection = true;
        public float DotThreshold = 0.99f;
        public float DeltaThreshold = 100f;
        
        [Space]
        public NavigationRuleType NavigationRuleType;
        public List<string> FindByTags;
        public TagFindType TagFindType = TagFindType.Any;
        public bool FilterTagsByNavigationType = true;

        public ContainerNavigationNode PriorNode;

        public NavigationNodeSelectRule()
        {
            NavigationType = UnityEngine.UI.Navigation.defaultNavigation;
        }

        public NavigationNodeSelectRule(ContainerNavigationNode priorNode)
        {
            PriorNode = priorNode;
            NavigationRuleType = NavigationRuleType.Auto;
            NavigationType = UnityEngine.UI.Navigation.defaultNavigation;
        }
    }
}