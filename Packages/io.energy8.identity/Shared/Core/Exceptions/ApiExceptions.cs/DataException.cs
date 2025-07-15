using Energy8.Identity.Shared.Core.Contracts.Dto.Errors;

namespace Energy8.Identity.Shared.Core.Exceptions
{
    public class DataException : ApiException
    {
        public DataException(ErrorDto error) : base(error)
        {
            CanProceed = false;
            CanRetry = true;
        }
    }
}
