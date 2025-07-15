using Energy8.Identity.UI.Runtime.Views.Base;
using Energy8.Identity.UI.Runtime.Views.Models;
using Energy8.Identity.UI.Runtime.Views.Animation;
using Cysharp.Threading.Tasks;
using System;
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
            UniTask.Create(async () =>
            {
                try
                {
                    if (@params.GetType() == typeof(ResultLoadingViewParams))
                    {
                        Debug.Log("LoadingView: content");
                        completionSource.TrySetResult(new LoadingViewResult(
                                            await ((ResultLoadingViewParams)@params).Task));
                    }
                    else
                    {
                        Debug.Log("LoadingView: empty");
                        await @params.Task;
                        completionSource.TrySetResult(new LoadingViewResult());
                    }
                }

                catch (Exception ex)
                {
                    Debug.LogError("LoadingView: " + ex.Message);
                    completionSource.TrySetException(ex);
                }
            }).Forget();
        }
    }
}
