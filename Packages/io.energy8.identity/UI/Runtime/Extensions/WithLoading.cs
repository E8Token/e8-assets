using Cysharp.Threading.Tasks;
using Energy8.Identity.Shared.Core.Extensions;
using Energy8.Identity.UI.Runtime.Views.Implementations;
using Energy8.Identity.UI.Runtime.Views.Management;
using Energy8.Identity.UI.Core.Views.Models;
using System;
using System.Threading;
using UnityEngine;
using Energy8.Identity.UI.Core.Management;

namespace Energy8.Identity.UI.Runtime.Extensions
{
    public static class WithLoadingExtensions
    {
        private static IViewManager _viewManager;

        public static void InitializeLoading(this IViewManager viewManager)
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

            var result = (T)(await _viewManager.Show<LoadingView, LoadingViewParams, LoadingViewResult>(
                new ResultLoadingViewParams(task.AsObjectTask()), ct)).Result;
            return result;
        }

        public static async UniTask WithLoading(
            this UniTask task,
            CancellationToken ct)
        {
            if (_viewManager == null)
                throw new InvalidOperationException("ViewManager not initialized. Call InitializeLoading first");

            await _viewManager.Show<LoadingView, LoadingViewParams, LoadingViewResult>(
                new LoadingViewParams(task), ct);
        }
    }
}
