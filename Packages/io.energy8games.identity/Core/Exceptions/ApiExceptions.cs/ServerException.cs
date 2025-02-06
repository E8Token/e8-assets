using Energy8.Contracts.Dto.Errors;

namespace Energy8.Core.Exceptions
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
