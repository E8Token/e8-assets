using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;


namespace Energy8.Identity.UI.Runtime.Views.Animation
{
    public class ViewFadeAnimation : ViewAnimationBase
    {
        private readonly CanvasGroup target;
        private readonly float from;
        private readonly float to;

        public ViewFadeAnimation(
            CanvasGroup target, 
            float from, 
            float to, 
            float duration, 
            AnimationCurve curve = null) : base(duration, curve)
        {
            this.target = target;
            this.from = from;
            this.to = to;
        }

        public override async UniTask Play(CancellationToken ct)
        {
            IsPlaying = true;
            float elapsed = 0;

            while (elapsed < duration && !ct.IsCancellationRequested && IsPlaying)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / duration;
                target.alpha = Mathf.Lerp(from, to, EvaluateProgress(progress));
                await UniTask.Yield();
            }

            if (IsPlaying && !ct.IsCancellationRequested)
            {
                target.alpha = to;
            }

            IsPlaying = false;
        }
    }
}
