using Energy8.Contracts.Dto.Common;

namespace Energy8.Contracts.Dto.Auth
{
    /// <summary>
    /// Data transfer object for authentication access token.
    /// </summary>
    /// <remarks>
    /// Represents a JWT access token for authenticated sessions.
    /// Used for maintaining user authentication state across requests.
    /// Token contains encoded claims about user identity and permissions.
    /// </remarks>
    [System.Serializable]
    public class AccessTokenDto : DtoBase
    {
        /// <summary>
        /// JWT access token string.
        /// </summary>
        /// <remarks>
        /// Contains three parts: header, payload, and signature.
        /// Encoded using Base64Url encoding.
        /// Validated on each authenticated request.
        /// </remarks>
        public string Token { get; set; }

        /// <summary>
        /// Default constructor for Unity serialization
        /// </summary>
        public AccessTokenDto()
        {
        }

        /// <summary>
        /// Constructor with token parameter
        /// </summary>
        /// <param name="token">JWT access token string</param>
        public AccessTokenDto(string token)
        {
            Token = token;
        }
    }
}