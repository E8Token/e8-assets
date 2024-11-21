using System;
using System.Net;

namespace Energy8.Models.Errors
{
    public class RequestErrorDataException : ErrorDataException
    {
        public HttpStatusCode HttpStatusCode { get; private set; }
        public RequestErrorDataException(HttpStatusCode httpStatusCode, string header, string description = "", bool canProceed = false, bool canRetry = false, bool mustSignOut = false)
            : base(header, description, canProceed, canRetry, mustSignOut)
        {
            HttpStatusCode = httpStatusCode;
        }
    }
}