using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

using Energy8.Identity.UI.Runtime.Views.Base;
using Energy8.Identity.UI.Runtime.Views.Management.Data;
using UnityEngine;
using UnityEngine.UI;

namespace Energy8.Identity.UI.Runtime.Views.Management
{
    public class ViewManager : MonoBehaviour
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
            Debug.Log("View system initialized");
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
                // Removed debug log for showing view

                var result = await presenter.ShowView<TView, TParams, TResult>(@params, cts.Token);

                // Removed debug log for view completion
                return result;
            }
            catch (OperationCanceledException)
            {
                // Kept warning log for cancellation as it's important
                Debug.LogWarning($"View {typeof(TView).Name} was cancelled");
                throw;
            }
            catch (Exception ex)
            {
                // Kept error log as it's critical information
                Debug.LogError($"View {typeof(TView).Name} failed: {ex.Message}");
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
