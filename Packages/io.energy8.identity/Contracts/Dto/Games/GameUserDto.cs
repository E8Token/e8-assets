using Energy8.Contracts.Dto.Common;

namespace Energy8.Contracts.Dto.Games
{
    /// <summary>
    /// Represents core game user information.
    /// </summary>
    [System.Serializable]
    public class GameUserDto : DtoBase
    {
        /// <summary>
        /// Gets or sets the user's current balance.
        /// </summary>
        public long Balance { get; set; }

        /// <summary>
        /// Default constructor for Unity serialization
        /// </summary>
        public GameUserDto()
        {
        }

        /// <summary>
        /// Constructor with balance parameter
        /// </summary>
        /// <param name="balance">User's current balance</param>
        public GameUserDto(long balance)
        {
            Balance = balance;
        }
    }
}