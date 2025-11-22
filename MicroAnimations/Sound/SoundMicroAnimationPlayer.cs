using UnityEngine;

namespace DingoUnityExtensions.MicroAnimations
{
    public class SoundMicroAnimationPlayer : MonoBehaviour
    {
        [SerializeField] private AudioSource _audioSource;
        
        private float _globalPitchMultiplier;
        private float _globalVolumeMultiplier;

        public void SetupGlobalParameters(float globalVolumeMultiplier, float globalPitchMultiplier)
        {
            _globalVolumeMultiplier = globalVolumeMultiplier;
            _globalPitchMultiplier = globalPitchMultiplier;
        }
        
        public void PlayOneShot(AudioPlayParameters parameters)
        {
            if (parameters?.AudioClip == null)
                return;
            _audioSource.volume = _globalVolumeMultiplier * (parameters.Volume + Random.value * parameters.VolumeRandomize);
            _audioSource.pitch = _globalPitchMultiplier * (parameters.Pitch + Random.value * parameters.PitchRandomize);
            _audioSource.PlayOneShot(parameters.AudioClip);
        }
    }
}