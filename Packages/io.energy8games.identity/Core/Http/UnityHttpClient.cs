using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using Cysharp.Threading.Tasks;
using Energy8.Core.Http.Models;
using Energy8.Identity.Core.Logging;
using Newtonsoft.Json;
using UnityEngine.Networking;
using System.Collections.Generic;
using UnityEngine;
using Energy8.Contracts.Dto.Common;
using Energy8.Contracts.Dto.Errors;
using Energy8.Core.Exceptions;

namespace Energy8.Identity.Core.Http
{
    public class UnityHttpClient : IHttpClient
    {
        private readonly ILogger<UnityHttpClient> logger = new Logger<UnityHttpClient>();
        private readonly string baseUrl;
        private string authToken;

        public string BaseUrl => baseUrl;

        public UnityHttpClient(string baseUrl)
        {
            this.baseUrl = baseUrl ?? throw new ArgumentNullException(nameof(baseUrl));
        }

        public void SetAuthToken(string token)
        {
            authToken = token;
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
                _ => new Energy8Exception("Unknown Error", "Error didn't handled by E8 Identity.",
                    true, true, true)
            };
        }

        private async UniTask<T> SendRequest<T>(string endpoint, string method, object data, CancellationToken ct)
        {
            var url = $"{baseUrl}/api/v1/{endpoint}";
            using var request = new UnityWebRequest(url, method);

            if (!string.IsNullOrEmpty(authToken))
            {
                request.SetRequestHeader("Authorization", $"Bearer {authToken}");
            }

            if (data != null)
            {
                var formData = new WWWForm();
                if (data is DtoBase model)
                {
                    foreach (var pair in model.ToDictionary())
                    {
                        if (pair.Value != null)
                            formData.AddField(pair.Key, pair.Value.ToString());
                    }
                    logger.LogDebug($"Request data (DtoBase): {JsonConvert.SerializeObject(model.ToDictionary())}");
                }
                else
                {
                    var dictionary = data as IDictionary<string, object>
                        ?? JsonConvert.DeserializeObject<Dictionary<string, object>>(JsonConvert.SerializeObject(data));
                    foreach (var pair in dictionary)
                    {
                        if (pair.Value != null)
                            formData.AddField(pair.Key, pair.Value.ToString());
                    }
                    logger.LogDebug($"Request data: {JsonConvert.SerializeObject(dictionary)}");
                }

                request.uploadHandler = new UploadHandlerRaw(formData.data);
                foreach (var header in formData.headers)
                {
                    request.SetRequestHeader(header.Key, header.Value);
                }
            }

            request.downloadHandler = new DownloadHandlerBuffer();

            try
            {
                logger.LogDebug($"Sending {method} request to {url}");
                await request.SendWebRequest();

                var responseText = request.downloadHandler.text;
                var response = JsonConvert.DeserializeObject<ApiResponse<T>>(responseText);

                if (request.result != UnityWebRequest.Result.Success || !response.Success)
                {
                    logger.LogError($"Request failed: [{request.responseCode}] {request.error}\nResponse: {responseText}");
                    var error = response?.Error ?? ValiadateErrorResponse((HttpStatusCode)request.responseCode);
                    throw CreateException((HttpStatusCode)request.responseCode, error);
                }

                logger.LogDebug($"Request succeeded: [{request.responseCode}]\nResponse: {JsonConvert.SerializeObject(response, Formatting.Indented)}");
                return response.Data;
            }
            catch (Exception ex) when (ex is not ApiException)
            {
                logger.LogError($"Request error: {ex.Message}\nStack trace: {ex.StackTrace}");
                var error = ValiadateErrorResponse((HttpStatusCode)request.responseCode);
                throw CreateException((HttpStatusCode)request.responseCode, error);
            }
        }

        private async UniTask SendRequest(string endpoint, string method, object data, CancellationToken ct)
        {
            var url = $"{baseUrl}/api/v1/{endpoint}";
            using var request = new UnityWebRequest(url, method);

            if (!string.IsNullOrEmpty(authToken))
            {
                request.SetRequestHeader("Authorization", $"Bearer {authToken}");
            }

            if (data != null)
            {
                var formData = new WWWForm();
                if (data is DtoBase model)
                {
                    foreach (var pair in model.ToDictionary())
                    {
                        if (pair.Value != null)
                            formData.AddField(pair.Key, pair.Value.ToString());
                    }
                    logger.LogDebug($"Request data (DtoBase): {JsonConvert.SerializeObject(model.ToDictionary())}");
                }
                else
                {
                    var dictionary = data as IDictionary<string, object>
                        ?? JsonConvert.DeserializeObject<Dictionary<string, object>>(JsonConvert.SerializeObject(data));
                    foreach (var pair in dictionary)
                    {
                        if (pair.Value != null)
                            formData.AddField(pair.Key, pair.Value.ToString());
                    }
                    logger.LogDebug($"Request data: {JsonConvert.SerializeObject(dictionary)}");
                }

                request.uploadHandler = new UploadHandlerRaw(formData.data);
                foreach (var header in formData.headers)
                {
                    request.SetRequestHeader(header.Key, header.Value);
                }
            }

            request.downloadHandler = new DownloadHandlerBuffer();

            try
            {
                logger.LogDebug($"Sending {method} request to {url}");
                await request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    logger.LogError($"Request failed: [{request.responseCode}] {request.error}\n");
                    var error = ValiadateErrorResponse((HttpStatusCode)request.responseCode);
                    throw CreateException((HttpStatusCode)request.responseCode, error);
                }

                logger.LogDebug($"Request succeeded: [{request.responseCode}]\n");
            }
            catch (Exception ex) when (ex is not ApiException)
            {
                logger.LogError($"Request error: {ex.Message}\nStack trace: {ex.StackTrace}");
                var error = ValiadateErrorResponse((HttpStatusCode)request.responseCode);
                throw CreateException((HttpStatusCode)request.responseCode, error);
            }
        }

        public async UniTask<T> GetAsync<T>(string endpoint, CancellationToken ct)
        {
            var response = await SendRequest<T>(endpoint, "GET", null, ct);
            return response;
        }

        public async UniTask<T> PostAsync<T>(string endpoint, object data, CancellationToken ct)
        {
            var response = await SendRequest<T>(endpoint, "POST", data, ct);
            return response;
        }

        public async UniTask<T> PutAsync<T>(string endpoint, object data, CancellationToken ct)
        {
            var response = await SendRequest<T>(endpoint, "PUT", data, ct);
            return response;
        }

        public async UniTask<T> DeleteAsync<T>(string endpoint, CancellationToken ct)
        {
            var response = await SendRequest<T>(endpoint, "DELETE", null, ct);
            return response;
        }

        public UniTask PostAsync(string endpoint, object data, CancellationToken ct) =>
            SendRequest(endpoint, "POST", data, ct);

        public UniTask PutAsync(string endpoint, object data, CancellationToken ct) =>
            SendRequest(endpoint, "PUT", data, ct);

        public UniTask DeleteAsync(string endpoint, CancellationToken ct) =>
            SendRequest(endpoint, "DELETE", null, ct);

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

        public UniTask DeleteAsync(string endpoint, object data, CancellationToken ct) =>
            SendRequest(endpoint, "DELETE", data, ct);
    }
}