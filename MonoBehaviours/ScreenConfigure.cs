using UnityEngine;

namespace DingoUnityExtensions.MonoBehaviours
{
    public class ScreenConfigure : MonoBehaviour
    {
        [SerializeField] private int _frameRate = 90;
        
        private void Start()
        {
            Application.targetFrameRate = _frameRate;
        }
    }
}