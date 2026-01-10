using System;
using System.Net;
using System.Threading;
using Cysharp.Threading.Tasks;

using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

using Energy8.Identity.Shared.Core.Contracts.Dto.Common;
using Energy8.Identity.Shared.Core.Contracts.Dto.Errors;
using Energy8.Identity.Shared.Core.Exceptions;

using Energy8.Identity.Http.Core;
using Energy8.Identity.Http.Core.Models;

namespace Energy8.Identity.Http.Runtime.Clients
{
    public class UnityHttpClient : IHttpClient
    {
        private readonly string baseUrl;
        private string authToken;
        private bool tokenLoggingEnabled = false;
        private int retryCount = 3;
        private int timeoutSeconds = 30;

        public string BaseUrl => baseUrl;

        public UnityHttpClient(string baseUrl)
        {
            this.baseUrl = baseUrl ?? throw new ArgumentNullException(nameof(baseUrl));
        }

        public void SetAuthToken(string token)
        {
            authToken = token;
        }

        public void ClearAuthToken()
        {
            authToken = null;
        }

        public void EnableTokenLogging(bool enabled)
        {
            tokenLoggingEnabled = enabled;
        }

        private Energy8Exception CreateException(HttpStatusCode statusCode, ErrorDto error)
        {
            return statusCode switch
            {
                0 => new Energy8Exception("Connection Error", "", canRetry: true),
                HttpStatusCode.BadRequest => new ValidationException(error),
                HttpStatusCode.Unauthorized => new AuthenticationException(error),
                HttpStatusCode.Forbidden => new AuthorizationException(error),
                HttpStatusCode.NotFound => new NotFoundException(error),
                HttpStatusCode.InternalServerError => new ServerException(error),
                HttpStatusCode.BadGateway => new ServerException(error),
                HttpStatusCode.ServiceUnavailable => new ServerException(error),
                HttpStatusCode.GatewayTimeout => new ServerException(error),
                HttpStatusCode.RequestTimeout => new Energy8Exception("Request Timeout", "The request timed out.", canRetry: true),
                _ => new Energy8Exception("Unknown Error", "Error didn't handled by E8 Identity.",
                    true, true, true)
            };
        }

