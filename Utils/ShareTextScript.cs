using DingoUnityExtensions.MonoBehaviours;
using DingoUnityExtensions.MonoBehaviours.Singletons;
using UnityEngine;

namespace DingoUnityExtensions.Utils
{
    public class ShareTextScript : SingletonBehaviour<ShareTextScript>
    {
        public void ShareText(string message)
        {
#if UNITY_EDITOR
            Debug.Log($"Share text {message}");
            return;
#endif

#if UNITY_ANDROID
            // block to open the file and share it ------------START
            AndroidJavaClass intentClass = new AndroidJavaClass("android.content.Intent");
            AndroidJavaObject intentObject = new AndroidJavaObject("android.content.Intent");
            intentObject.Call<AndroidJavaObject>("setAction", intentClass.GetStatic<string>("ACTION_SEND"));

            intentObject.Call<AndroidJavaObject>("setType", "text/plain");
            intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_TEXT"), "" + message);
            intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_SUBJECT"), "SUBJECT");

            AndroidJavaClass unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = unity.GetStatic<AndroidJavaObject>("currentActivity");

            currentActivity.Call("startActivity", intentObject);
#elif UNITY_IPHONE
            GUIUtility.systemCopyBuffer = message;
#endif
        }
    }
}