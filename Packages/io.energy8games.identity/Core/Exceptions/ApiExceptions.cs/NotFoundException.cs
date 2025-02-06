using Energy8.Contracts.Dto.Errors;

namespace Energy8.Core.Exceptions
{
    public class NotFoundException : ApiException
    {
        public NotFoundException(ErrorDto error) : base(error)
        {
            CanProceed = true;
            CanRetry = false;
        }
    }
}
