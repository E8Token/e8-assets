using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Energy8.Identity.Shared.Core.Contracts.Dto.Errors;
using Energy8.Identity.Shared.Core.Exceptions;
using UnityEngine;

namespace Energy8.Identity.Shared.Core.Error
{
    public static class ErrorHandlingExtensions
    {
        public static async UniTask<T> WithRetry<T>(
            this UniTask<T> task,
            int maxRetries = 3,
            TimeSpan? delay = null,
            CancellationToken ct = default)
        {
            var retryDelay = delay ?? TimeSpan.FromSeconds(1);
            Exception lastException = null;

            for (int i = 0; i <= maxRetries; i++)
            {
                try
                {
                    if (i > 0) await UniTask.Delay(retryDelay, cancellationToken: ct);
                    return await task;
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    if (i == maxRetries) throw;
                }
            }

            throw lastException;
        }

        public static async UniTask WithRetry(
            this UniTask task,
            int maxRetries = 3,
            TimeSpan? delay = null,
            CancellationToken ct = default)
        {
            var retryDelay = delay ?? TimeSpan.FromSeconds(1);
            Exception lastException = null;

            for (int i = 0; i <= maxRetries; i++)
            {
                try
                {
                    if (i > 0) await UniTask.Delay(retryDelay, cancellationToken: ct);
                    await task;
                    return;
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    if (i == maxRetries) throw;
                }
            }

            throw lastException;
        }

        public static async UniTask<T> WithErrorHandler<T>(
            this Func<CancellationToken, UniTask<T>> taskFactory,
            Func<Energy8Exception, CancellationToken, UniTask<ErrorHandlingMethod>> errorHandler,
            CancellationToken ct = default)
        {
            while (true)
            {
                try
                {
                    return await taskFactory(ct);
                }
                catch (Exception ex)
                {
                    Energy8Exception e8Exception = ex is Energy8Exception exception ? exception :
                        new Energy8Exception("Error", ex.Message, true, true, false);

                    var handling = await errorHandler(e8Exception, ct);

                    switch (handling)
                    {
                        case ErrorHandlingMethod.Close:
                            throw new OperationCanceledException();
                        case ErrorHandlingMethod.TryAgain:
                            continue;
                        case ErrorHandlingMethod.SignOut:
                            throw new SignOutRequiredException();
                        default:
                            throw;
                    }
                }
            }
        }

        public static async UniTask WithErrorHandler(
            this Func<CancellationToken, UniTask> taskFactory,
            Func<Energy8Exception, CancellationToken, UniTask<ErrorHandlingMethod>> errorHandler,
            CancellationToken ct = default)
        {
            while (true)
            {
                try
                {
                    await taskFactory(ct);
                    return;
                }
                catch (Exception ex)
                {
                    Energy8Exception e8Exception = ex is Energy8Exception exception ? exception :
                        new Energy8Exception("Error", ex.Message, true, true, false);

                    var handling = await errorHandler(e8Exception, ct);

                    switch (handling)
                    {
                        case ErrorHandlingMethod.Close:
                            throw new OperationCanceledException();
                        case ErrorHandlingMethod.TryAgain:
                            continue;
                        case ErrorHandlingMethod.SignOut:
                            throw new SignOutRequiredException();
                        default:
                            throw;
                    }
                }
            }
        }

        public static UniTask<T> WithTimeout<T>(
            this UniTask<T> task,
            TimeSpan timeout,
            CancellationToken ct = default)
        {
            var timeoutTask = UniTask.Delay(timeout, cancellationToken: ct)
                .ContinueWith(() => throw new TimeoutException($"Operation timed out after {timeout.TotalSeconds}s"));

            return UniTask.WhenAny(task, timeoutTask).ContinueWith(t => t.result);
        }

        public static UniTask WithTimeout(
            this UniTask task,
            TimeSpan timeout,
            CancellationToken ct = default)
        {
            var timeoutTask = UniTask.Delay(timeout, cancellationToken: ct)
                .ContinueWith(() => throw new TimeoutException($"Operation timed out after {timeout.TotalSeconds}s"));

            return UniTask.WhenAny(task, timeoutTask);
        }
    }

    public class SignOutRequiredException : Exception
    {
    }
}