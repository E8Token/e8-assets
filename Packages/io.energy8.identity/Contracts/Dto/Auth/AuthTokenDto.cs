using System.Collections.Generic;
using Energy8.Contracts.Dto.Common;

namespace Energy8.Contracts.Dto.Auth
{
    /// <summary>
    /// Represents an authentication token with associated claims.
    /// </summary>
    /// <remarks>
    /// Used for maintaining user authentication state and storing session-related claims.
    /// The token is typically a JWT that contains encoded claim information.
    /// </remarks>
    [System.Serializable]
    public class AuthTokenDto : DtoBase
    {
        /// <summary>
        /// Gets or sets the authentication token string.
        /// </summary>
        /// <remarks>
        /// Usually a JWT token that follows RFC 7519 standard.
        /// Contains three parts: header, payload, and signature.
        /// </remarks>
        public string Token { get; set; }

        /// <summary>
        /// Gets or sets the dictionary of claims associated with the token.
        /// </summary>
        /// <remarks>
        /// Contains key-value pairs representing user identity and authorization information.
        /// Common claims include user ID, roles, permissions, and session metadata.
        /// </remarks>
        public Dictionary<string, object> Claims { get; set; }

        /// <summary>
        /// Default constructor for Unity serialization
        /// </summary>
        public AuthTokenDto()
        {
        }

        /// <summary>
        /// Constructor with parameters
        /// </summary>
        /// <param name="token">Authentication token string</param>
        /// <param name="claims">Dictionary of claims</param>
        public AuthTokenDto(string token, Dictionary<string, object> claims)
        {
            Token = token;
            Claims = claims;
        }
    }
}