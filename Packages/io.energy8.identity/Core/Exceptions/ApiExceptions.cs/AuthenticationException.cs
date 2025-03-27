using Energy8.Contracts.Dto.Errors;

namespace Energy8.Core.Exceptions
{
    public class AuthenticationException : ApiException
    {
        public AuthenticationException(ErrorDto error) : base(error)
        {
            CanProceed = false;
            CanRetry = true;
            MustSignOut = true;
        }
    }
}
