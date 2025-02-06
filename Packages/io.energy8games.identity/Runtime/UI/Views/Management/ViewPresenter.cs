using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Energy8.Identity.Core.Logging;
using Energy8.Identity.Views.Base;
using UnityEngine;
using UnityEngine.UI;

namespace Energy8.Identity.Views.Management
{
    public class ViewPresenter : IViewPresenter
    {
        private readonly ILogger<ViewPresenter> logger = new Logger<ViewPresenter>();
        private readonly IViewFactory factory;
        private readonly Transform parent;
        private readonly ScrollRect scrollView;
        private UnityEngine.Object currentView;

        public ViewPresenter(IViewFactory factory, Transform parent, ScrollRect scrollView)
        {
            this.factory = factory ?? throw new ArgumentNullException(nameof(factory));
            this.parent = parent ?? throw new ArgumentNullException(nameof(parent));
            this.scrollView = scrollView ?? throw new ArgumentNullException(nameof(scrollView));
            logger.LogDebug("ViewPresenter created");
        }

        public async UniTask<TResult> ShowView<TView, TParams, TResult>(
            TParams @params,
            CancellationToken ct
        ) where TView : ViewBase<TParams, TResult>
          where TParams : ViewParams
          where TResult : ViewResult
        {

            // if (currentView != null)
            // {
            //     UnityEngine.Object.Destroy(currentView);
            //     currentView = null;
            // }

            logger.LogDebug($"Creating view {typeof(TView).Name}");
            var view = await factory.Create<TView, TParams, TResult>(parent);
            currentView = view;

            scrollView.content = view.RectTransform;
            view.Initialize(@params);

            try
            {
                await view.Show(ct);
                var result = await view.ProcessAsync(ct);
                await view.Hide(ct);

                UnityEngine.Object.Destroy(view.gameObject);
                currentView = null;

                return result;
            }
            catch (Exception ex)
            {
                logger.LogWarning($"Error processing view {typeof(TView).Name}: {ex.Message}");
                await view.Hide(ct);

                UnityEngine.Object.Destroy(view.gameObject);
                currentView = null;

                throw;
            }
        }
    }
}