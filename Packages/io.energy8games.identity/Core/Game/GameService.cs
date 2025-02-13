using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Energy8.Contracts.Dto.Games;
using Energy8.Identity.Core.Logging;
using Energy8.Identity.Core.Http; // Интерфейс IHttpClient, используемый в проекте

namespace Energy8.Identity.Core.Game.Services
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
        private readonly IHttpClient httpClient;
        private readonly ILogger<GameService<TGameUserDto, TGameSessionDto>> logger;

        protected virtual string Game { get; set; } = "Game";

        public GameService(IHttpClient httpClient, ILogger<GameService<TGameUserDto, TGameSessionDto>> logger)
        {
            this.httpClient = httpClient;
            this.logger = logger;
        }

        public async UniTask<GameUserDto> GetUserAsync(CancellationToken ct)
        {
            try
            {
                logger.LogInfo("Получение информации о игровом пользователе");
                var userDto = await httpClient.GetAsync<GameUserDto>($"{Game}/user", ct);
                return userDto;
            }
            catch (Exception ex)
            {
                logger.LogError($"Не удалось получить пользователя: {ex.Message}");
                throw new GameServiceException("Не удалось получить информацию о пользователе", ex);
            }
        }

        public async UniTask<GameSessionDto> CreateSessionsAsync(CancellationToken ct)
        {
            try
            {
                logger.LogInfo("Создание игровой сессии");
                // Если понадобятся дополнительные данные для создания сессии, можно создать и передать GameSessionCreateDto
                var createDto = new GameSessionCreateDto
                {
                    // Если требуется, заполните необходимые поля, например:
                    ServerId = string.Empty
                };

                // Вызов POST "session" для создания сессии
                return await httpClient.PostAsync<GameSessionDto>("session", createDto, ct);
            }
            catch (Exception ex)
            {
                logger.LogError($"Не удалось создать сессию: {ex.Message}");
                throw new GameServiceException("Не удалось создать игровую сессию", ex);
            }
        }
    }
}