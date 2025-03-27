using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Energy8.Identity.Core.Logging;

namespace Energy8.Identity.Views.Animation
{
    public abstract class ViewAnimationBase : IViewAnimation
    {
        private readonly ILogger<ViewAnimationBase> logger = new Logger<ViewAnimationBase>();
        protected readonly float duration;
        protected readonly AnimationCurve curve;
        
        public bool IsPlaying { get; protected set; }

        protected ViewAnimationBase(float duration, AnimationCurve curve = null)
        {
            this.duration = duration;
            this.curve = curve ?? AnimationCurve.Linear(0, 0, 1, 1);
        }

        public abstract UniTask Play(CancellationToken ct);
        
        public virtual void Stop()
        {
            IsPlaying = false;
            logger.LogDebug($"Animation {GetType().Name} stopped");
        }

        protected float EvaluateProgress(float progress)
        {
            var evaluated = curve.Evaluate(Mathf.Clamp01(progress));
            return evaluated;
        }
    }
}