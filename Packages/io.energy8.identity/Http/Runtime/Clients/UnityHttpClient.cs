using System;
using System.Net;
using System.Threading;
using Cysharp.Threading.Tasks;

using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

using Energy8.Identity.Shared.Core.Contracts.Dto.Common;
using Energy8.Identity.Shared.Core.Contracts.Dto.Errors;
using Energy8.Identity.Shared.Core.Exceptions;

using Energy8.Identity.Http.Core;
using Energy8.Identity.Http.Core.Models;
using Energy8.Identity.Http.Runtime.Serializers;
using Energy8.Identity.Http.Runtime.Middleware;
using Energy8.Identity.Http.Runtime.Clients;

namespace Energy8.Identity.Http.Runtime.Clients
{
    public class UnityHttpClient : IHttpClient
    {
        private readonly string baseUrl;
        private string authToken;
        private readonly HttpPipeline pipeline;
        private readonly HttpClientStats stats;
        private readonly LoggingMiddleware loggingMiddleware;
        private readonly ValidationMiddleware validationMiddleware;
        private readonly RetryMiddleware retryMiddleware;
        private readonly StatisticsMiddleware statisticsMiddleware;
        private readonly CircuitBreakerMiddleware circuitBreakerMiddleware;

        private readonly object lockObj = new();
        private RequestOptions defaultOptions = new RequestOptions(30, 3, false);

        public string BaseUrl => baseUrl;

        public IRequestSerializer Serializer { get; set; }
        public HttpClientStats Statistics => stats;

        public UnityHttpClient(string baseUrl)
        {
            this.baseUrl = baseUrl ?? throw new ArgumentNullException(nameof(baseUrl));
            this.Serializer = new WWWFormSerializer();
            this.stats = new HttpClientStats();
            
            // Создаём middleware
            this.validationMiddleware = new ValidationMiddleware();
            this.retryMiddleware = new RetryMiddleware(3, 1000, 5000);
            this.loggingMiddleware = new LoggingMiddleware(enableDetailedLogging: true, maskAuthTokens: true);
            this.statisticsMiddleware = new StatisticsMiddleware(stats);
            this.circuitBreakerMiddleware = new CircuitBreakerMiddleware(failureThreshold: 5, openTimeout: TimeSpan.FromMinutes(1), halfOpenMaxCalls: 3);
            
            // Создаём pipeline
            this.pipeline = new HttpPipeline()
                .Use(validationMiddleware)       // 1. Валидация (всегда первой)
                .Use(circuitBreakerMiddleware)   // 2. Circuit Breaker
                .Use(retryMiddleware)            // 3. Retry
                .Use(statisticsMiddleware)       // 4. Статистика
                .Use(loggingMiddleware);         // 5. Логирование (всегда последней)
                
            // Устанавливаем финальный handler для выполнения фактического HTTP запроса
            pipeline.SetFinalHandler(async (request, ct) => await ExecuteHttpRequestAsync(request, ct));
        }

        public void SetAuthToken(string token)
        {
            lock (lockObj)
            {
                authToken = token;
            }
        }

        public void ClearAuthToken()
        {
            lock (lockObj)
            {
                authToken = null;
            }
        }

        public void SetDefaultOptions(RequestOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }
            
            lock (lockObj)
            {
                defaultOptions = options;
            }
        }

        public void EnableTokenLogging(bool enabled)
        {
            lock (lockObj)
            {
                loggingMiddleware.EnableTokenLogging(enabled);
            }
        }

