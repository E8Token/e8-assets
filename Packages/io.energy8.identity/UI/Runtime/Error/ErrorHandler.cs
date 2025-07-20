using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Energy8.Identity.Shared.Core.Error;
using Energy8.Identity.Shared.Core.Exceptions;
using Energy8.Identity.UI.Runtime.Canvas;
using Energy8.Identity.UI.Runtime.Views.Implementations;
using Energy8.Identity.UI.Runtime.Views.Models;

namespace Energy8.Identity.UI.Runtime.Error
{
    /// <summary>
    /// Централизованная обработка ошибок для Identity системы.
    /// Точный перенос ShowErrorAsync (строки 843-860)
    /// </summary>
    public class ErrorHandler : IErrorHandler
    {
        private readonly ICanvasManager canvasManager;
        private readonly bool debugLogging;
        
        public ErrorHandler(ICanvasManager canvasManager, bool debugLogging)
        {
            this.canvasManager = canvasManager;
            this.debugLogging = debugLogging;
        }
        
        /// <summary>
        /// Показывает ошибку пользователю через ErrorView
        /// Точный перенос ShowErrorAsync (строки 843-860)
        /// </summary>
        public async UniTask<ErrorHandlingMethod> ShowErrorAsync(Energy8Exception e8Exception, CancellationToken ct)
        {
            var viewManager = canvasManager.GetViewManager();
            if (viewManager == null)
            {
                Debug.LogError($"No ViewManager available to show error: {e8Exception.Message}");
                return ErrorHandlingMethod.Close;
            }

            var result = await viewManager.Show<ErrorView, ErrorViewParams, ErrorViewResult>(
                new ErrorViewParams(
                    e8Exception.Header,
                    e8Exception.Message,
                    e8Exception.CanRetry,
                    e8Exception.CanProceed,
                    e8Exception.MustSignOut), ct);

            return result.Method;
        }
    }
}
