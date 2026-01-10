using System.Threading;
using Cysharp.Threading.Tasks;
using Energy8.Identity.UI.Core.Views;
using UnityEngine;


namespace Energy8.Identity.UI.Runtime.Views.Animation
{
    public abstract class ViewAnimationBase : IViewAnimation
    {
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
        }

        protected float EvaluateProgress(float progress)
        {
            var evaluated = curve.Evaluate(Mathf.Clamp01(progress));
            return evaluated;
        }
    }
}
