#if ADDRESSABLES_EXISTS
using UnityEngine.ResourceManagement.AsyncOperations;

namespace DingoUnityExtensions.Addressables
{
    public static class AddressablesExtension
    {
        public static AsyncOperationHandle<TAsset> Load<TAsset>(this string addressablePath) => UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<TAsset>(addressablePath);
    }
}
#endif