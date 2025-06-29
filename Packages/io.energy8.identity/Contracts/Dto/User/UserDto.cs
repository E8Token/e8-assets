using System.Collections.Generic;
using Energy8.Contracts.Dto.Common;

namespace Energy8.Contracts.Dto.User
{
    /// <summary>
    /// Data transfer object representing a user in the system.
    /// </summary>
    [System.Serializable]
    public class UserDto : DtoBase
    {
        /// <summary>
        /// Gets or sets the list of authentication providers associated with the user.
        /// </summary>
        public List<string> AuthProviders { get; set; }

        /// <summary>
        /// Gets or sets the user's email address.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the display name of the user.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Default constructor for Unity serialization
        /// </summary>
        public UserDto()
        {
            AuthProviders = new List<string>();
        }

        /// <summary>
        /// Constructor with parameters
        /// </summary>
        public UserDto(string name, List<string> authProviders, string email = null)
        {
            Name = name;
            AuthProviders = authProviders ?? new List<string>();
            Email = email;
        }
    }
}
