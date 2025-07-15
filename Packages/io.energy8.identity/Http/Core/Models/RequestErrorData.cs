using System.Net;
using Energy8.Identity.Shared.Core.Exceptions;

namespace Energy8.Identity.Http.Core.Models
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