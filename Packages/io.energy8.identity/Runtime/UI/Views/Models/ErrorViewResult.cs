using Energy8.Identity.Core.Error;

namespace Energy8.Identity.Views.Models
{
    public class ErrorViewResult : ViewResult
    {
        public ErrorHandlingMethod Method { get; }

        public ErrorViewResult(ErrorHandlingMethod method)
        {
            Method = method;
        }
    }
}
