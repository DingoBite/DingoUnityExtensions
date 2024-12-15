using UnityEngine;

namespace DingoUnityExtensions.SleepSystem
{
    public class GameObjectSleepable : Sleepable
    {
        [field: SerializeField] public GameObject GameObject { get; private set; }

        private void Reset() => GameObject = gameObject;

        protected override void OnSetSlippingValue(bool value)
        {
            GameObject.SetActive(!value);
            Visible = GameObject.activeInHierarchy;
        }
    }
}