using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Energy8.Models;
using UnityEngine;

namespace Energy8.Auth.Content
{
    [RequireComponent(typeof(CanvasGroup))]
    public class AuthContentBase : MonoBehaviour
    {
        [Header("Logger")]
        [SerializeField] string loggerName;
        [SerializeField] Color loggerColor;

        [Header("Functional (Base)")]
        [SerializeField] RectTransform rt;
        [SerializeField] CanvasGroup canvasGroup;

        [Header("Animation (Base)")]
        [SerializeField][Tooltip("Animation duration in msc")] int animationDuration = 500;

        private protected Logger logger;

        public RectTransform RectTransform => rt;
        public bool Interactable
        {
            get => canvasGroup.interactable;
            private protected set => canvasGroup.interactable = value;
        }

        private protected UniTaskCompletionSource<AuthContentResultBase> taskCompletionSource;
        CancellationTokenSource showCTS;

        void Reset()
        {
            TryGetComponent(out rt);
            TryGetComponent(out canvasGroup);
        }

        #region UI
        private protected void ShowUI()
        {
            showCTS = new();
            Interactable = false;
            AnimateAlphaAsync(showCTS.Token, 0, 1, animationDuration).
                AttachExternalCancellation(destroyCancellationToken).
                ContinueWith(() => Interactable = true);
        }
        private protected void HideUI()
        {
            showCTS.Cancel();
            Interactable = false;
            AnimateAlphaAsync(destroyCancellationToken, 1, 0, animationDuration).
                ContinueWith(() => Destroy(gameObject));
        }
        async UniTask AnimateAlphaAsync(CancellationToken cancellationToken, float startAlpha, float endAlpha, int duration)
        {
            float i = 0;
            do
            {
                await UniTask.Yield(PlayerLoopTiming.Update);
                if (cancellationToken.IsCancellationRequested)
                    throw new OperationCanceledException();
                i += Time.deltaTime * 1000;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, i / duration);
            }
            while (i < duration);
            canvasGroup.alpha = endAlpha;
        }
        #endregion

        #region Functional
        private protected virtual void Initialize<TResult>(UniTaskCompletionSource<TResult> taskCompletionSource, params object[] args) where TResult : AuthContentResultBase
        {
            logger = new Logger(gameObject, loggerName, loggerColor);
            logger.Log($"Initialize({string.Join(", ", args)})");
        }
        public virtual async UniTask<TResult> TryProcessContentAsync<TResult>(CancellationToken cancellationToken, params object[] args) where TResult : AuthContentResultBase
        {
            try
            {
                UniTaskCompletionSource<TResult> taskCompletionSource = new();
                Initialize(taskCompletionSource, args);
                ShowUI();
                var result = await taskCompletionSource.Task.AttachExternalCancellation(cancellationToken);
                HideUI();
                return result;
            }
            catch (OperationCanceledException ex)
            {
                await UniTask.SwitchToMainThread();
                Destroy(gameObject);
                throw ex;
            }
            catch (Exception ex)
            {
                await UniTask.SwitchToMainThread();
                Destroy(gameObject);
                throw ex;
            }
        }
    }
    #endregion
    public class AuthContentResultBase
    { }
}