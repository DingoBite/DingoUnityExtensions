using DingoUnityExtensions.MonoBehaviours.Singletons;
using DingoUnityExtensions.Pools;
using DingoUnityExtensions.Pools.Core;
using UnityEngine;

namespace DingoUnityExtensions.MicroAnimations
{
    public class SoundMicroAnimationSource : SingletonProtectedBehaviour<SoundMicroAnimationSource>
    {
        [SerializeField] private SoundMicroAnimationClips _soundMicroAnimationClips;
        [SerializeField] private SoundMicroAnimationPlayer _prefab;
        [SerializeField] private int _poolSize = 2;
        [SerializeField] private float _globalVolumeMultiplier = 1;
        [SerializeField] private float _globalPitchMultiplier = 1;
        
        private bool _initialized;
        private Pool<SoundMicroAnimationPlayer> _pool;
        private int _lastPlayer = -1;

        private void Initialize()
        {
            _initialized = true;
            _pool = new Pool<SoundMicroAnimationPlayer>(_prefab, gameObject);
            for (var i = 0; i < _poolSize; i++)
            {
                var element = _pool.PullElement();
                element.SetupGlobalParameters(_globalVolumeMultiplier, _globalPitchMultiplier);
            }
        }

        private void PlayClip(AudioPlayParameters audioPlayParameters)
        {
            if (!Instance._initialized)
                Instance.Initialize();
            _lastPlayer = (_lastPlayer + 1) % _poolSize;
            var player = _pool.PulledElements[_lastPlayer];
            player.PlayOneShot(audioPlayParameters);
        }
        
        public static void PlayOneHot(string clipName)
        {
            if (Instance == null)
                return;
            var playParameters = Instance._soundMicroAnimationClips.GetClip(clipName);
            Instance.PlayClip(playParameters);
        }
    }
}