using Energy8.Contracts.Dto.Common;

namespace Energy8.Contracts.Dto.Games
{
    /// <summary>
    /// Data transfer object for registering a new game server.
    /// </summary>
    [System.Serializable]
    public class GameServerRegistrationDto : DtoBase
    {
        /// <summary>
        /// Gets or sets the IP address of the game server.
        /// </summary>
        public string IP { get; set; }
        
        /// <summary>
        /// Gets or sets the port number the server is listening on.
        /// </summary>
        public ushort Port { get; set; }
        
        /// <summary>
        /// Gets or sets the maximum number of concurrent players allowed.
        /// </summary>
        public byte MaxPlayers { get; set; }

        /// <summary>
        /// Default constructor for Unity serialization
        /// </summary>
        public GameServerRegistrationDto()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GameServerRegistrationDto"/> class.
        /// </summary>
        /// <param name="ip">The server's IP address.</param>
        /// <param name="port">The server's port number.</param>
        /// <param name="maxPlayers">Maximum number of concurrent players.</param>
        public GameServerRegistrationDto(string ip, ushort port, byte maxPlayers)
        {
            IP = ip;
            Port = port;
            MaxPlayers = maxPlayers;
        }
    }

    /// <summary>
    /// Contains authentication credentials for a registered game server.
    /// </summary>
    [System.Serializable]
    public class GameServerCredentialsDto : DtoBase
    {
        /// <summary>
        /// Gets or sets the unique server identifier.
        /// </summary>
        public string ServerId { get; set; }

        /// <summary>
        /// Gets or sets the server's authentication key.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Default constructor for Unity serialization
        /// </summary>
        public GameServerCredentialsDto()
        {
        }

        /// <summary>
        /// Constructor with parameters
        /// </summary>
        public GameServerCredentialsDto(string serverId, string key)
        {
            ServerId = serverId;
            Key = key;
        }
    }
}
