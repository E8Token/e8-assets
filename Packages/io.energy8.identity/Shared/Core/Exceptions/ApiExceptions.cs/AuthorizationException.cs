using Energy8.Identity.Shared.Core.Contracts.Dto.Errors;

namespace Energy8.Identity.Shared.Core.Exceptions
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
