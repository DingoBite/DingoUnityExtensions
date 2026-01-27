using System.Collections.Generic;
using DingoUnityExtensions.MonoBehaviours.Singletons;
using UnityEngine;

namespace DingoUnityExtensions.PrefabsCreateMenu
{
    [CreateAssetMenu(menuName = nameof(PrefabsCreateMenuConfig), fileName = S_PREFIX + nameof(PrefabsCreateMenuConfig), order = 0)]
    public class PrefabsCreateMenuConfig : ProtectedSingletonScriptableObject<PrefabsCreateMenuConfig>
    {
        [SerializeField] private List<string> _assetSubPathsToPrefabs;

        public static IReadOnlyList<string> AssetsSubPathsToPrefabs => Instance._assetSubPathsToPrefabs;
    }
}