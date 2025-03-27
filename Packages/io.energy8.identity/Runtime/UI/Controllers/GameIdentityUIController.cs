using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Energy8.Identity.Core.Game.Services;
using Energy8.Identity.Core.Http;
using Energy8.Identity.Core.Logging;
using Energy8.Contracts.Dto.Games;
using Energy8.Contracts.Dto.User;
using Energy8.Identity.Views.Implementations;
using Energy8.Identity.Views.Models;
using Energy8.Identity.Core.Error;

namespace Energy8.Identity.Runtime.UI.Controllers
{
    // Обратите внимание: Unity не поддерживает generic MonoBehaviour,
    // однако если класс используется не напрямую как компонент – это допустимо.
    public class GameIdentityUIController<TGameService, TGameUserDto, TGameSessionDto> : IdentityUIController
        where TGameService : GameService<TGameUserDto, TGameSessionDto>
        where TGameUserDto : GameUserDto
        where TGameSessionDto : GameSessionDto
    {
        public static new GameIdentityUIController<TGameService, TGameUserDto, TGameSessionDto>
            Instance
        => (GameIdentityUIController<TGameService, TGameUserDto, TGameSessionDto>)
            IdentityUIController.Instance;

        public event Action<TGameUserDto> OnGameUserGot;
        public event Action<TGameSessionDto> OnGameSessionCreated;

        protected string SessionId {get; set; }

        protected IGameService gameService;

        protected override void Awake()
        {
            base.Awake();
            OnGameSessionCreated += (resp) => SessionId = resp.SessionId;
            InitializeGameService();
        }

        protected virtual void InitializeGameService()
        {
            gameService = new GameService<TGameUserDto, TGameSessionDto>(httpClient,
                new Logger<GameService<TGameUserDto, TGameSessionDto>>());
        }

        /// <summary>
        /// Загружает информацию об игровом пользователе через GameService.
        /// Результат приводится к TGameUserDto.
        /// </summary>
        public async UniTask<TGameUserDto> GetGameUserAsync(CancellationToken ct)
        {
            try
            {
                GameUserDto gameUserDto = await gameService.GetUserAsync(ct);
                var result = (TGameUserDto)(object)gameUserDto;
                OnGameUserGot?.Invoke(result);
                return result;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to retrieve game user data: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Создает новую игровую сессию через GameService.
        /// Результат приводится к TGameSessionDto.
        /// </summary>
        public async UniTask<TGameSessionDto> CreateGameSessionAsync(CancellationToken ct)
        {
            try
            {
                GameSessionDto sessionDto = await gameService.CreateSessionsAsync(ct);
                var result = (TGameSessionDto)(object)sessionDto;
                OnGameSessionCreated?.Invoke(result);
                return result;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to create game session: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Основной авторизационный цикл с дополнительным получением игровых данных.
        /// Метод скрывает одноимённый метод базового класса.
        /// </summary>
        protected override async UniTask ShowUserFlow(CancellationToken ct)
        {
            SetOpenState(false);
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    // Получаем информацию о пользователе (identity)
                    UserDto identityUser = await userService.GetUserAsync(ct);
                    // Дополнительно получаем данные игрового персонажа
                    TGameUserDto gameUser = await GetGameUserAsync(ct);

                    var result = await viewManager
                        .Show<UserView, UserViewParams, UserViewResult>(
                            new UserViewParams(identityUser.Name), ct);

                    switch (result.Action)
                    {
                        case UserAction.OpenSettings:
                            await ShowSettings(ct);
                            break;
                        case UserAction.SignOut:
                            await identityService.SignOut(ct);
                            return;
                    }
                }
                catch (OperationCanceledException)
                {
                    continue;
                }
                catch (Exception ex) when (ex is SignOutRequiredException)
                {
                    await identityService.SignOut(ct);
                    continue;
                }
            }
        }
    }
}