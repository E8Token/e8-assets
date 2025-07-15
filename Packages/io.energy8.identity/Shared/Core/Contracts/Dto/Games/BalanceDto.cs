using Energy8.Identity.Shared.Core.Contracts.Dto.Common;

namespace Energy8.Identity.Shared.Core.Contracts.Dto.Games
{
    /// <summary>
    /// Represents a balance update request for a game session.
    /// </summary>
    /// <remarks>
    /// Used to modify player's balance during gameplay.
    /// Supports both positive (credits) and negative (debits) amounts.
    /// The session must be active for the balance update to succeed.
    /// </remarks>
    [System.Serializable]
    public class GameBalanceUpdateDto : DtoBase
    {
        /// <summary>
        /// Gets the session identifier for the balance update.
        /// </summary>
        /// <remarks>
        /// Must reference an active game session.
        /// Balance updates for inactive or expired sessions will be rejected.
        /// </remarks>
        public string SessionId { get; set; }

        /// <summary>
        /// Gets or sets the amount to change. Positive for credits, negative for debits.
        /// </summary>
        public long Amount { get; set; }

        /// <summary>
        /// Default constructor for Unity serialization
        /// </summary>
        public GameBalanceUpdateDto()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GameBalanceUpdateDto"/> class.
        /// </summary>
        /// <param name="sessionId">The active session identifier.</param>
        /// <param name="amount">The amount to change. Positive for credits, negative for debits.</param>
        public GameBalanceUpdateDto(string sessionId, long amount)
        {
            SessionId = sessionId;
            Amount = amount;
        }
    }
}
