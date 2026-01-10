using Energy8.Identity.UI.Core.Views;
using Energy8.Identity.UI.Core.Views.Models;
using Energy8.Identity.UI.Runtime.Views.Animation;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

namespace Energy8.Identity.UI.Runtime.Views.Implementations
{
    public class LoadingView : ViewBase<LoadingViewParams, LoadingViewResult>
    {
        protected override IViewAnimation ShowAnimation =>
            new ViewFadeAnimation(canvasGroup, 0f, 1f, animationDuration);

        protected override IViewAnimation HideAnimation =>
            new ViewFadeAnimation(canvasGroup, 1f, 0f, animationDuration);

        public override void Initialize(LoadingViewParams @params)
        {
            base.Initialize(@params);

            // Не запускаем таск сразу, а сохраняем параметры для использования в ProcessAsync
        }

        public override async UniTask<LoadingViewResult> ProcessAsync(CancellationToken ct)
        {
            // Минимальное время отображения loading view (500ms)
            var minDisplayTask = UniTask.Delay(500, cancellationToken: ct);
            
            try
            {
                UniTask taskToWait;
                bool hasResult = false;
                object result = null;
                
                if (Parameters.GetType() == typeof(ResultLoadingViewParams))
                {
                    var resultParams = (ResultLoadingViewParams)Parameters;
                    taskToWait = resultParams.Task.ContinueWith(r => { result = r; hasResult = true; });
                }
                else
                {
                    taskToWait = Parameters.Task;
                }
                
                // Ждем как минимум 500ms И завершения таска
                await UniTask.WhenAll(minDisplayTask, taskToWait);

                if (hasResult)
                {
                    return new LoadingViewResult(result);
                }
                else
                {
                    return new LoadingViewResult();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"LoadingView: Task failed with error: {ex.Message}");
                
                // Все равно ждем минимальное время даже при ошибке
                try
                {
                    await minDisplayTask;
                }
                catch
                {
                    // Игнорируем отмену минимального времени
                }
                
                throw;
            }
        }
    }
}
