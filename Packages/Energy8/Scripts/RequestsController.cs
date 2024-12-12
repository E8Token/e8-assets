using Cysharp.Threading.Tasks;
using System;
using System.Linq;
using UnityEngine.Networking;
using UnityEngine;
using Energy8.Models;
using System.Net;
using Energy8.Models.Errors;
using System.Collections.Generic;

namespace Energy8.Requests
{
    public class RequestsController
    {
        public const string ApiEndpoint = "/api/";

        public const string PostMethod = "POST";
        public const string PutMethod = "PUT";
        public const string GetMethod = "GET";
        public const string DeleteMethod = "DELETE";

        public const string AuthorizationHeader = "Authorization";
        public const string AuthorizationBearer = "Bearer ";
        public const string AuthorizationServer = "Server ";

        static readonly Logger _logger = new(null, "RequestsController", new Color(0.5f, 0.8f, 0f));
        static string RemoteAddress => ApplicationConfig.SelectedIP;

        public static UniTask Post(
            string endpoint,
            AuthorizationType authorizationType = AuthorizationType.None,
            string authorizationData = "",
            Data requestData = null,
            params (string key, string value)[] requestDataFields)
        {
            _logger.Log("Post");
            if (authorizationData is null)
                return Send(CreateRequest(endpoint, PostMethod, authorizationType, authorizationData, GetFormData(requestDataFields)));
            else
                return Send(CreateRequest(endpoint, PostMethod, authorizationType, authorizationData, GetFormData(requestData)));
        }

        public static UniTask<T> Post<T>(
            string endpoint,
            AuthorizationType authorizationType = AuthorizationType.None,
            string authorizationData = "",
            Data requestData = null,
            params (string key, string value)[] requestDataFields) where T : Data
        {
            _logger.Log("Post");
            if (requestData is null)
                return Send<T>(CreateRequest(endpoint, PostMethod, authorizationType, authorizationData, GetFormData(requestDataFields)));
            else
                return Send<T>(CreateRequest(endpoint, PostMethod, authorizationType, authorizationData, GetFormData(requestData)));
        }

        public static UniTask Put(
            string endpoint,
            AuthorizationType authorizationType = AuthorizationType.None,
            string authorizationData = "",
            Data requestData = null,
            params (string key, string value)[] requestDataFields)
        {
            if (requestData is null)
                return Send(CreateRequest(endpoint, PutMethod, authorizationType, authorizationData, GetFormData(requestDataFields)));
            else
                return Send(CreateRequest(endpoint, PutMethod, authorizationType, authorizationData, GetFormData(requestData)));
        }

        public static UniTask<T> Put<T>(
            string endpoint,
            AuthorizationType authorizationType = AuthorizationType.None,
            string authorizationData = "",
            Data requestData = null,
            params (string key, string value)[] requestDataFields) where T : Data
        {
            if (requestData is null)
                return Send<T>(CreateRequest(endpoint, PutMethod, authorizationType, authorizationData, GetFormData(requestDataFields)));
            else
                return Send<T>(CreateRequest(endpoint, PutMethod, authorizationType, authorizationData, GetFormData(requestData)));
        }


        public static UniTask Delete(
            string endpoint,
            AuthorizationType authorizationType = AuthorizationType.None,
            string authorizationData = "",
            Data requestData = null,
            params (string key, string value)[] requestDataFields)
        {
            if (requestData is null)
                return Send(CreateRequest(endpoint, DeleteMethod, authorizationType, authorizationData, GetFormData(requestDataFields)));
            else
                return Send(CreateRequest(endpoint, DeleteMethod, authorizationType, authorizationData, GetFormData(requestData)));
        }

        public static UniTask<T> Delete<T>(
            string endpoint,
            AuthorizationType authorizationType = AuthorizationType.None,
            string authorizationData = "",
            Data requestData = null,
            params (string key, string value)[] requestDataFields) where T : Data
        {
            if (requestData is null)
                return Send<T>(CreateRequest(endpoint, DeleteMethod, authorizationType, authorizationData, GetFormData(requestDataFields)));
            else
                return Send<T>(CreateRequest(endpoint, DeleteMethod, authorizationType, authorizationData, GetFormData(requestData)));
        }


        public static UniTask Get(
            string endpoint,
            AuthorizationType authorizationType,
            string authorizationData = "",
            params (string key, string value)[] headers) =>
                Send(CreateRequest(endpoint, GetMethod, authorizationType, authorizationData, headers: GetRequestHeaders(headers)));

        public static UniTask<T> Get<T>(
            string endpoint,
            AuthorizationType authorizationType,
            string authorizationData = "",
            params (string key, string value)[] headers) where T : Data =>
                Send<T>(CreateRequest(endpoint, GetMethod, authorizationType, authorizationData, headers: GetRequestHeaders(headers)));

        static WWWForm GetFormData(params (string key, string value)[] fields)
        {
            WWWForm form = new();
            foreach (var (key, value) in fields)
                form.AddField(key, value);
            return form;
        }
        static WWWForm GetFormData(Data data)
        {
            WWWForm form = new();
            var headers = data?.ToDictionary();
            foreach (var key in headers.Keys)
                form.AddField(key, headers[key]);
            return form;
        }