        /// <summary>
        /// Основной метод отправки HTTP запроса через UnityWebRequest
        /// </summary>
        private async UniTask<HttpResponse> ExecuteHttpRequestAsync(HttpRequest request, CancellationToken ct)
        {
            string currentToken;
            lock (lockObj)
            {
                currentToken = authToken;
            }
            
            // Добавляем токен авторизации если есть
            if (!string.IsNullOrEmpty(currentToken))
            {
                request.Headers["Authorization"] = $"Bearer {currentToken}";
            }
            
            // Создаём UnityWebRequest
            using var unityRequest = new UnityWebRequest(request.Url, request.Method)
            {
                timeout = request.TimeoutSeconds
            };
            
            // Добавляем заголовки
            foreach (var header in request.Headers)
            {
                unityRequest.SetRequestHeader(header.Key, header.Value);
            }
            
            // Сериализуем данные если есть
            if (request.Data != null)
            {
                try
                {
                    var serializedData = Serializer.Serialize(request.Data);
                    if (serializedData != null && serializedData.Length > 0)
                    {
                        unityRequest.uploadHandler = new UploadHandlerRaw(serializedData);
                        
                        // Добавляем заголовки сериализатора
                        foreach (var header in Serializer.GetHeaders())
                        {
                            unityRequest.SetRequestHeader(header.Key, header.Value);
                        }
                    }
                }
                catch (Exception ex)
                {
                    return new HttpResponse
                    {
                        Success = false,
                        StatusCode = HttpStatusCode.InternalServerError,
                        Error = $"Serialization failed: {ex.Message}"
                    };
                }
            }
            
            // Устанавливаем download handler
            unityRequest.downloadHandler = new DownloadHandlerBuffer();
            
            // Выполняем запрос
            try
            {
                await unityRequest.SendWebRequest();
                
                var statusCode = (HttpStatusCode)unityRequest.responseCode;
                var responseBody = unityRequest.downloadHandler.text;
                
                if (unityRequest.result != UnityWebRequest.Result.Success)
                {
                    return new HttpResponse
                    {
                        Success = false,
                        StatusCode = statusCode,
                        ResponseBody = responseBody,
                        Error = unityRequest.error
                    };
                }
                
                // Проверяем ApiResponse формат
                ApiResponse<object> apiResponse = null;
                if (!string.IsNullOrEmpty(responseBody))
                {
                    try
                    {
                        apiResponse = JsonConvert.DeserializeObject<ApiResponse<object>>(responseBody);
                    }
                    catch
                    {
                        // Если не удаётся распарсить как ApiResponse, возвращаем как есть
                    }
                }
                
                var success = unityRequest.result == UnityWebRequest.Result.Success;
                if (apiResponse != null)
                {
                    success = success && apiResponse.Success;
                }
                
                return new HttpResponse
                {
                    Success = success,
                    StatusCode = statusCode,
                    ResponseBody = responseBody,
                        Error = success ? null : (apiResponse?.Error?.Description ?? unityRequest.error)
                };
            }
            catch (Exception ex)
            {
                return new HttpResponse
                {
                    Success = false,
                    StatusCode = 0, // Connection error
                    ResponseBody = null,
                    Error = ex.Message
                };
            }
        }

        /// <summary>
        /// Выполняет запрос через pipeline и парсит ответ
        /// </summary>
        private async UniTask<T> SendRequestCore<T>(
            string endpoint, 
            string method, 
            object data, 
            CancellationToken ct,
            RequestOptions options = null)
        {
            var effectiveOptions = options ?? defaultOptions;
            var url = $"{baseUrl}/{endpoint}";
            
            // Создаём HTTP запрос
            var request = new HttpRequest
            {
                Method = method,
                Url = url,
                Data = data,
                TimeoutSeconds = effectiveOptions.TimeoutSeconds,
                Headers = new System.Collections.Generic.Dictionary<string, string>()
            };
            
            // Выполняем через pipeline
            var response = await pipeline.ExecuteAsync(request, ct);
            
            // Обрабатываем ошибки
            if (!response.Success)
            {
                var errorDto = new ErrorDto(response.Error ?? "Unknown Error", "");
                throw CreateException(response.StatusCode, errorDto);
            }
            
            // Парсим ответ
            if (string.IsNullOrEmpty(response.ResponseBody))
            {
                throw new Exception("Empty response body");
            }
            
            var apiResponse = JsonConvert.DeserializeObject<ApiResponse<T>>(response.ResponseBody);
            
            if (apiResponse == null || !apiResponse.Success)
            {
                var errorDto = apiResponse?.Error ?? new ErrorDto("Unknown Error", "API response parsing failed");
                var errorDescription = errorDto.Description ?? errorDto.Header ?? "Unknown error";
                throw CreateException(response.StatusCode, errorDto, errorDescription);
            }
            
            return apiResponse.Data;
        }

        /// <summary>
        /// Выполняет запрос без возврата данных
        /// </summary>
        private async UniTask SendRequestWithoutResponse(
            string endpoint, 
            string method, 
            object data, 
            CancellationToken ct,
            RequestOptions options = null)
        {
            var effectiveOptions = options ?? defaultOptions;
            var url = $"{baseUrl}/{endpoint}";
            
            var request = new HttpRequest
            {
                Method = method,
                Url = url,
                Data = data,
                TimeoutSeconds = effectiveOptions.TimeoutSeconds
            };
            
            var response = await pipeline.ExecuteAsync(request, ct);
            
            if (!response.Success)
            {
                var errorDto = new ErrorDto(response.Error ?? "Unknown Error", "");
                throw CreateException(response.StatusCode, errorDto);
            }
            
            return;
        }

        public async UniTask<T> GetAsync<T>(string endpoint, CancellationToken ct, RequestOptions options = null)
        {
            return await SendRequestCore<T>(endpoint, "GET", null, ct, options);
        }

        public async UniTask<T> PostAsync<T>(string endpoint, object data, CancellationToken ct, RequestOptions options = null)
        {
            return await SendRequestCore<T>(endpoint, "POST", data, ct, options);
        }

