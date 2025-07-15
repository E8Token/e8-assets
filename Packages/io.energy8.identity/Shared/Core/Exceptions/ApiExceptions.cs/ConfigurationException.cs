using Energy8.Identity.Shared.Core.Contracts.Dto.Errors;

namespace Energy8.Identity.Shared.Core.Exceptions
{
    public class ConfigurationException : ApiException
    {
        public ConfigurationException(ErrorDto error) : base(error)
        {
            CanProceed = false;
            CanRetry = false;
        }
    }
}
