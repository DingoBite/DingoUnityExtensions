using DingoUnityExtensions.MonoBehaviours.Singletons;
using UnityEngine;

namespace DingoUnityExtensions.MicroAnimations
{
    public class SoundMicroAnimationSource : SingletonProtectedBehaviour<SoundMicroAnimationSource>
    {
        [SerializeField] private AudioSource _audioSource;

        public static void PlayOneHot(AudioClip audioClip)
        {
            if (Instance != null || audioClip == null)
                Instance._audioSource.PlayOneShot(audioClip);
        }
    }
}