        public UniTask PostAsync(string endpoint, object data, CancellationToken ct, RequestOptions options = null) =>
            SendRequestWithoutResponse(endpoint, "POST", data, ct, options);

        public async UniTask<T> PutAsync<T>(string endpoint, object data, CancellationToken ct, RequestOptions options = null)
        {
            return await SendRequestCore<T>(endpoint, "PUT", data, ct, options);
        }

        public UniTask PutAsync(string endpoint, object data, CancellationToken ct, RequestOptions options = null) =>
            SendRequestWithoutResponse(endpoint, "PUT", data, ct, options);

        public async UniTask<T> DeleteAsync<T>(string endpoint, CancellationToken ct, RequestOptions options = null)
        {
            return await SendRequestCore<T>(endpoint, "DELETE", null, ct, options);
        }

        public UniTask DeleteAsync(string endpoint, CancellationToken ct, RequestOptions options = null) =>
            SendRequestWithoutResponse(endpoint, "DELETE", null, ct, options);

        public UniTask DeleteAsync(string endpoint, object data, CancellationToken ct, RequestOptions options = null) =>
            SendRequestWithoutResponse(endpoint, "DELETE", data, ct, options);

        private Energy8Exception CreateException(HttpStatusCode statusCode, ErrorDto errorDto, string errorDescription = null)
        {
            var errorMessage = errorDescription ?? errorDto?.Description ?? errorDto?.Header ?? "Unknown error";
            
            return statusCode switch
            {
                0 => new Energy8Exception("Connection Error", "", canRetry: true),
                HttpStatusCode.BadRequest => new ValidationException(errorDto),
                HttpStatusCode.Unauthorized => new AuthenticationException(errorDto),
                HttpStatusCode.Forbidden => new AuthorizationException(errorDto),
                HttpStatusCode.NotFound => new NotFoundException(errorDto),
                HttpStatusCode.InternalServerError => new ServerException(errorDto),
                HttpStatusCode.BadGateway => new ServerException(errorDto),
                HttpStatusCode.ServiceUnavailable => new ServerException(errorDto),
                HttpStatusCode.GatewayTimeout => new ServerException(errorDto),
                HttpStatusCode.RequestTimeout => new Energy8Exception("Request Timeout", errorMessage, canRetry: true),
                _ => new Energy8Exception("Unknown Error", "Error didn't handled by E8 Identity.", true, true, true)
            };
        }

        static public ErrorDto ValiadateErrorResponse(HttpStatusCode code)
        {
            ErrorDto validateErrorMessage = new("Unknown Error",
                "The client is unable to validate this error, as it <b>does not align with the expected behavior</b> of the application.\n\n" +
                "<b>Please reach out to support for further assistance.</b>");
            
            switch (code)
            {
                case 0:
                    validateErrorMessage = new("Connection Error", "The server is not responding to the request. Most likely the problem is in your network connection.\n" +
                        "Please check that you have internet access and try again later.");
                    return validateErrorMessage;
                case HttpStatusCode.OK:
                    validateErrorMessage = new("Connection Error", "The server is not responding to the request. Most likely the problem is in your network connection.\n" +
                        "Please check that you have internet access and try again later.");
                    return validateErrorMessage;
                case HttpStatusCode.BadGateway:
                    validateErrorMessage = new("Server Error", "<b>Error on the <u>server</u> side!</b>\n" +
                        "It is possible that technical work is underway on the server.\n\n" +
                        "<b><u>Try again or wait, please.</u></b>");
                    return validateErrorMessage;
                case HttpStatusCode.GatewayTimeout:
                    validateErrorMessage = new("Server Error", "<b>Error on the <u>server</u> side!</b>\n" +
                        "The server took too long to respond.\n\n" +
                        "<b><u>Try again or wait, please.</u></b>");
                    return validateErrorMessage;
                case HttpStatusCode.InternalServerError:
                    validateErrorMessage = new("Server Error", "<b>Error on the <u>server</u> side!</b>\n" +
                        "It is possible that technical work is underway on the server.\n\n" +
                        "<b><u>Try again or wait, please.</u></b>");
                    return validateErrorMessage;
                case HttpStatusCode.BadRequest:
                    validateErrorMessage = new("System error",
                        "Error processing the request.\n" +
                        "Please try again, try restart the app and contact support to report the problem.");
                    return validateErrorMessage;
                case HttpStatusCode.Unauthorized:
                    validateErrorMessage = new("Authorization Error",
                        "<b>Your device is not authorized.</b>\n" +
                        "The key may have been deleted or the authorization session of this device is outdated.\n" +
                        "<b><u>Try again</u> and, in case of an error, <u>Re-Login</u>.</b>");
                    return validateErrorMessage;
            }
            return validateErrorMessage;
        }
    }
}
