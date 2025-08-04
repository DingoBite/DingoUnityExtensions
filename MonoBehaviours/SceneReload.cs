using UnityEngine;
using UnityEngine.SceneManagement;

namespace DingoUnityExtensions.MonoBehaviours
{
    public class SceneReload : MonoBehaviour
    {
        [SerializeField] private KeyCode _keyModifier1 = KeyCode.RightControl;
        [SerializeField] private KeyCode _keyModifier2 = KeyCode.RightShift;
        [SerializeField] private KeyCode _keyToReload = KeyCode.R;
        
        private void Update()
        {
            if (_keyModifier1 is KeyCode.None && _keyModifier2 is KeyCode.None && _keyToReload is KeyCode.None)
                return;
            
            var modifier1 = _keyModifier1 == KeyCode.None || Input.GetKey(_keyModifier1);
            var modifier2 = _keyModifier2 == KeyCode.None || Input.GetKey(_keyModifier2);
            
            if (modifier1 && modifier2 && Input.GetKeyDown(_keyToReload))
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}