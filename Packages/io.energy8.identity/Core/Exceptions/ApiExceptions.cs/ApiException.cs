using Energy8.Contracts.Dto.Errors;

namespace Energy8.Core.Exceptions
{
    public class ApiException : Energy8Exception
    {
        protected ApiException(ErrorDto error) : base(error.Description)
        {
            Header = error.Header;
            CanProceed = true;
            CanRetry = true;
        }
    }
}
