using System;
using System.Net;
using System.Threading;
using Cysharp.Threading.Tasks;

using Energy8.Identity.Http.Core;
using Energy8.Identity.Http.Core.Models;

namespace Energy8.Identity.Http.Runtime.Middleware
{
    /// <summary>
    /// Middleware для автоматического retry при временных ошибках
    /// Использует экспоненциальный backoff между попытками
    /// </summary>
    public class RetryMiddleware : IHttpMiddleware
    {
        public string Name => "Retry";
        
        private readonly int maxRetries;
        private readonly int baseDelayMs;
        private readonly int maxDelayMs;

        public RetryMiddleware(int maxRetries = 3, int baseDelayMs = 1000, int maxDelayMs = 5000)
        {
            if (maxRetries < 0)
                throw new ArgumentException("Max retries must be non-negative", nameof(maxRetries));
            if (baseDelayMs < 0)
                throw new ArgumentException("Base delay must be non-negative", nameof(baseDelayMs));
            if (maxDelayMs < baseDelayMs)
                throw new ArgumentException("Max delay must be greater or equal to base delay", nameof(maxDelayMs));
            
            this.maxRetries = maxRetries;
            this.baseDelayMs = baseDelayMs;
            this.maxDelayMs = maxDelayMs;
        }

        public async UniTask<HttpResponse> ProcessAsync(HttpRequest request, HttpMiddlewareDelegate next, CancellationToken ct)
        {
            HttpResponse lastResponse = null;
            Exception lastException = null;
            
            for (int attempt = 0; attempt <= maxRetries; attempt++)
            {
                try
                {
                    var response = await next(request, ct);
                    
                    // Если успех или ошибка, которую не стоит retry, возвращаем результат
                    if (response.Success || !ShouldRetry(response.StatusCode))
                    {
                        return response;
                    }
                    
                    lastResponse = response;
                    
                    // Если это последняя попытка, возвращаем ответ
                    if (attempt == maxRetries)
                    {
                        return response;
                    }
                    
                    // Ждём перед следующей попыткой
                    await DelayWithBackoff(attempt, ct);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    lastException = ex;
                    
                    // Если это не исключение, которое стоит retry, пробрасываем
                    if (!ShouldRetryException(ex))
                    {
                        throw;
                    }
                    
                    // Если это последняя попытка, пробрасываем исключение
                    if (attempt == maxRetries)
                    {
                        throw;
                    }
                    
                    // Ждём перед следующей попыткой
                    await DelayWithBackoff(attempt, ct);
                }
            }
            
            // Если мы здесь, значит все попытки исчерпаны
            throw lastException ?? new Exception("Request failed after retries", new InvalidOperationException(lastResponse?.Error));
        }
        
        private bool ShouldRetry(HttpStatusCode statusCode)
        {
            return statusCode == HttpStatusCode.ServiceUnavailable ||
                   statusCode == HttpStatusCode.BadGateway ||
                   statusCode == HttpStatusCode.GatewayTimeout ||
                   statusCode == 0; // Connection error
        }
        
        private bool ShouldRetryException(Exception ex)
        {
            return ex is WebException || ex is TimeoutException;
        }
        
        private async UniTask DelayWithBackoff(int attempt, CancellationToken ct)
        {
            // Экспоненциальный backoff: 2^attempt * baseDelay
            var delayMs = (long)Math.Pow(2, attempt) * baseDelayMs;
            
            // Ограничиваем максимальную задержку
            delayMs = Math.Min(delayMs, maxDelayMs);
            
            try
            {
                await UniTask.Delay((int)delayMs, cancellationToken: ct);
            }
            catch (OperationCanceledException)
            {
                // Игнорируем отмену при задержке между retry
            }
        }
    }
}
