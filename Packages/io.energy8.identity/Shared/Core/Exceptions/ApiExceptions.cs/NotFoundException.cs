using Energy8.Identity.Shared.Core.Contracts.Dto.Errors;

namespace Energy8.Identity.Shared.Core.Exceptions
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
