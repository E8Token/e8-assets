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
            Debug.Log("View system initialized");
        }

        public async UniTask<TResult> Show<TView, TParams, TResult>(
            TParams @params,
            CancellationToken ct = default
        ) where TView : ViewBase<TParams, TResult>
          where TParams : ViewParams
          where TResult : ViewResult
        {
            Debug.Log($"[ViewManager] Show<{typeof(TView).Name}> called with params: {@params?.GetType().Name}");
            try
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct, lifetimeCts.Token);
                var result = await presenter.ShowView<TView, TParams, TResult>(@params, cts.Token);
                Debug.Log($"[ViewManager] Show<{typeof(TView).Name}> completed with result: {result}");
                return result;
            }
            catch (OperationCanceledException)
            {
                Debug.LogWarning($"[ViewManager] Show<{typeof(TView).Name}> cancelled");
                throw;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ViewManager] Show<{typeof(TView).Name}> failed: {ex.Message}");
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
