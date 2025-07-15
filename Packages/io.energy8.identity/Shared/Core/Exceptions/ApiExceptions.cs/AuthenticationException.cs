using Energy8.Identity.Shared.Core.Contracts.Dto.Errors;

namespace Energy8.Identity.Shared.Core.Exceptions
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
