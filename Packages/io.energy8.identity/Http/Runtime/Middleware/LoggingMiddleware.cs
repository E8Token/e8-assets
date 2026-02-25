using System;
using System.Threading;
using Cysharp.Threading.Tasks;

using Newtonsoft.Json;
using UnityEngine;

using Energy8.Identity.Http.Core;
using Energy8.Identity.Http.Core.Models;
using Energy8.Identity.Http.Runtime.Clients;

namespace Energy8.Identity.Http.Runtime.Middleware
{
    /// <summary>
    /// Middleware для логирования HTTP запросов и ответов
    /// Записывает детальную информацию о каждом запросе в Unity console
    /// </summary>
    public class LoggingMiddleware : IHttpMiddleware
    {
        public string Name => "Logging";
        
        private bool enableDetailedLogging;
        private bool maskAuthTokens;

        public LoggingMiddleware(bool enableDetailedLogging = true, bool maskAuthTokens = true)
        {
            this.enableDetailedLogging = enableDetailedLogging;
            this.maskAuthTokens = maskAuthTokens;
        }

        /// <summary>
        /// Включает или выключает маскирование токенов в логах
        /// </summary>
        public void EnableTokenLogging(bool enabled)
        {
            this.maskAuthTokens = !enabled;
        }

        public async UniTask<HttpResponse> ProcessAsync(HttpRequest request, HttpMiddlewareDelegate next, CancellationToken ct)
        {
            // Создаём копию заголовков для логирования
            var headers = new System.Collections.Generic.Dictionary<string, string>();
            foreach (var header in request.Headers)
            {
                headers[header.Key] = header.Value;
            }
            
            // Маскируем токен авторизации в логах
            if (maskAuthTokens && headers.ContainsKey("Authorization"))
            {
                headers["Authorization"] = "*** (masked) ***";
            }
            
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            HttpResponse response = null;
            
            try
            {
                response = await next(request, ct);
                stopwatch.Stop();
                
                // Лируем запрос
                LogRequest(request.Method, request.Url, headers, response.StatusCode, stopwatch.ElapsedMilliseconds, response.Success, response.Error, response.ResponseBody);
                
                return response;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                
                // Лируем ошибку
                LogError(request.Method, request.Url, headers, ex.Message, stopwatch.ElapsedMilliseconds);
                
                throw;
            }
        }
        
        private void LogRequest(string method, string url, System.Collections.Generic.Dictionary<string, string> headers, System.Net.HttpStatusCode statusCode, long durationMs, bool success, string error, string responseBody)
        {
            if (!enableDetailedLogging)
            {
                UnityEngine.Debug.Log($"[Identity HTTP] {method} {url} - Status: {(int)statusCode}, Duration: {durationMs}ms, Success: {success}");
                return;
            }
            
            var log = new HttpRequestLog
            {
                Method = method,
                Url = url,
                Headers = headers,
                ResponseCode = (long)statusCode,
                Duration = durationMs,
                Success = success,
                ErrorMessage = error,
                ResponseBody = responseBody
            };
            
            var json = JsonConvert.SerializeObject(log, Formatting.Indented);
            UnityEngine.Debug.Log($"[Identity HTTP] Request completed:\n{json}");
        }
        
        private void LogError(string method, string url, System.Collections.Generic.Dictionary<string, string> headers, string errorMessage, long durationMs)
        {
            if (!enableDetailedLogging)
            {
                UnityEngine.Debug.LogError($"[Identity HTTP] {method} {url} - Failed: {errorMessage}, Duration: {durationMs}ms");
                return;
            }
            
            var log = new HttpRequestLog
            {
                Method = method,
                Url = url,
                Headers = headers,
                Duration = durationMs,
                Success = false,
                ErrorMessage = errorMessage
            };
            
            var json = JsonConvert.SerializeObject(log, Formatting.Indented);
            UnityEngine.Debug.LogError($"[Identity HTTP] Request failed:\n{json}");
        }
    }
}
