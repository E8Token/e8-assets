using System;
using System.Threading;
using Cysharp.Threading.Tasks;

using Energy8.Identity.UI.Runtime.Views.Base;
using UnityEngine;
using UnityEngine.UI;

namespace Energy8.Identity.UI.Runtime.Views.Management
{
    public class ViewPresenter : IViewPresenter
    {
        private readonly IViewFactory factory;
        private readonly Transform parent;
        private readonly ScrollRect scrollView;
        private UnityEngine.Object currentView;

        public ViewPresenter(IViewFactory factory, Transform parent, ScrollRect scrollView)
        {
            this.factory = factory ?? throw new ArgumentNullException(nameof(factory));
            this.parent = parent ?? throw new ArgumentNullException(nameof(parent));
            this.scrollView = scrollView ?? throw new ArgumentNullException(nameof(scrollView));
            // Removed debug log for creation
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

            Debug.Log($"Showing {typeof(TView).Name}");
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
                Debug.LogWarning($"Error in {typeof(TView).Name}: {ex.Message}");
                await view.Hide(ct);

                UnityEngine.Object.Destroy(view.gameObject);
                currentView = null;

                throw;
            }
        }
    }
}
