using DingoProjectAppStructure.Core.Config;
using DingoUnityExtensions.MonoBehaviours.GizmosUtility;

namespace DingoUnityExtensions.MonoBehaviours
{
    public abstract class ScriptableConfigWithGizmos<T> : ScriptableConfig<T> where T : ConfigBase
    {
        protected virtual void DrawGizmosOnSelected() {}
        protected virtual void DrawGizmos() {}

        private void OnEnable()
        {
            if (GizmosDrawer.Instance == null)
                return; 
            GizmosDrawer.AddPersistentCallWhenSelected(this, DrawGizmosOnSelected);
            GizmosDrawer.AddPersistentCall(this, DrawGizmos);
        }

        private void OnDisable() => GizmosDrawer.RemovePersistentCallWhenSelected(this);
    }
}