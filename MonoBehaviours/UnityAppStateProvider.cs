using System;
using UnityEngine;

namespace DingoUnityExtensions.MonoBehaviours
{
    public class UnityAppStateProvider : MonoBehaviour
    {
        public delegate void QuitAction(Action quitAction);

        public event QuitAction OnQuit;

        public void ToggleFullscreen()
        {
            var fullScreen = !Screen.fullScreen;
            Screen.fullScreen = fullScreen;
            Screen.fullScreenMode = fullScreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
        }
        
        public void QuitWithInvoke()
        {
            if (OnQuit == null)
                QuitImmediately();
            else 
                OnQuit?.Invoke(QuitImmediately);
        }
        
        public void QuitImmediately()
        {
            Application.Quit();
        }
    }
}