        /// <summary>
        /// Проверяет, является ли ошибка временной и warrants retry
        /// </summary>
        private bool ShouldRetry(HttpStatusCode statusCode, Exception ex)
        {
            if (statusCode == HttpStatusCode.ServiceUnavailable || 
                statusCode == HttpStatusCode.BadGateway || 
                statusCode == HttpStatusCode.GatewayTimeout ||
                statusCode == 0) // Connection error
            {
                return true;
            }

            if (ex is WebException || ex is TimeoutException)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Выполняет экспоненциальный backoff для retry
        /// </summary>
        private async UniTask DelayWithBackoff(int attempt, CancellationToken ct)
        {
            var delayMs = (long)Math.Pow(2, attempt) * 1000; // 1s, 2s, 4s, 8s...
            var maxDelayMs = 5000; // Максимум 5 секунд
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

        /// <summary>
        /// Универсальный метод отправки запроса с retry и timeout
        /// </summary>
        private async UniTask<TResponse> SendRequestCore<TResponse>(
            string endpoint, 
            string method, 
            object data, 
            CancellationToken ct,
            bool expectResponse = true)
        {
            var url = $"{baseUrl}/{endpoint}";
            HttpStatusCode statusCode = HttpStatusCode.OK;
            Exception lastException = null;
            ApiResponse<TResponse> lastResponse = null;

            // Retry loop
            for (int attempt = 0; attempt <= retryCount; attempt++)
            {
                using var request = new UnityWebRequest(url, method);
                
                var log = new HttpRequestLog
                {
                    Method = method,
                    Url = url,
                    Attempt = attempt + 1,
                    TotalAttempts = retryCount + 1
                };

                if (!string.IsNullOrEmpty(authToken))
                {
                    request.SetRequestHeader("Authorization", $"Bearer {authToken}");
                    log.Headers["Authorization"] = tokenLoggingEnabled ? $"Bearer {authToken}" : "***";
                }

                object requestData = null;
                if (data != null)
                {
                    var formData = new WWWForm();
                    if (data is DtoBase model)
                    {
                        requestData = model.ToDictionary();
                        foreach (var pair in model.ToDictionary())
                        {
                            if (pair.Value != null)
                                formData.AddField(pair.Key, pair.Value.ToString());
                        }
                    }
                    else
                    {
                        requestData = data;
                        var dictionary = data as IDictionary<string, object>
                            ?? JsonConvert.DeserializeObject<Dictionary<string, object>>(JsonConvert.SerializeObject(data));
                        foreach (var pair in dictionary)
                        {
                            if (pair.Value != null)
                                formData.AddField(pair.Key, pair.Value.ToString());
                        }
                    }

                    request.uploadHandler = new UploadHandlerRaw(formData.data);
                    foreach (var header in formData.headers)
                    {
                        request.SetRequestHeader(header.Key, header.Value);
                        log.Headers[header.Key] = header.Value;
                    }
                }
                
                log.RequestData = requestData;
                request.downloadHandler = new DownloadHandlerBuffer();
                request.timeout = timeoutSeconds;

                var stopwatch = System.Diagnostics.Stopwatch.StartNew();

                try
                {
                    await request.SendWebRequest();

                    stopwatch.Stop();
                    log.Duration = stopwatch.ElapsedMilliseconds;
                    log.ResponseCode = request.responseCode;
                    log.ResponseBody = request.downloadHandler.text;

                    statusCode = (HttpStatusCode)request.responseCode;

                    if (expectResponse)
                    {
                        var responseText = request.downloadHandler.text;
                        lastResponse = JsonConvert.DeserializeObject<ApiResponse<TResponse>>(responseText);

                        if (request.result != UnityWebRequest.Result.Success || 
                            (lastResponse != null && !lastResponse.Success))
                        {
                            log.Success = false;
                            log.ErrorMessage = request.error;
                            
                            Debug.Log("[Identity HTTP] " + JsonConvert.SerializeObject(log, Formatting.Indented));
                            
                            var error = lastResponse?.Error ?? ValiadateErrorResponse(statusCode);
                            
                            if (attempt < retryCount && ShouldRetry(statusCode, null))
                            {
                                lastException = CreateException(statusCode, error);
                                await DelayWithBackoff(attempt, ct);
                                continue;
                            }
                            
                            throw CreateException(statusCode, error);
                        }
                    }
                    else
                    {
                        if (request.result != UnityWebRequest.Result.Success)
                        {
                            log.Success = false;
                            log.ErrorMessage = request.error;
                            
                            Debug.Log("[Identity HTTP] " + JsonConvert.SerializeObject(log, Formatting.Indented));
                            
                            var error = ValiadateErrorResponse(statusCode);
                            
                            if (attempt < retryCount && ShouldRetry(statusCode, null))
                            {
                                lastException = CreateException(statusCode, error);
                                await DelayWithBackoff(attempt, ct);
                                continue;
                            }
                            
                            throw CreateException(statusCode, error);
                        }
                    }

                    log.Success = true;
                    Debug.Log("[Identity HTTP] " + JsonConvert.SerializeObject(log, Formatting.Indented));
                    
                    return expectResponse ? lastResponse.Data : default;
                }
                catch (Exception ex) when (ex is not ApiException)
                {
                    stopwatch.Stop();
                    log.Duration = stopwatch.ElapsedMilliseconds;
                    log.ResponseCode = request.responseCode;
                    log.Success = false;
                    log.ErrorMessage = ex.Message;
                    log.ResponseBody = request.downloadHandler?.text;
                    
                    Debug.Log("[Identity HTTP] " + JsonConvert.SerializeObject(log, Formatting.Indented));
                    
                    lastException = ex;
                    var error = ValiadateErrorResponse(statusCode);
                    
                    if (attempt < retryCount && ShouldRetry(statusCode, ex))
                    {
                        await DelayWithBackoff(attempt, ct);
                        continue;
                    }
                    
                    throw CreateException(statusCode, error);
                }
            }

            // Если мы здесь, значит все попытки исчерпаны
            throw lastException ?? new Energy8Exception("Request failed after retries", "", canRetry: true);
        }

        public async UniTask<T> GetAsync<T>(string endpoint, CancellationToken ct)
        {
            return await SendRequestCore<T>(endpoint, "GET", null, ct);
        }

        public async UniTask<T> PostAsync<T>(string endpoint, object data, CancellationToken ct)
        {
            return await SendRequestCore<T>(endpoint, "POST", data, ct);
        }

        public async UniTask<T> PutAsync<T>(string endpoint, object data, CancellationToken ct)
        {
            return await SendRequestCore<T>(endpoint, "PUT", data, ct);
        }

        public async UniTask<T> DeleteAsync<T>(string endpoint, CancellationToken ct)
        {
            return await SendRequestCore<T>(endpoint, "DELETE", null, ct);
        }

        public UniTask PostAsync(string endpoint, object data, CancellationToken ct) =>
            SendRequestCore<object>(endpoint, "POST", data, ct, expectResponse: false);

        public UniTask PutAsync(string endpoint, object data, CancellationToken ct) =>
            SendRequestCore<object>(endpoint, "PUT", data, ct, expectResponse: false);

        public UniTask DeleteAsync(string endpoint, CancellationToken ct) =>
            SendRequestCore<object>(endpoint, "DELETE", null, ct, expectResponse: false);

        public UniTask DeleteAsync(string endpoint, object data, CancellationToken ct) =>
            SendRequestCore<object>(endpoint, "DELETE", data, ct, expectResponse: false);

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
