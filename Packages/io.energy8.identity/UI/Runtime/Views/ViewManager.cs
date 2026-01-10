using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Energy8.Identity.UI.Core.Management;
using Energy8.Identity.UI.Core.Views;
using Energy8.Identity.UI.Runtime.Views.Management.Data;
using UnityEngine;
using UnityEngine.UI;

namespace Energy8.Identity.UI.Runtime.Views.Management
{
    public class ViewManager : MonoBehaviour, IViewManager
    {
        [Header("Setup")]
        [SerializeField] private Transform viewRoot;
        [SerializeField] private ScrollRect scrollView;
        [SerializeField] private ViewPrefabs prefabs;

        private IViewFactory factory;
        private IViewPresenter presenter;
        private readonly CancellationTokenSource lifetimeCts = new();

        private void Awake()
        {
            factory = new ViewFactory(prefabs);
            presenter = new ViewPresenter(factory, viewRoot, scrollView);
        }

        public async UniTask<TResult> Show<TView, TParams, TResult>(
            TParams @params,
            CancellationToken ct = default
        ) where TView : ViewBase<TParams, TResult>
          where TParams : ViewParams
          where TResult : ViewResult
        {
            try
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct, lifetimeCts.Token);
                var result = await presenter.ShowView<TView, TParams, TResult>(@params, cts.Token);
                return result;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private void OnDestroy()
        {
            lifetimeCts.Cancel();
            lifetimeCts.Dispose();
            // Removed debug log for destruction
        }
    }
}
