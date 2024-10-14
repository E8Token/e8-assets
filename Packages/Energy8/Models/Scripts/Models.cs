using Energy8.Models.Errors;
using System;
using System.Net;
using UnityEngine;
using UnityEngine.Networking;

namespace Energy8.Models.Requests
{
    public class WebTryResult<TResult> : TryResult<TResult> where TResult : Data
    {
        public HttpStatusCode StatusCode { get; set; }

        public WebTryResult(TryResultStatus status, TResult value = default, ErrorData error = null) : base(status, value, error)
        {
            StatusCode = status == TryResultStatus.Successful ? HttpStatusCode.OK : HttpStatusCode.BadRequest;
        }
        public WebTryResult(TryResultStatus status, HttpStatusCode code, TResult value = default, ErrorData error = null) : base(status, value, error)
        {
            StatusCode = code;
            if (error == null)
                Error = WebTryResult.ValiadateErrorResponse(code);
        }

        public static WebTryResult<TResult> Create(UnityWebRequest request)
        {
            if (!request.isDone)
                throw new Exception("Request is not done.");

            HttpStatusCode code = ValidateResponseCode(request.responseCode);
            TryResultStatus status = code == HttpStatusCode.OK ? TryResultStatus.Successful : TryResultStatus.Failed;
            TResult value = null;
            ErrorData error = null;
            if (request.downloadHandler.text is not null)
            {
                if (status == TryResultStatus.Successful)
                    if (Data.TryFromJson(request.downloadHandler.text, out TResult data))
                        value = data;
                    else
                    {
                        //code = HttpStatusCode.UnprocessableEntity;
                        status = TryResultStatus.Failed;
                        error = new("Invalid JSON",
                            "The request was successfully completed, but the client is unable to process the JSON response from the server.");
                    }
                else
                    error = WebTryResult.ValiadateErrorResponse(request);
            }
            return new WebTryResult<TResult>(status, code, value, error);
        }

        static HttpStatusCode ValidateResponseCode(long code)
        {
            try
            {
                return (HttpStatusCode)code;
            }
            catch (Exception ex)
            {
                Debug.LogWarning(ex.ToString());
                return HttpStatusCode.BadRequest;
            }
        }

        public static WebTryResult<T> Create<T>(TryResult<T> source) where T : Data =>
            new(source.Status, source.Value, source.Error);
        public static WebTryResult<OutT> Create<InT, OutT>(TryResult<InT> source)
            where OutT : Data where InT : Data
        {
            if (source.GetType() == typeof(WebTryResult<InT>))
                return new(source.Status, ((WebTryResult<InT>)source).StatusCode, source.Value as OutT, source.Error);
            else
                return new(source.Status, source.Value as OutT, source.Error);
        }

        public TryResult<TResult> AsTryResult() => new(Status, Value, Error);
    }
    public class WebTryResult : TryResult
    {
        public HttpStatusCode StatusCode { get; set; }

        public WebTryResult(TryResultStatus status, ErrorData error = null) : base(status, error)
        {
            StatusCode = status == TryResultStatus.Successful ? HttpStatusCode.OK : HttpStatusCode.BadRequest;
        }
        public WebTryResult(TryResultStatus status, HttpStatusCode code, ErrorData error = null) : base(status, error)
        {
            StatusCode = code;
            if (error == null)
                Error = ValiadateErrorResponse(code);
        }

        public static WebTryResult Create(UnityWebRequest request)
        {
            if (!request.isDone)
                throw new Exception("Request is not done.");

            HttpStatusCode code = ValidateResponseCode(request.responseCode);
            TryResultStatus status = code == HttpStatusCode.OK ? TryResultStatus.Successful : TryResultStatus.Failed;
            ErrorData error = null;
            if (request.downloadHandler.text is not null)
            {
                if (status != TryResultStatus.Successful)
                    error = ValiadateErrorResponse(request);
            }
            return new WebTryResult(status, code, error);
        }

        static HttpStatusCode ValidateResponseCode(long code)
        {
            try
            {
                return (HttpStatusCode)code;
            }
            catch (Exception ex)
            {
                Debug.LogWarning(ex.ToString());
                return HttpStatusCode.BadRequest;
            }
        }

        public static WebTryResult<T> Create<T>(TryResult<T> source) where T : Data =>
            new(source.Status, source.Value, source.Error);
        public static WebTryResult<OutT> Create<InT, OutT>(TryResult<InT> source)
            where OutT : Data where InT : Data =>
                new(source.Status, source.Value as OutT, source.Error);

        public TryResult AsTryResult() => new(Status, Error);

        static public ErrorData ValiadateErrorResponse(UnityWebRequest request)
        {
            ErrorData validateErrorMessage = new("Unknown Error", request.downloadHandler.text);
            switch (request.responseCode)
            {
                case 0:
                    validateErrorMessage = new("Connection Error", "The server is not responding to the request. Most likely the problem is in your network connection.\n" +
                        "Please check that you have internet access and try again later.", canProceed: true);
                    return validateErrorMessage;
                case 502:
                    validateErrorMessage = new("Server Error", "<b>Error on the <u>server</u> side!</b>\n" +
                        "It is possible that technical work is underway on the server.\n\n" +
                        "<b><u>Try again or wait, please.</u></b>", canProceed: true);
                    return validateErrorMessage;
                case 500:
                    validateErrorMessage = new("Server Error", "<b>Error on the <u>server</u> side!</b>\n" +
                        "It is possible that technical work is underway on the server.\n\n" +
                        "<b><u>Try again or wait, please.</u></b>", canProceed: true);
                    return validateErrorMessage;
                case 400:
                    validateErrorMessage = Data.TryFromJson(request.downloadHandler.text, out validateErrorMessage) ? validateErrorMessage : new("System error",
                        "Error processing the request.\n" + "Please try again, try restart the app and contact support to report the problem.", canProceed: true);
                    return validateErrorMessage;
                case 401:
                    validateErrorMessage = Data.TryFromJson(request.downloadHandler.text, out validateErrorMessage) ? validateErrorMessage : new("Authorization Error",
                        "<b>Your device is not authorized.</b>\n" +
                        "The key may have been deleted or the authorization session of this device is outdated.\n" +
                        "<b><u>Try again</u> and, in case of an error, <u>Re-Login</u>.</b>", canProceed: true, mustSignOut: true);
                    return validateErrorMessage;
            }
            return validateErrorMessage;
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