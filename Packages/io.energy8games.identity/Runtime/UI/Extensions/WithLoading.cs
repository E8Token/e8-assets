using Cysharp.Threading.Tasks;
using Energy8.Identity.Core.Extensions;
using Energy8.Identity.Views.Implementations;
using Energy8.Identity.Views.Management;
using Energy8.Identity.Views.Models;
using System;
using System.Threading;

namespace Energy8.Identity.Extensions
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

            return (T)(await _viewManager.Show<LoadingView, LoadingViewParams, LoadingViewResult>(
                new ResultLoadingViewParams(task.AsObjectTask()), ct)).Result;
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