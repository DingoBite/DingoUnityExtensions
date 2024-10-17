using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;

namespace DingoUnityExtensions.Tweens
{
    public class TweenList : List<Tween>
    {
        public float MaxDuration { get; private set; }
        public Tween MaxTween { get; private set; }

        private readonly HashSet<Tween> _cachedKillTweens = new();

        public bool IsPlaying() => MaxTween != null && MaxTween.IsPlaying();
        
        public bool AddOnCompleteOrInvoke(Action callback)
        {
            if (MaxTween == null || !MaxTween.IsPlaying())
            {
                callback?.Invoke();
                return false;
            }

            MaxTween.onComplete += new TweenCallback(callback);
            return true;
        }
        
        public void ClearAndCacheKill()
        {
            _cachedKillTweens.RemoveWhere(t => !t.active);
            foreach (var tween in this.Where(t => t != null && t.active))
            {
                _cachedKillTweens.Add(tween);
            }

            Clear();
            MaxDuration = 0;
            MaxTween = null;
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
            MaxTween = null;
        }

        public void Play(Action onCompleteMaxDurationTween = null, Action<float> onExecuting = null, Action onKillMaxDurationTween = null)
        {
            foreach (var tween in this.Where(t => t != null))
            {
                tween.Play();
            }

            if (onCompleteMaxDurationTween != null)
                MaxTween?.OnComplete(() => onCompleteMaxDurationTween());
            if (onExecuting != null)
                MaxTween?.OnUpdate(() => onExecuting(MaxTween.position / MaxTween.Duration()));
            if (onKillMaxDurationTween != null)
                MaxTween?.OnKill(() => onKillMaxDurationTween());
        }

        public void AddWithDuration(Tween tween) => Add(tween, tween.Duration());

        public new void Add(Tween tween) => Add(tween, tween.Delay() + tween.Duration());
        
        public void Add(Tween tween, float fullDuration)
        {
            base.Add(tween);
            if (fullDuration >= MaxDuration)
            {
                MaxDuration = fullDuration;
                MaxTween = tween;
            }
        }
        
        public void AddMany(params Tween[] tweens)
        {
            foreach (var tween in tweens)
            {
                base.Add(tween);
            }
        }
        
        public void AddManyWithDurations(params Tween[] tweens)
        {
            foreach (var tween in tweens)
            {
                Add(tween, tween.Duration());
            }
        }
    }
}