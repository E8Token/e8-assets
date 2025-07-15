using Energy8.Identity.Shared.Core.Contracts.Dto.Common;

namespace Energy8.Identity.Shared.Core.Contracts.Dto.User
{
    /// <summary>
    /// Data transfer object for changing user's email address.
    /// </summary>
    [System.Serializable]
    public class EmailChangeDto : DtoBase
    {
        /// <summary>
        /// Gets or sets the new email address.
        /// </summary>
        public string NewEmail { get; set; }

        /// <summary>
        /// Default constructor for Unity serialization
        /// </summary>
        public EmailChangeDto()
        {
        }

        /// <summary>
        /// Constructor with new email parameter
        /// </summary>
        /// <param name="newEmail">The new email address</param>
        public EmailChangeDto(string newEmail)
        {
            NewEmail = newEmail;
        }
    }
}