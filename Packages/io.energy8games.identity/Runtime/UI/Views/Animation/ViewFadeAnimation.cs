using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Energy8.Identity.Core.Logging;

namespace Energy8.Identity.Views.Animation
{
    public class ViewFadeAnimation : ViewAnimationBase
    {
        private readonly CanvasGroup target;
        private readonly float from;
        private readonly float to;
        private readonly ILogger<ViewFadeAnimation> logger = new Logger<ViewFadeAnimation>();

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
            logger.LogDebug($"Starting fade animation from {from} to {to}");
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
                logger.LogDebug("Fade animation completed");
            }

            IsPlaying = false;
        }
    }
}