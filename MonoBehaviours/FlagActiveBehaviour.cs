using UnityEngine;

namespace DingoUnityExtensions.MonoBehaviours
{
    public class FlagActiveBehaviour : MonoBehaviour
    {
        private ulong _flags = 1;
        private ulong _mask = 0;

        public void SetFlags(ulong flags)
        {
            _flags = flags;
            UpdateActiveness();
        }
        
        public void SetMaskFlags(ulong flags)
        {
            _mask = flags;
            UpdateActiveness();
        }

        public void MaskEnable(ushort flag)
        {
            _mask |= (uint)(1 << flag);
            UpdateActiveness();
        }
        
        public void MaskDisable(ushort flag = 0)
        {
            _mask &= ~(uint)(1 << flag);
            UpdateActiveness();
        }
        
        public void Enable(ushort flag = 0)
        {
            _flags |= (uint)(1 << flag);
            UpdateActiveness();
        }

        public void Disable(ushort flag = 0)
        {
            _flags &= ~(uint)(1 << flag);
            UpdateActiveness();
        }

        private void UpdateActiveness()
        {
            var flags = _flags & ~_mask;
            gameObject.SetActive(flags != 0);
        }
    }
}