using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;

using Energy8.Identity.Http.Core;
using Energy8.Identity.Http.Core.Models;

namespace Energy8.Identity.Http.Runtime.Middleware
{
    /// <summary>
    /// Middleware для валидации HTTP запросов перед отправкой
    /// Должен быть первым в цепочке middleware
    /// </summary>
    public class ValidationMiddleware : IHttpMiddleware
    {
        public string Name => "Validation";

        /// <summary>
        /// Валидирует HTTP запрос перед отправкой
        /// </summary>
        public async UniTask<HttpResponse> ProcessAsync(HttpRequest request, HttpMiddlewareDelegate next, CancellationToken ct)
        {
            // Валидация URL
            if (string.IsNullOrWhiteSpace(request.Url))
            {
                throw new ArgumentException("URL cannot be empty or whitespace");
            }

            // Валидация формата URL
            if (!Uri.TryCreate(request.Url, UriKind.Absolute, out _))
            {
                throw new ArgumentException($"Invalid URL format: {request.Url}");
            }

            // Валидация метода
            if (string.IsNullOrWhiteSpace(request.Method))
            {
                throw new ArgumentException("HTTP method cannot be empty or whitespace");
            }

            var validMethods = new[] { "GET", "POST", "PUT", "DELETE", "PATCH", "HEAD", "OPTIONS" };
            if (!validMethods.Contains(request.Method.ToUpper()))
            {
                throw new ArgumentException($"Invalid HTTP method: {request.Method}. Valid methods: {string.Join(", ", validMethods)}");
            }

            // Валидация таймаута
            if (request.TimeoutSeconds <= 0)
            {
                throw new ArgumentException($"Timeout must be positive, got: {request.TimeoutSeconds}");
            }

            if (request.TimeoutSeconds > 300) // Максимум 5 минут
            {
                throw new ArgumentException($"Timeout too large: {request.TimeoutSeconds}s. Maximum allowed: 300s (5 minutes)");
            }

            // Валидация заголовков (проверка на null значения)
            if (request.Headers != null)
            {
                foreach (var header in request.Headers)
                {
                    if (string.IsNullOrWhiteSpace(header.Key))
                    {
                        throw new ArgumentException("Header key cannot be empty or whitespace");
                    }
                }
            }

            return await next(request, ct);
        }
    }
}
