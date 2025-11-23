using System;
using UnityEngine;

namespace DingoUnityExtensions.UnityViewProviders.Navigation
{
    [Serializable]
    public class NavigationNodeSelectRule
    {
        public UnityEngine.UI.Navigation NavigationType;
        public string RoutePath;
        public bool YAsDirection = true;
        public float DotThreshold = 0.99f;
        public float DeltaThreshold = 100f;
        
        [Space]
        public bool OverrideAuto;

        public ContainerNavigationNode PriorNode;

        public NavigationNodeSelectRule()
        {
            NavigationType = UnityEngine.UI.Navigation.defaultNavigation;
        }

        public NavigationNodeSelectRule(ContainerNavigationNode priorNode)
        {
            PriorNode = priorNode;
            OverrideAuto = true;
            NavigationType = UnityEngine.UI.Navigation.defaultNavigation;
        }
    }
}