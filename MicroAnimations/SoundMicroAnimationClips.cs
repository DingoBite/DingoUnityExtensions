using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;

namespace DingoUnityExtensions.MicroAnimations
{
    [Serializable]
    public class AudioPlayParameters
    {
        public AudioClip AudioClip;
        public float Volume = 1;
        public float Pitch = 1;
        
        public float PitchRandomize;
        public float VolumeRandomize;
    }

    
    [CreateAssetMenu(menuName = "MicroAnimations/" + nameof(SoundMicroAnimationClips), fileName = nameof(SoundMicroAnimationClips), order = 0)]
    public class SoundMicroAnimationClips : ScriptableObject
    {
        [SerializeField] private SerializedDictionary<string, AudioPlayParameters> _audioClip;

        public AudioPlayParameters GetClip(string clipName) => _audioClip.GetValueOrDefault(clipName);
    }
}