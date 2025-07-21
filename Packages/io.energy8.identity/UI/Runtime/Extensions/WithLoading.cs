using Cysharp.Threading.Tasks;
using Energy8.Identity.Shared.Core.Extensions;
using Energy8.Identity.UI.Runtime.Views.Implementations;
using Energy8.Identity.UI.Runtime.Views.Management;
using Energy8.Identity.UI.Runtime.Views.Models;
using System;
using System.Threading;
using UnityEngine;

namespace Energy8.Identity.UI.Runtime.Extensions
{
    public static class WithLoadingExtensions
    {
        private static ViewManager _viewManager;

        public static void InitializeLoading(this ViewManager viewManager)
        {
            _viewManager = viewManager;
        }

        public static void CleanupLoading()
        {
            _viewManager = null;
        }

        public static async UniTask<T> WithLoading<T>(
            this UniTask<T> task,
            CancellationToken ct) where T : class
        {
            if (_viewManager == null)
                throw new InvalidOperationException("ViewManager not initialized. Call InitializeLoading first");

            Debug.Log("WithLoading: Starting task with loading view");
            var result = (T)(await _viewManager.Show<LoadingView, LoadingViewParams, LoadingViewResult>(
                new ResultLoadingViewParams(task.AsObjectTask()), ct)).Result;
            Debug.Log("WithLoading: Task completed and loading view closed");
            return result;
        }

        public static async UniTask WithLoading(
            this UniTask task,
            CancellationToken ct)
        {
            if (_viewManager == null)
                throw new InvalidOperationException("ViewManager not initialized. Call InitializeLoading first");

            Debug.Log("WithLoading: Starting task with loading view (no result)");
            await _viewManager.Show<LoadingView, LoadingViewParams, LoadingViewResult>(
                new LoadingViewParams(task), ct);
            Debug.Log("WithLoading: Task completed and loading view closed (no result)");
        }
    }
}