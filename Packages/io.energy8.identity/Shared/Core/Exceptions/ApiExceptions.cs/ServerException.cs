using Energy8.Identity.Shared.Core.Contracts.Dto.Errors;

namespace Energy8.Identity.Shared.Core.Exceptions
{
    public class ServerException : ApiException
    {
        public ServerException(ErrorDto error) : base(error)
        {
            CanProceed = false;
            CanRetry = true;
        }
    }
}
