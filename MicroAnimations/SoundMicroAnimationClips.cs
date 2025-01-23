using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;

namespace DingoUnityExtensions.MicroAnimations
{
    [CreateAssetMenu(menuName = "MicroAnimations/" + nameof(SoundMicroAnimationClips), fileName = nameof(SoundMicroAnimationClips), order = 0)]
    public class SoundMicroAnimationClips : ScriptableObject
    {
        [SerializeField] private SerializedDictionary<string, AudioClip> _audioClip;

        public AudioClip GetClip(string clipName) => _audioClip.GetValueOrDefault(clipName);
    }
}