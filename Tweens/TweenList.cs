using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;

namespace DingoUnityExtensions.Tweens
{
    public class TweenList : List<Tween>
    {
        public float MaxDuration { get; private set; }
        private Tween _maxTween;

        private readonly HashSet<Tween> _cachedKillTweens = new();
        
        public void ClearAndCacheKill()
        {
            _cachedKillTweens.RemoveWhere(t => !t.active);
            foreach (var tween in this.Where(t => t != null && t.active))
            {
                _cachedKillTweens.Add(tween);
            }

            Clear();
            MaxDuration = 0;
            _maxTween = null;
        }
        
        public void Kill(bool isComplete = false)
        {
            foreach (var tween in _cachedKillTweens)
            {
                tween.Kill();
            }
            foreach (var tween in this.Where(t => t != null))
            {
                tween.Kill(isComplete);
            }
            Clear();
            _cachedKillTweens.Clear();
            MaxDuration = 0;
            _maxTween = null;
        }

        public void Play(Action onCompleteMaxDurationTween = null)
        {
            foreach (var tween in this.Where(t => t != null))
            {
                tween.Play();
            }
            
            if (onCompleteMaxDurationTween == null)
                return;
            
            _maxTween?.OnComplete(onCompleteMaxDurationTween.Invoke);
        }

        public void Add(Tween tween, float fullDuration)
        {
            Add(tween);
            if (fullDuration >= MaxDuration)
                _maxTween = tween;
        }
        
        public void AddMany(params Tween[] tweens)
        {
            foreach (var tween in tweens)
            {
                Add(tween);
            }
        }
    }
}