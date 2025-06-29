using Energy8.Contracts.Dto.Common;

namespace Energy8.Contracts.Dto.Games
{
    /// <summary>
    /// Data transfer object for managing referral relationships.
    /// </summary>
    /// <remarks>
    /// Used to track and validate referral connections between users.
    /// </remarks>
    [System.Serializable]
    public class ReferralSetDto : DtoBase
    {
        /// <summary>
        /// Gets or sets the unique identifier for this referral record.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the unique device identifier of the referred user.
        /// </summary>
        public string DeviceId { get; set; }

        /// <summary>
        /// Gets or sets the referral code used.
        /// </summary>
        public string ReferralId { get; set; }

        /// <summary>
        /// Gets or sets the cryptographic signature validating this referral.
        /// </summary>
        public string Signature { get; set; }

        /// <summary>
        /// Default constructor for Unity serialization
        /// </summary>
        public ReferralSetDto()
        {
            Id = string.Empty;
            DeviceId = string.Empty;
            ReferralId = string.Empty;
            Signature = string.Empty;
        }

        /// <summary>
        /// Constructor with parameters
        /// </summary>
        public ReferralSetDto(string id, string deviceId, string referralId, string signature)
        {
            Id = id;
            DeviceId = deviceId;
            ReferralId = referralId;
            Signature = signature;
        }
    }
}
