using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Energy8.Identity.Shared.Core.Contracts.Dto.Games;

using Energy8.Identity.Http.Core;
using UnityEngine; // Интерфейс IHttpClient, используемый в проекте

namespace Energy8.Identity.Game.Core.Services
{
    // При необходимости можно определить собственное исключение для GameService
    public class GameServiceException : Exception
    {
        public GameServiceException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }

    public class GameService<TGameUserDto, TGameSessionDto> : IGameService
        where TGameUserDto : GameUserDto
        where TGameSessionDto : GameSessionDto
    {
        protected readonly IHttpClient httpClient;

        protected virtual string Game { get; set; } = "Game";

        public GameService(IHttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public virtual async UniTask<GameUserDto> GetUserAsync(CancellationToken ct)
        {
            try
            {
                Debug.Log("Fetching game user");
                var userDto = await httpClient.GetAsync<GameUserDto>($"{Game}/user", ct);
                return userDto;
            }
            catch (Exception ex)
            {
                Debug.LogError("Game user fetch failed: " + ex.Message);
                throw new GameServiceException("Failed to retrieve game user", ex);
            }
        }

        public virtual async UniTask<GameSessionDto> CreateSessionsAsync(CancellationToken ct)
        {
            try
            {
                Debug.Log("Creating game session");
                
                var createDto = new GameSessionCreateDto
                {
                    ServerId = string.Empty,
                    Data = string.Empty
                };

                return await httpClient.PostAsync<GameSessionDto>($"{Game}/session", createDto, ct);
            }
            catch (Exception ex)
            {
                Debug.LogError("Game session creation failed: " + ex.Message);
                throw new GameServiceException("Failed to create game session", ex);
            }
        }
    }
}