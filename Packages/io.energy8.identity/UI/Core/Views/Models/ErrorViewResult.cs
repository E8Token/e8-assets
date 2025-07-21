using Energy8.Identity.Shared.Core.Error;

namespace Energy8.Identity.UI.Core.Views.Models
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

