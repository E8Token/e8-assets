using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Energy8.Identity.Views.Animation;
using Energy8.Identity.Core.Logging;

namespace Energy8.Identity.Views.Base
{
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class ViewBase<TParams, TResult> : MonoBehaviour, IView<TParams, TResult>
        where TParams : ViewParams
        where TResult : ViewResult
    {
        private protected readonly ILogger<ViewBase<TParams, TResult>> logger = new Logger<ViewBase<TParams, TResult>>();

        [Header("Base Components")]
        [SerializeField] protected RectTransform rt;
        [SerializeField] protected CanvasGroup canvasGroup;
        [SerializeField] protected float animationDuration = 0.5f;

        private protected UniTaskCompletionSource<TResult> completionSource;

        protected TParams Parameters { get; private set; }
        protected abstract IViewAnimation ShowAnimation { get; }
        protected abstract IViewAnimation HideAnimation { get; }

        public bool IsVisible => canvasGroup.alpha > 0;
        public bool IsInteractable
        {
            get => canvasGroup.interactable;
            private set => canvasGroup.interactable = value;
        }
        public RectTransform RectTransform => rt;

        protected virtual void OnValidate()
        {
            if (!rt) rt = GetComponent<RectTransform>();
            if (!canvasGroup) canvasGroup = GetComponent<CanvasGroup>();
        }

        public virtual void Initialize(TParams @params)
        {
            Parameters = @params ?? throw new ArgumentNullException(nameof(@params));
            completionSource = new UniTaskCompletionSource<TResult>();
            logger.LogDebug($"View {GetType().Name} initialized");
        }

        public virtual async UniTask<TResult> ProcessAsync(CancellationToken ct)
        {
            var result = await completionSource.Task.AttachExternalCancellation(ct);
            return result;
        }

        public virtual async UniTask Show(CancellationToken ct)
        {
            logger.LogDebug($"View {GetType().Name} showing");
            SetInteractable(false);
            await ShowAnimation.Play(ct);
            SetInteractable(true);
            logger.LogInfo($"View {GetType().Name} showed");
        }

        public virtual async UniTask Hide(CancellationToken ct)
        {
            logger.LogDebug($"View {GetType().Name} hiding");
            SetInteractable(false);
            await HideAnimation.Play(ct);
            logger.LogInfo($"View {GetType().Name} hiden");
        }

        public void SetInteractable(bool state) => IsInteractable = state;

        protected virtual void OnDestroy()
        {
            logger.LogDebug("View destroyed");
            completionSource?.TrySetCanceled();
        }
    }
}