using System;
using System.Diagnostics;
using System.Threading;
using Cysharp.Threading.Tasks;

using Energy8.Identity.Http.Core;
using Energy8.Identity.Http.Core.Models;

namespace Energy8.Identity.Http.Runtime.Middleware
{
    /// <summary>
    /// Middleware для сбора статистики HTTP запросов
    /// Отслеживает успешные/неуспешные запросы, среднее время ответа
    /// </summary>
    public class StatisticsMiddleware : IHttpMiddleware
    {
        public string Name => "Statistics";
        
        private readonly HttpClientStats stats;

        public StatisticsMiddleware(HttpClientStats stats = null)
        {
            this.stats = stats ?? new HttpClientStats();
        }

        /// <summary>
        /// Возвращает текущую статистику
        /// </summary>
        public HttpClientStats GetStatistics() => stats;

        public async UniTask<HttpResponse> ProcessAsync(HttpRequest request, HttpMiddlewareDelegate next, CancellationToken ct)
        {
            var stopwatch = Stopwatch.StartNew();
            stats.TotalRequests++;
            
            try
            {
                var response = await next(request, ct);
                stopwatch.Stop();
                
                // Обновляем статистику успешных запросов
                if (response.Success)
                {
                    stats.SuccessfulRequests++;
                }
                else
                {
                    stats.FailedRequests++;
                }
                
                // Обновляем среднее время ответа
                UpdateAverageResponseTime(stats.TotalRequests, stopwatch.ElapsedMilliseconds);
                stats.LastRequestTime = DateTime.UtcNow;
                
                // Записываем длительность в ответ
                response.DurationMs = stopwatch.ElapsedMilliseconds;
                
                return response;
            }
            catch (Exception)
            {
                stopwatch.Stop();
                stats.FailedRequests++;
                UpdateAverageResponseTime(stats.TotalRequests, stopwatch.ElapsedMilliseconds);
                stats.LastRequestTime = DateTime.UtcNow;
                throw;
            }
        }
        
        private void UpdateAverageResponseTime(int totalRequests, long durationMs)
        {
            // Вычисляем скользящее среднее
            var previousTotal = stats.AverageResponseTime.TotalMilliseconds * (totalRequests - 1);
            var newAverage = TimeSpan.FromMilliseconds((previousTotal + durationMs) / totalRequests);
            stats.AverageResponseTime = newAverage;
        }
    }
}
