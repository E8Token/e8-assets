using System.Net;
using Energy8.Core.Exceptions;

namespace Energy8.Core.Http.Models
{
    public class RequestErrorModelException : Energy8Exception
    {
        public HttpStatusCode HttpStatusCode { get; private set; }
        public RequestErrorModelException(HttpStatusCode httpStatusCode, string header, string description = "", bool canProceed = false, bool canRetry = false, bool mustSignOut = false)
            : base(header, description, canProceed, canRetry, mustSignOut)
        {
            HttpStatusCode = httpStatusCode;
        }
    }
}