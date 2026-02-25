using System;
using System.Net;
using System.Threading;
using Cysharp.Threading.Tasks;

using Energy8.Identity.Http.Core;
using Energy8.Identity.Http.Core.Models;

namespace Energy8.Identity.Http.Runtime.Middleware
{
    /// <summary>
    /// Состояние Circuit Breaker
    /// </summary>
    public enum CircuitState
    {
        /// <summary>
        /// Circuit закрыт - запросы проходят нормально
        /// </summary>
        Closed,
        
        /// <summary>
        /// Circuit открыт - запросы блокируются
        /// </summary>
        Open,
        
        /// <summary>
        /// Circuit полуоткрыт - проверочное состояние
        /// </summary>
        HalfOpen
    }

    /// <summary>
    /// Middleware для реализации Circuit Breaker паттерна
    /// Защищает от каскадных отказов путём временного блокирования запросов
    /// </summary>
    public class CircuitBreakerMiddleware : IHttpMiddleware
    {
        public string Name => "CircuitBreaker";
        
        private readonly int failureThreshold;
        private readonly TimeSpan openTimeout;
        private readonly int halfOpenMaxCalls;
        
        private CircuitState state = CircuitState.Closed;
        private int failureCount = 0;
        private int halfOpenSuccessCount = 0;
        private DateTime? openUntil;
        
        private readonly object lockObj = new();

        public CircuitBreakerMiddleware(int failureThreshold = 5, TimeSpan openTimeout = default, int halfOpenMaxCalls = 3)
        {
            if (failureThreshold < 1)
                throw new ArgumentException("Failure threshold must be at least 1", nameof(failureThreshold));
            if (halfOpenMaxCalls < 1)
                throw new ArgumentException("Half-open max calls must be at least 1", nameof(halfOpenMaxCalls));
            
            this.failureThreshold = failureThreshold;
            this.openTimeout = openTimeout == default ? TimeSpan.FromMinutes(1) : openTimeout;
            this.halfOpenMaxCalls = halfOpenMaxCalls;
        }

        /// <summary>
        /// Возвращает текущее состояние Circuit Breaker
        /// </summary>
        public CircuitState GetState()
        {
            lock (lockObj)
            {
                // Проверяем не истёк ли timeout в состоянии Open
                if (state == CircuitState.Open && openUntil.HasValue && DateTime.UtcNow >= openUntil.Value)
                {
                    state = CircuitState.HalfOpen;
                    halfOpenSuccessCount = 0;
                    UnityEngine.Debug.Log($"[CircuitBreaker] Transitioning to Half-Open state (timeout expired)");
                }
                
                return state;
            }
        }

        public async UniTask<HttpResponse> ProcessAsync(HttpRequest request, HttpMiddlewareDelegate next, CancellationToken ct)
        {
            CircuitState currentState;
            DateTime? timeoutValue;
            
            lock (lockObj)
            {
                currentState = state;
                timeoutValue = openUntil;
            }
            
            // Если circuit открыт, возвращаем ошибку сразу
            if (currentState == CircuitState.Open)
            {
                if (timeoutValue.HasValue && DateTime.UtcNow < timeoutValue.Value)
                {
                    return new HttpResponse
                    {
                        Success = false,
                        StatusCode = HttpStatusCode.ServiceUnavailable,
                        Error = $"Circuit breaker is OPEN. Will retry after {(timeoutValue.Value - DateTime.UtcNow):mm\\:ss}"
                    };
                }
                else
                {
                    // Переход в Half-Open
                    lock (lockObj)
                    {
                        state = CircuitState.HalfOpen;
                        halfOpenSuccessCount = 0;
                        UnityEngine.Debug.Log("[CircuitBreaker] Transitioning to Half-Open state");
                    }
                }
            }
            
            try
            {
                var response = await next(request, ct);
                
                // Успешный запрос
                if (response.Success)
                {
                    lock (lockObj)
                    {
                        if (state == CircuitState.HalfOpen)
                        {
                            halfOpenSuccessCount++;
                            
                            if (halfOpenSuccessCount >= halfOpenMaxCalls)
                            {
                                // Возвращаем в Closed после успешных запросов
                                state = CircuitState.Closed;
                                failureCount = 0;
                                UnityEngine.Debug.Log($"[CircuitBreaker] Transitioning to CLOSED state ({halfOpenSuccessCount} successful calls in Half-Open)");
                            }
                        }
                        else
                        {
                            failureCount = 0;
                            UnityEngine.Debug.Log($"[CircuitBreaker] Success in Half-Open state ({halfOpenSuccessCount}/{halfOpenMaxCalls})");
                        }
                    }
                }
                else
                {
                    // Неуспешный запрос
                    lock (lockObj)
                    {
                        failureCount++;
                        
                        if (state == CircuitState.HalfOpen)
                        {
                            // Возвращаем в Open при ошибке в Half-Open
                            state = CircuitState.Open;
                            openUntil = DateTime.UtcNow + openTimeout;
                            UnityEngine.Debug.LogError($"[CircuitBreaker] Transitioning to OPEN state (failure in Half-Open: {response.Error})");
                        }
                        else if (failureCount >= failureThreshold)
                        {
                            // Открываем circuit при достижении порога
                            state = CircuitState.Open;
                            openUntil = DateTime.UtcNow + openTimeout;
                            UnityEngine.Debug.LogError($"[CircuitBreaker] Transitioning to OPEN state ({failureCount} failures >= {failureThreshold} threshold)");
                        }
                        else
                        {
                            UnityEngine.Debug.LogWarning($"[CircuitBreaker] Failure in CLOSED state ({failureCount}/{failureThreshold}): {response.Error}");
                        }
                    }
                }
                
                return response;
            }
            catch (Exception)
            {
                // Исключение считается как отказ
                lock (lockObj)
                {
                    failureCount++;
                    
                    if (state == CircuitState.HalfOpen)
                    {
                        state = CircuitState.Open;
                        openUntil = DateTime.UtcNow + openTimeout;
                        UnityEngine.Debug.LogError($"[CircuitBreaker] Transitioning to OPEN state (exception in Half-Open)");
                    }
                    else if (failureCount >= failureThreshold)
                    {
                        state = CircuitState.Open;
                        openUntil = DateTime.UtcNow + openTimeout;
                        UnityEngine.Debug.LogError($"[CircuitBreaker] Transitioning to OPEN state ({failureCount} failures >= {failureThreshold} threshold)");
                    }
                }
                
                throw;
            }
        }

        /// <summary>
        /// Сбрасывает Circuit Breaker в закрытое состояние
        /// </summary>
        public void Reset()
        {
            lock (lockObj)
            {
                state = CircuitState.Closed;
                failureCount = 0;
                halfOpenSuccessCount = 0;
                openUntil = null;
                UnityEngine.Debug.Log("[CircuitBreaker] Manually reset to CLOSED state");
            }
        }

        /// <summary>
        /// Получает статистику Circuit Breaker
        /// </summary>
        public CircuitBreakerStats GetStats()
        {
            lock (lockObj)
            {
                return new CircuitBreakerStats
                {
                    State = state,
                    FailureCount = failureCount,
                    OpenUntil = openUntil,
                    FailureThreshold = failureThreshold
                };
            }
        }
    }

    /// <summary>
    /// Статистика Circuit Breaker
    /// </summary>
    public class CircuitBreakerStats
    {
        public CircuitState State { get; set; }
        public int FailureCount { get; set; }
        public DateTime? OpenUntil { get; set; }
        public int FailureThreshold { get; set; }
    }
}
