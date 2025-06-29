using Energy8.Contracts.Dto.Common;

namespace Energy8.Contracts.Dto.Auth
{
    /// <summary>
    /// Represents a user's authentication information and account status.
    /// </summary>
    /// <remarks>
    /// Contains core user identity information used for authentication and authorization.
    /// </remarks>
    [System.Serializable]
    public class UserAuthRecord : DtoBase
    {
        /// <summary>
        /// Gets or sets the unique authentication identifier for the user.
        /// </summary>
        public string AuthId { get; set; }

        /// <summary>
        /// Gets or sets the user's email address.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the user's display name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets whether the user's email has been verified.
        /// </summary>
        public bool EmailVerified { get; set; }

        /// <summary>
        /// Gets or sets whether the user account is disabled.
        /// </summary>
        public bool Disabled { get; set; }

        /// <summary>
        /// Default constructor for Unity serialization
        /// </summary>
        public UserAuthRecord()
        {
        }

        /// <summary>
        /// Constructor with parameters
        /// </summary>
        /// <param name="authId">Unique authentication identifier</param>
        /// <param name="email">User's email address</param>
        /// <param name="name">User's display name</param>
        /// <param name="emailVerified">Whether email is verified</param>
        /// <param name="disabled">Whether account is disabled</param>
        public UserAuthRecord(string authId, string email, string name, bool emailVerified, bool disabled)
        {
            AuthId = authId;
            Email = email;
            Name = name;
            EmailVerified = emailVerified;
            Disabled = disabled;
        }
    }
}