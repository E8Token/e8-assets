using Cysharp.Threading.Tasks;
using System;
using UnityEngine.Networking;
using UnityEngine;
using Energy8.Models;
using Energy8.Models.Requests;

namespace Energy8.Requests
{
    public class RequestsController
    {
        public const string API_ENDPOINT = "/api/";
        public const string POST_METHOD = "POST";
        public const string PUT_METHOD = "PUT";
        public const string GET_METHOD = "GET";
        public const string DELETE_METHOD = "DELETE";
        public const string AUTHORIZATION_HEADER = "Authorization";
        public const string AUTHORIZATION_BEARER = "Bearer";
        public const string AUTHORIZATION_SERVER = "Server";

        static readonly Logger logger = new(null, "RequestsController", new Color(0.5f, 0.8f, 0f));
        static string RemoteAddress => ApplicationConfig.SelectedIP;

        public static async UniTask<WebTryResult> Post(string endpoint, AuthorizationType authorizationType,
            string authorizationData = "", Data requestData = null) =>
            await Send(CreateRequest(endpoint, POST_METHOD, authorizationType, authorizationData, GetFormData(requestData)));
        public static async UniTask<WebTryResult> Post(string endpoint, AuthorizationType authorizationType,
            string authorizationData = "", params (string key, string value)[] fields) =>
            await Send(CreateRequest(endpoint, POST_METHOD, authorizationType, authorizationData, GetFormData(fields)));

        public static async UniTask<WebTryResult<T>> Post<T>(string endpoint, AuthorizationType authorizationType,
            string authorizationData = "", Data requestData = null) where T : Data =>
            await Send<T>(CreateRequest(endpoint, POST_METHOD, authorizationType, authorizationData, GetFormData(requestData)));
        public static async UniTask<WebTryResult<T>> Post<T>(string endpoint, AuthorizationType authorizationType,
            string authorizationData = "", params (string key, string value)[] fields) where T : Data =>
            await Send<T>(CreateRequest(endpoint, POST_METHOD, authorizationType, authorizationData, GetFormData(fields)));

        public static async UniTask<WebTryResult> Put(string endpoint, AuthorizationType authorizationType,
            string authorizationData = "", Data requestData = null) =>
            await Send(CreateRequest(endpoint, PUT_METHOD, authorizationType, authorizationData, GetFormData(requestData)));
        public static async UniTask<WebTryResult> Put(string endpoint, AuthorizationType authorizationType,
            string authorizationData = "", params (string key, string value)[] fields) =>
            await Send(CreateRequest(endpoint, PUT_METHOD, authorizationType, authorizationData, GetFormData(fields)));

        public static async UniTask<WebTryResult<T>> Put<T>(string endpoint, AuthorizationType authorizationType,
            string authorizationData = "", Data requestData = null) where T : Data =>
            await Send<T>(CreateRequest(endpoint, PUT_METHOD, authorizationType, authorizationData, GetFormData(requestData)));
        public static async UniTask<WebTryResult<T>> Put<T>(string endpoint, AuthorizationType authorizationType,
            string authorizationData = "", params (string key, string value)[] fields) where T : Data =>
            await Send<T>(CreateRequest(endpoint, PUT_METHOD, authorizationType, authorizationData, GetFormData(fields)));

        public static async UniTask<WebTryResult> Delete(string endpoint, AuthorizationType authorizationType,
            string authorizationData = "", Data requestData = null) =>
            await Send(CreateRequest(endpoint, DELETE_METHOD, authorizationType, authorizationData, GetFormData(requestData)));
        public static async UniTask<WebTryResult> Delete(string endpoint, AuthorizationType authorizationType,
            string authorizationData = "", params (string key, string value)[] fields) =>
            await Send(CreateRequest(endpoint, DELETE_METHOD, authorizationType, authorizationData, GetFormData(fields)));

        public static async UniTask<WebTryResult<T>> Delete<T>(string endpoint, AuthorizationType authorizationType,
            string authorizationData = "", Data requestData = null) where T : Data =>
            await Send<T>(CreateRequest(endpoint, DELETE_METHOD, authorizationType, authorizationData, GetFormData(requestData)));
        public static async UniTask<WebTryResult<T>> Delete<T>(string endpoint, AuthorizationType authorizationType,
            string authorizationData = "", params (string key, string value)[] fields) where T : Data =>
            await Send<T>(CreateRequest(endpoint, DELETE_METHOD, authorizationType, authorizationData, GetFormData(fields)));

