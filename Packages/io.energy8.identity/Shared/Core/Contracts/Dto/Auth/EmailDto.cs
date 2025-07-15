using Energy8.Identity.Shared.Core.Contracts.Dto.Common;

namespace Energy8.Identity.Shared.Core.Contracts.Dto.Auth
{
    /// <summary>
    /// Data transfer object for initiating email-based authentication.
    /// </summary>
    /// <remarks>
    /// Used in the first step of email authentication flow to request verification code.
    /// Supports both new user registration and existing user authentication.
    /// Email validation and rate limiting are applied at the service level.
    /// </remarks>
    [System.Serializable]
    public class EmailSignInDto : DtoBase
    {
        /// <summary>
        /// User's email address for authentication
        /// </summary>
        /// <remarks>
        /// - Must be a valid email format
        /// - Used for sending verification code
        /// - Case-insensitive during validation
        /// </remarks>
        public string Email { get; set; }

        /// <summary>
        /// Default constructor for Unity serialization
        /// </summary>
        public EmailSignInDto()
        {
        }

        /// <summary>
        /// Constructor with email parameter
        /// </summary>
        /// <param name="email">User's email address</param>
        public EmailSignInDto(string email)
        {
            Email = email;
        }
    }

    /// <summary>
    /// Data transfer object containing email verification session token.
    /// </summary>
    /// <remarks>
    /// Returned after successful initiation of email verification process.
    /// The token is time-limited and single-use for security purposes.
    /// Used to correlate the verification code with the original email request.
    /// </remarks>
    [System.Serializable]
    public class EmailVerificationTokenDto : DtoBase
    {
        /// <summary>
        /// Verification session token
        /// </summary>
        /// <remarks>
        /// - Used to link verification code with specific email request
        /// - Time-limited token
        /// - Required for confirmation step
        /// </remarks>
        public string Token { get; set; }

        /// <summary>
        /// Default constructor for Unity serialization
        /// </summary>
        public EmailVerificationTokenDto()
        {
        }

        /// <summary>
        /// Constructor with token parameter
        /// </summary>
        /// <param name="token">Verification session token</param>
        public EmailVerificationTokenDto(string token)
        {
            Token = token;
        }
    }

    /// <summary>
    /// DTO for linking email account to existing user.
    /// </summary>
    /// <remarks>
    /// Used when adding email authentication to an existing account.
    /// Requires valid authentication ID from the current session.
    /// Triggers email verification process for the new email address.
    /// </remarks>
    [System.Serializable]
    public class EmailLinkDto : EmailSignInDto
    {
        /// <summary>
        /// Authentication ID for linking with existing account
        /// </summary>
        public string AuthId { get; set; }

        /// <summary>
        /// Default constructor for Unity serialization
        /// </summary>
        public EmailLinkDto()
        {
        }

        /// <summary>
        /// Constructor with email and auth ID parameters
        /// </summary>
        /// <param name="email">User's email address</param>
        /// <param name="authId">Authentication ID for linking</param>
        public EmailLinkDto(string email, string authId) : base(email)
        {
            AuthId = authId;
        }
    }

    /// <summary>
    /// Data transfer object for email verification confirmation.
    /// </summary>
    /// <remarks>
    /// Used in the final step of email verification process.
    /// Combines verification token with user-provided verification code.
    /// Failed verification attempts may trigger security measures.
    /// </remarks>
    [System.Serializable]
    public class EmailConfirmDto : DtoBase
    {
        /// <summary>
        /// Verification token
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// Verification code
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Default constructor for Unity serialization
        /// </summary>
        public EmailConfirmDto()
        {
        }

        /// <summary>
        /// Constructor with token and code parameters
        /// </summary>
        /// <param name="token">Verification token</param>
        /// <param name="code">Verification code</param>
        public EmailConfirmDto(string token, string code)
        {
            Token = token;
            Code = code;
        }
    }
}
