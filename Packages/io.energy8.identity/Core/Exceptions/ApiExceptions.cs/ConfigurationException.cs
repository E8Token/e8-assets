using Energy8.Contracts.Dto.Errors;

namespace Energy8.Core.Exceptions
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