using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Energy8.Identity.Shared.Core.Exceptions;
using Energy8.Identity.Shared.Core.Error;

namespace Energy8.Identity.UI.Runtime.Extensions
{
    /// <summary>
    /// Extension методы для обработки ошибок с delegate-функциями
    /// </summary>
    public static class ErrorHandlerExtensions
    {
        /// <summary>
        /// Обработка ошибок для Func с UniTask
        /// </summary>
        public static async UniTask WithErrorHandler(
            this Func<CancellationToken, UniTask> func,
            Func<Energy8Exception, CancellationToken, UniTask<ErrorHandlingMethod>> errorHandler,
            CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    await func(ct);
                    return;
                }
                catch (Energy8Exception e8Exception)
                {
                    var method = await errorHandler(e8Exception, ct);
                    switch (method)
                    {
                        case ErrorHandlingMethod.TryAgain:
                            continue;
                        case ErrorHandlingMethod.Close:
                            return;
                    }
                }
            }
        }
        
        /// <summary>
        /// Обработка ошибок для Func с UniTask<T>
        /// </summary>
        public static async UniTask<T> WithErrorHandler<T>(
            this Func<CancellationToken, UniTask<T>> func,
            Func<Energy8Exception, CancellationToken, UniTask<ErrorHandlingMethod>> errorHandler,
            CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    return await func(ct);
                }
                catch (Energy8Exception e8Exception)
                {
                    var method = await errorHandler(e8Exception, ct);
                    switch (method)
                    {
                        case ErrorHandlingMethod.TryAgain:
                            continue;
                        case ErrorHandlingMethod.Close:
                            return default(T);
                    }
                }
            }
            
            return default(T);
        }
    }
}
