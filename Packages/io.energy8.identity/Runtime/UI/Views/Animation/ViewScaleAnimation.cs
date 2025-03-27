using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Energy8.Identity.Core.Logging;

namespace Energy8.Identity.Views.Animation
{
    public class ViewScaleAnimation : ViewAnimationBase
    {
        private readonly RectTransform target;
        private readonly Vector3 from;
        private readonly Vector3 to;
        private readonly ILogger<ViewScaleAnimation> logger = new Logger<ViewScaleAnimation>();

        public ViewScaleAnimation(
            RectTransform target, 
            Vector3 from, 
            Vector3 to, 
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
            logger.LogDebug($"Starting scale animation from {from} to {to}");
            float elapsed = 0;

            while (elapsed < duration && !ct.IsCancellationRequested && IsPlaying)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / duration;
                target.localScale = Vector3.Lerp(from, to, EvaluateProgress(progress));
                await UniTask.Yield();
            }

            if (IsPlaying && !ct.IsCancellationRequested)
            {
                target.localScale = to;
                logger.LogDebug("Scale animation completed");
            }

            IsPlaying = false;
        }
    }
}