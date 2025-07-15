using Energy8.Identity.Shared.Core.Contracts.Dto.Common;

namespace Energy8.Identity.Shared.Core.Contracts.Dto.Games
{
    /// <summary>
    /// Data transfer object for creating a new game session.
    /// </summary>
    [System.Serializable]
    public class GameSessionCreateDto : DtoBase
    {
        /// <summary>
        /// Gets or sets the unique identifier of the game server.
        /// </summary>
        public string ServerId { get; set; }
        
        /// <summary>
        /// Gets or sets the session initialization data.
        /// </summary>
        public string Data { get; set; }

        /// <summary>
        /// Default constructor for Unity serialization
        /// </summary>
        public GameSessionCreateDto()
        {
            ServerId = string.Empty;
            Data = string.Empty;
        }

        /// <summary>
        /// Constructor with parameters
        /// </summary>
        public GameSessionCreateDto(string serverId, string data)
        {
            ServerId = serverId;
            Data = data;
        }
    }

    /// <summary>
    /// Represents a game session and its core information.
    /// </summary>
    [System.Serializable]
    public class GameSessionDto : DtoBase
    {
        /// <summary>
        /// Gets or sets the unique identifier for this game session.
        /// </summary>
        public string SessionId { get; set; }

        /// <summary>
        /// Default constructor for Unity serialization
        /// </summary>
        public GameSessionDto()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GameSessionDto"/> class.
        /// </summary>
        /// <param name="sessionId">The unique session identifier</param>
        public GameSessionDto(string sessionId)
        {
            SessionId = sessionId;
        }
    }
}
