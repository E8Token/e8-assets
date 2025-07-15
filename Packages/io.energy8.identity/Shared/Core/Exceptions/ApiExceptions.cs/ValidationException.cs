using Energy8.Identity.Shared.Core.Contracts.Dto.Errors;

namespace Energy8.Identity.Shared.Core.Exceptions
{
    public class ValidationException : ApiException
    {
        public ValidationException(ErrorDto error) : base(error)
        {
            CanProceed = true;
            CanRetry = false;
        }
    }
}