        static Dictionary<string, string> GetRequestHeaders(params (string key, string value)[] headers) =>
            headers.ToDictionary((header) => header.key, (header) => header.value);
        static Dictionary<string, string> GetRequestHeaders(Data data) =>
            data.ToDictionary();

        static UnityWebRequest CreateRequest(
            string endpoint,
            string method,
            AuthorizationType authorizationType,
            string authorizationData = "",
            WWWForm form = null,
            Dictionary<string, string> headers = null)
        {
            UnityWebRequest request;

            request = form == null ? UnityWebRequest.Get(RemoteAddress + ApiEndpoint + endpoint) :
                                     UnityWebRequest.Post(RemoteAddress + ApiEndpoint + endpoint, form);

            request.method = method;

            request.SetRequestHeader(AuthorizationHeader, authorizationType switch
            {
                AuthorizationType.Bearer => AuthorizationBearer,
                AuthorizationType.Server => AuthorizationServer,
                _ => string.Empty
            } + authorizationData);

            if (headers is not null)
                foreach (var key in headers.Keys)
                    request.SetRequestHeader(key, headers[key]);

            return request;
        }

        static async UniTask<T> Send<T>(UnityWebRequest request) where T : Data
        {
            string requestLog = $"Send({request.uri}, {request.method}) : ";
            try
            {
                await request.SendWebRequest();
                if (request.downloadHandler.text is null)
                    throw new RequestErrorDataException(HttpStatusCode.NotFound, "Request Error", "Data returned from server is empty.");

                if (!Data.TryFromJson(request.downloadHandler.text, out T data, false))
                {
                    _logger.LogWarning(requestLog + "Data returned from server is invalid: " + request.downloadHandler.text);
                    throw new RequestErrorDataException(HttpStatusCode.UnprocessableEntity, "Request Error", "Data returned from server is invalid.");
                }

                _logger.Log(requestLog + $"{request.responseCode}, {data}");
                return data;
            }
            catch
            {
                if (!Data.TryFromJson(request.downloadHandler.text, out ErrorData errorData))
                    errorData = ValiadateErrorResponse((HttpStatusCode)request.responseCode);
                _logger.LogWarning(requestLog + $"{request.responseCode}, {errorData}");
                throw new RequestErrorDataException((HttpStatusCode)request.responseCode, errorData.Header, errorData.Description, errorData.CanProceed, errorData.CanRetry, errorData.MustSignOut);
            }
        }
        static async UniTask Send(UnityWebRequest request)
        {
            string requestLog = $"Send({request.uri}, {request.method}) : ";
            try
            {
                await request.SendWebRequest();
                _logger.Log(requestLog + $"{request.responseCode}");
            }
            catch
            {
                if (!Data.TryFromJson(request.downloadHandler.text, out ErrorData errorData))
                    errorData = ValiadateErrorResponse((HttpStatusCode)request.responseCode);
                _logger.LogWarning(requestLog + $"{request.responseCode}, {errorData}");
                throw new RequestErrorDataException((HttpStatusCode)request.responseCode, errorData.Header, errorData.Description, errorData.CanProceed, errorData.CanRetry, errorData.MustSignOut);
            }
        }

        static public ErrorData ValiadateErrorResponse(HttpStatusCode code)
        {
            ErrorData validateErrorMessage = new("Unknown Error");
            switch (code)
            {
                case HttpStatusCode.OK:
                    validateErrorMessage = new("Connection Error", "The server is not responding to the request. Most likely the problem is in your network connection.\n" +
                        "Please check that you have internet access and try again later.", canProceed: true);
                    return validateErrorMessage;
                case HttpStatusCode.BadGateway:
                    validateErrorMessage = new("Server Error", "<b>Error on the <u>server</u> side!</b>\n" +
                        "It is possible that technical work is underway on the server.\n\n" +
                        "<b><u>Try again or wait, please.</u></b>", canProceed: true);
                    return validateErrorMessage;
                case HttpStatusCode.InternalServerError:
                    validateErrorMessage = new("Server Error", "<b>Error on the <u>server</u> side!</b>\n" +
                        "It is possible that technical work is underway on the server.\n\n" +
                        "<b><u>Try again or wait, please.</u></b>", canProceed: true);
                    return validateErrorMessage;
                case HttpStatusCode.BadRequest:
                    validateErrorMessage = new("System error",
                        "Error processing the request.\n" +
                        "Please try again, try restart the app and contact support to report the problem.", canProceed: true);
                    return validateErrorMessage;
                case HttpStatusCode.Unauthorized:
                    validateErrorMessage = new("Authorization Error",
                        "<b>Your device is not authorized.</b>\n" +
                        "The key may have been deleted or the authorization session of this device is outdated.\n" +
                        "<b><u>Try again</u> and, in case of an error, <u>Re-Login</u>.</b>", canProceed: true, mustSignOut: true);
                    return validateErrorMessage;
            }
            return validateErrorMessage;
        }
    }
}
public enum AuthorizationType
{
    None,
    Bearer,
    Server
}