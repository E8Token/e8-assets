using Energy8.Identity.Configuration.Core;
using Energy8.EnvironmentConfig.Base;
using Energy8.Identity.Game.Core.Services;
using Energy8.Identity.Game.Runtime.Services;
using Energy8.Identity.Http.Core;
using Energy8.Identity.Http.Runtime.Clients;
using Energy8.Identity.Shared.Core.Contracts.Dto.Games;
using UnityEngine;

namespace Energy8.Identity.Game.Runtime.Factory
{
    /// <summary>
    /// Factory for creating game service instances with proper dependency injection and type safety
    /// </summary>
    public static class GameServiceFactory
    {
        /// <summary>
        /// Creates a typed game service with provided dependencies
        /// </summary>
        /// <typeparam name="TGameUserDto">Game user DTO type</typeparam>
        /// <typeparam name="TGameSessionDto">Game session DTO type</typeparam>
        /// <param name="httpClient">HTTP client for API calls</param>
        /// <param name="gameEndpoint">Game API endpoint (default: "game")</param>
        /// <returns>Typed game service instance</returns>
        public static GameService<TGameUserDto, TGameSessionDto> CreateService<TGameUserDto, TGameSessionDto>(
            IHttpClient httpClient, 
            string gameEndpoint = "game")
            where TGameUserDto : GameUserDto
            where TGameSessionDto : GameSessionDto
        {
            var config = ModuleConfigManager<IdentityConfig>.GetCurrentConfig("Identity");

            if (httpClient == null)
            {
                throw new System.ArgumentNullException(nameof(httpClient));
            }

            if (string.IsNullOrWhiteSpace(gameEndpoint))
            {
                gameEndpoint = "game";
            }

            return new GameService<TGameUserDto, TGameSessionDto>(httpClient, gameEndpoint);
        }
        
        /// <summary>
        /// Creates a non-generic game service with base DTOs
        /// </summary>
        /// <param name="httpClient">HTTP client for API calls</param>
        /// <param name="gameEndpoint">Game API endpoint (default: "game")</param>
        /// <returns>Game service instance</returns>
        public static IGameService CreateService(IHttpClient httpClient, string gameEndpoint = "game")
        {
            return CreateService<GameUserDto, GameSessionDto>(httpClient, gameEndpoint);
        }
        
        /// <summary>
        /// Creates typed game service with default configuration
        /// </summary>
        /// <typeparam name="TGameUserDto">Game user DTO type</typeparam>
        /// <typeparam name="TGameSessionDto">Game session DTO type</typeparam>
        /// <returns>Typed game service instance with default dependencies</returns>
        public static GameService<TGameUserDto, TGameSessionDto> CreateDefaultService<TGameUserDto, TGameSessionDto>()
            where TGameUserDto : GameUserDto
            where TGameSessionDto : GameSessionDto
        {
            var config = ModuleConfigManager<IdentityConfig>.GetCurrentConfig("Identity");

            var httpClient = new UnityHttpClient(config?.AuthServerUrl ?? "http://localhost");
            return CreateService<TGameUserDto, TGameSessionDto>(httpClient);
        }
        
        /// <summary>
        /// Creates game service with default configuration (non-generic)
        /// </summary>
        /// <returns>Game service instance with default dependencies</returns>
        public static IGameService CreateDefaultService()
        {
            return CreateDefaultService<GameUserDto, GameSessionDto>();
        }
        
        /// <summary>
        /// Creates a typed game service for testing with mock dependencies
        /// </summary>
        /// <typeparam name="TGameUserDto">Game user DTO type</typeparam>
        /// <typeparam name="TGameSessionDto">Game session DTO type</typeparam>
        /// <param name="gameEndpoint">Test game endpoint</param>
        /// <returns>Test typed game service instance</returns>
        public static GameService<TGameUserDto, TGameSessionDto> CreateTestService<TGameUserDto, TGameSessionDto>(
            string gameEndpoint = "test-game")
            where TGameUserDto : GameUserDto
            where TGameSessionDto : GameSessionDto
        {
            var mockHttpClient = new UnityHttpClient("http://localhost:3000");
            return CreateService<TGameUserDto, TGameSessionDto>(mockHttpClient, gameEndpoint);
        }
        
        /// <summary>
        /// Creates a game service for testing with mock dependencies (non-generic)
        /// </summary>
        /// <param name="gameEndpoint">Test game endpoint</param>
        /// <returns>Test game service instance</returns>
        public static IGameService CreateTestService(string gameEndpoint = "test-game")
        {
            return CreateTestService<GameUserDto, GameSessionDto>(gameEndpoint);
        }
    }
}
