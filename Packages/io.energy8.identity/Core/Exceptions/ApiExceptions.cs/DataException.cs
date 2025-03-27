using Energy8.Contracts.Dto.Errors;

namespace Energy8.Core.Exceptions
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
