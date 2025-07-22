using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Energy8.Identity.Http.Core;
using Energy8.Identity.Shared.Core.Contracts.Dto.Games;
using Energy8.Identity.Shared.Core.Exceptions;
using Game.Dto;
using Game.Services;
using UnityEngine;

namespace Game.Services
{
    /// <summary>
    /// Кастомная реализация игрового сервиса для NeonFruits слота
    /// </summary>
    public class NeonFruitsGameService : INeonFruitsGameService
    {
        private readonly IHttpClient httpClient;
        private readonly string gameEndpoint;

        /// <summary>
        /// Базовый путь для API игры
        /// </summary>
        protected virtual string Game => gameEndpoint;

        public NeonFruitsGameService(IHttpClient httpClient, string gameEndpoint = "neon-fruits")
        {
            this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            this.gameEndpoint = gameEndpoint;
            
            Debug.Log($"[NeonFruitsGameService] Initialized with endpoint: {gameEndpoint}");
        }

        #region IGameService Implementation (базовые методы)

        /// <summary>
        /// Получает данные игрового пользователя
        /// </summary>
        public virtual async UniTask<GameUserDto> GetUserAsync(CancellationToken ct)
        {
            try
            {
                Debug.Log($"[NeonFruitsGameService] Getting user data from: {Game}/user");
                var endpoint = $"{Game}/user";
                var userDto = await httpClient.GetAsync<GameUserDto>(endpoint, ct);
                
                Debug.Log($"[NeonFruitsGameService] User data retrieved. Balance: {userDto.Balance}");
                return userDto;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[NeonFruitsGameService] Failed to get game user: {ex.Message}");
                throw new Energy8Exception(
                    "Game User Fetch Failed", 
                    "Unable to retrieve game user data", 
                    canRetry: true);
            }
        }

        /// <summary>
        /// Создает игровую сессию с кастомными данными для NeonFruits
        /// </summary>
        public virtual async UniTask<GameSessionDto> CreateSessionsAsync(CancellationToken ct)
        {
            try
            {
                Debug.Log($"[NeonFruitsGameService] Creating game session for NeonFruits");
                
                var createDto = new GameSessionCreateDto
                {
                    ServerId = string.Empty,
                    Data = "NeonFruits"
                };

                var endpoint = $"{Game}/session";
                var sessionDto = await httpClient.PostAsync<GameSessionDto>(endpoint, createDto, ct);
                
                Debug.Log($"[NeonFruitsGameService] Session created: {sessionDto.SessionId}");
                return sessionDto;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[NeonFruitsGameService] Failed to create game session: {ex.Message}");
                throw new Energy8Exception("Failed to create game session", ex.Message);
            }
        }

        #endregion

        #region INeonFruitsGameService Implementation (кастомные методы)

        /// <summary>
        /// Инициализирует игру NeonFruits
        /// </summary>
        public async UniTask<NFGameStatusResponseDto> InitializeGameAsync(string sessionId, CancellationToken ct)
        {
            try
            {
                Debug.Log($"[NeonFruitsGameService] Initializing game for session: {sessionId}");
                
                var request = new GameInitializeRequestDto(sessionId);
                var endpoint = $"{Game}/initialize";
                
                var response = await httpClient.PutAsync<NFGameStatusResponseDto>(endpoint, request, ct);
                
                Debug.Log($"[NeonFruitsGameService] Game initialized. Status: {response.Status}, Balance: {response.Balance}");
                return response;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[NeonFruitsGameService] Failed to initialize game: {ex.Message}");
                throw new Energy8Exception("Не удалось инициализировать игру", ex.Message);
            }
        }

        /// <summary>
        /// Выполняет спин в слоте
        /// </summary>
        public async UniTask<TSpinResponse> SpinAsync<TSpinRequest, TSpinResponse>(TSpinRequest request, CancellationToken ct)
            where TSpinRequest : SpinRequestDto
            where TSpinResponse : GameStatusResponseDto
        {
            try
            {
                Debug.Log($"[NeonFruitsGameService] Executing spin. Bet: {request.BetAmount}, Lines: {request.Lines}");
                
                var endpoint = $"{Game}/spin";
                var response = await httpClient.PutAsync<TSpinResponse>(endpoint, request, ct);
                
                Debug.Log($"[NeonFruitsGameService] Spin completed. Win: {response.WinAmount}, Balance: {response.Balance}");
                return response;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[NeonFruitsGameService] Failed to execute spin: {ex.Message}");
                throw new Energy8Exception("Не удалось выполнить вращение", ex.Message);
            }
        }

        /// <summary>
        /// Активирует бонусную игру
        /// </summary>
        public async UniTask<NFGameStatusResponseDto> ActivateBonusAsync(string sessionId, CancellationToken ct)
        {
            try
            {
                Debug.Log($"[NeonFruitsGameService] Activating bonus for session: {sessionId}");
                
                var endpoint = $"{Game}/bonus/activate";
                var requestData = new { sessionId };
                
                var response = await httpClient.PostAsync<NFGameStatusResponseDto>(endpoint, requestData, ct);
                
                Debug.Log($"[NeonFruitsGameService] Bonus activated. Status: {response.Status}");
                return response;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[NeonFruitsGameService] Failed to activate bonus: {ex.Message}");
                throw new Energy8Exception("Не удалось активировать бонус", ex.Message);
            }
        }

        /// <summary>
        /// Получает текущий статус игры
        /// </summary>
        public async UniTask<NFGameStatusResponseDto> GetGameStatusAsync(string sessionId, CancellationToken ct)
        {
            try
            {
                Debug.Log($"[NeonFruitsGameService] Getting game status for session: {sessionId}");
                
                var endpoint = $"{Game}/status/{sessionId}";
                var response = await httpClient.GetAsync<NFGameStatusResponseDto>(endpoint, ct);
                
                Debug.Log($"[NeonFruitsGameService] Game status retrieved. Status: {response.Status}, Balance: {response.Balance}");
                return response;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[NeonFruitsGameService] Failed to get game status: {ex.Message}");
                throw new Energy8Exception("Не удалось получить статус игры", ex.Message);
            }
        }

        /// <summary>
        /// Завершает игровую сессию
        /// </summary>
        public async UniTask<NFGameStatusResponseDto> EndGameSessionAsync(string sessionId, CancellationToken ct)
        {
            try
            {
                Debug.Log($"[NeonFruitsGameService] Ending game session: {sessionId}");
                
                var endpoint = $"{Game}/session/{sessionId}/end";
                var response = await httpClient.PostAsync<NFGameStatusResponseDto>(endpoint, new { }, ct);
                
                Debug.Log($"[NeonFruitsGameService] Game session ended. Final balance: {response.Balance}");
                return response;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[NeonFruitsGameService] Failed to end game session: {ex.Message}");
                throw new Energy8Exception("Не удалось завершить игровую сессию", ex.Message);
            }
        }

        #endregion
    }
}