        public static async UniTask<WebTryResult> Get(string endpoint, AuthorizationType authorizationType,
            string authorizationData = "", Data requestData = null) =>
            await Send(CreateRequest(endpoint, GET_METHOD, authorizationType, authorizationData, headersData: requestData));
        public static async UniTask<WebTryResult> Get(string endpoint, AuthorizationType authorizationType,
            string authorizationData = "", params (string key, string value)[] fields) =>
            await Send(CreateRequest(endpoint, GET_METHOD, authorizationType, authorizationData, headers: fields));

        public static async UniTask<WebTryResult<T>> Get<T>(string endpoint, AuthorizationType authorizationType,
            string authorizationData = "", Data requestData = null) where T : Data =>
            await Send<T>(CreateRequest(endpoint, GET_METHOD, authorizationType, authorizationData, headersData: requestData));
        public static async UniTask<WebTryResult<T>> Get<T>(string endpoint, AuthorizationType authorizationType,
            string authorizationData = "", params (string key, string value)[] fields) where T : Data =>
            await Send<T>(CreateRequest(endpoint, GET_METHOD, authorizationType, authorizationData, headers: fields));

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
            var fields = data.ToDictionary();
            foreach (var key in fields.Keys)
                form.AddField(key, fields[key]);
            return form;
        }

        static void SetRequestHeaders(ref UnityWebRequest request, params (string key, string value)[] fields)
        {
            foreach (var (key, value) in fields)
                request.SetRequestHeader(key, value);
        }
        static void SetRequestHeaders(ref UnityWebRequest request, Data data)
        {
            if (data == null)
                return;
            var fields = data.ToDictionary();
            foreach (var key in fields.Keys)
                request.SetRequestHeader(key, fields[key]);
        }

        static UnityWebRequest CreateRequest(string endpoint, string method, AuthorizationType authorizationType, string authorizationData = "",
            WWWForm form = null, Data headersData = null, params (string key, string value)[] headers)
        {
            UnityWebRequest request;

            if (form == null)
                request = UnityWebRequest.Get(RemoteAddress + API_ENDPOINT + endpoint);
            else
                request = UnityWebRequest.Post(RemoteAddress + API_ENDPOINT + endpoint, form);

            request.method = method;

            if (authorizationType == AuthorizationType.Bearer)
                request.SetRequestHeader(AUTHORIZATION_HEADER, AUTHORIZATION_BEARER + " " + authorizationData);
            else if (authorizationType == AuthorizationType.Server)
                request.SetRequestHeader(AUTHORIZATION_HEADER, AUTHORIZATION_SERVER + " " + authorizationData);

            if (headersData != null)
                SetRequestHeaders(ref request, headersData);
            if (headers.Length > 0)
                SetRequestHeaders(ref request, headers);

            return request;
        }

        static async UniTask<WebTryResult<T>> Send<T>(UnityWebRequest request) where T : Data
        {
            try
            {
                await request.SendWebRequest();
                var response = WebTryResult<T>.Create(request);
                if (request.responseCode == 200)
                    logger.Log($"Send({request.uri}, {request.method}) : {response.Value}");
                else
                    logger.LogWarning($"Send({request.uri}, {request.method}, {response.StatusCode}) : {response.Error}");
                return response;
            }
            catch (Exception ex)
            {
                var response = WebTryResult<T>.Create(request);
                Debug.Log(response.StatusCode);
                logger.LogWarning($"Send({request.uri}, {request.method}, {response.StatusCode}) : {ex.Message}");
                return response;
            }
        }
        static async UniTask<WebTryResult> Send(UnityWebRequest request)
        {
            try
            {
                await request.SendWebRequest();
                var response = WebTryResult.Create(request);
                if (request.responseCode == 200)
                    logger.Log($"Send({request.uri}, {request.method})");
                else
                    logger.LogWarning($"Send({request.uri}, {request.method}, {response.StatusCode}) : {response.Error}");
                return response;
            }
            catch (Exception ex)
            {
                var response = WebTryResult.Create(request);
                logger.LogWarning($"Send({request.uri}, {request.method}, {response.StatusCode}) : {ex.Message}");
                return response;
            }
        }
    }
}

public enum AuthorizationType
{
    None,
    Bearer,
    Server
}