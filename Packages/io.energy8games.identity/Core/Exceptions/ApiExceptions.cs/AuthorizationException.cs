using Energy8.Contracts.Dto.Errors;

namespace Energy8.Core.Exceptions
{
    public class AuthorizationException : ApiException
    {
        public AuthorizationException(ErrorDto error) : base(error)
        {
            CanProceed = false;
            CanRetry = false;
            MustSignOut = true;
        }
    }
}