using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Energy8.Identity.Game.Core.Services;
using Energy8.Identity.Http.Core;
using Energy8.Identity.Shared.Core.Contracts.Dto.Games;
using Energy8.Identity.Shared.Core.Exceptions;
using UnityEngine;

namespace Energy8.Identity.Game.Runtime.Services
{
    /// <summary>
    /// Generic game service for type-safe game operations
    /// </summary>
    /// <typeparam name="TGameUserDto">Game user DTO type</typeparam>
    /// <typeparam name="TGameSessionDto">Game session DTO type</typeparam>
    public class GameService<TGameUserDto, TGameSessionDto> : IGameService
        where TGameUserDto : GameUserDto
        where TGameSessionDto : GameSessionDto
    {
        protected readonly IHttpClient httpClient;
        protected readonly string gameEndpoint;

        /// <summary>
        /// Game identifier for API endpoints
        /// </summary>
        protected virtual string Game => gameEndpoint;

        public GameService(IHttpClient httpClient, string gameEndpoint = "game")
        {
            this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            this.gameEndpoint = gameEndpoint;
        }

        public virtual async UniTask<GameUserDto> GetUserAsync(CancellationToken ct)
        {
            try
            {
                var endpoint = $"{Game}/user";
                var userDto = await httpClient.GetAsync<TGameUserDto>(endpoint, ct);
                return userDto;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to get game user: {ex.Message}");
                throw new Energy8Exception(
                    "Game User Fetch Failed", 
                    "Unable to retrieve game user data", 
                    canRetry: true);
            }
        }

        public virtual async UniTask<GameSessionDto> CreateSessionsAsync(CancellationToken ct)
        {
            try
            {
                var endpoint = $"{Game}/session";
                var sessionDto = await httpClient.PostAsync<TGameSessionDto>(endpoint, new { }, ct);
                return sessionDto;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to create game session: {ex.Message}");
                throw new Energy8Exception(
                    "Game Session Creation Failed", 
                    "Unable to create game session", 
                    canRetry: true);
            }
        }

        /// <summary>
        /// Gets typed game user data
        /// </summary>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Typed game user data</returns>
        public virtual async UniTask<TGameUserDto> GetTypedUserAsync(CancellationToken ct)
        {
            var result = await GetUserAsync(ct);
            return (TGameUserDto)result;
        }

        /// <summary>
        /// Creates typed game session
        /// </summary>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Typed game session data</returns>
        public virtual async UniTask<TGameSessionDto> CreateTypedSessionAsync(CancellationToken ct)
        {
            var result = await CreateSessionsAsync(ct);
            return (TGameSessionDto)result;
        }
    }
}
