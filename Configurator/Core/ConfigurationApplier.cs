using DingoUnityExtensions.Utils;
using UnityEngine;

namespace DingoUnityExtensions.Configurator.Core
{
    public class ConfigurationApplier : MonoBehaviour
    {
        [SerializeField] private KeyCode _applyKey;

        private void Update()
        {
            if (Input.GetKeyDown(_applyKey))
            {
                var appliables = transform.FindComponents<IAppliable>();
                foreach (var appliable in appliables)
                {
                    appliable.Apply();
                }
            }
        }
    }
}