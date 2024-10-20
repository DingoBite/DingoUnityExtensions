using UnityEngine;
using UnityEngine.SceneManagement;

namespace DingoUnityExtensions.MonoBehaviours
{
    public class SceneReload : MonoBehaviour
    {
        private void Update()
        {
            if (Input.GetKey(KeyCode.RightShift) && Input.GetKeyDown(KeyCode.R))
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}