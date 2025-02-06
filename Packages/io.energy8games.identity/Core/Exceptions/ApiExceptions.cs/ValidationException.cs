using Energy8.Contracts.Dto.Errors;

namespace Energy8.Core.Exceptions
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