using System.Linq;
using UnityEngine;

namespace DingoUnityExtensions.Utils
{
    public static class EnumerableUtils
    {
        public static void SetActiveGameObjects(bool value, params Component[] components) => SetActive(value, components.Select(c => c.gameObject).ToArray());
        
        public static void SetActive(bool value, params GameObject[] objects)
        {
            foreach (var go in objects)
            {
                go.SetActive(value);
            }
        }
    }
}