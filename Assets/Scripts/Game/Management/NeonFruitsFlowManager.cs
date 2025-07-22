using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Energy8.Identity.UI.Runtime.Management.Flows;
using Energy8.Identity.Shared.Core.Contracts.Dto.Games;
using Game.Dto;
using Game.Services;
using UnityEngine;

namespace Game.Management
{
    /// <summary>
    /// Менеджер для интеграции NeonFruits игры с UI потоками
    /// Обеспечивает связь между игровой логикой и пользовательским интерфейсом
    /// </summary>
    public class NeonFruitsFlowManager
    {
        private readonly UserFlowManager userFlowManager;
        private readonly INeonFruitsGameService neonFruitsService;
        private readonly bool debugLogging;
        private string currentSessionId;

        public NeonFruitsFlowManager(
            UserFlowManager userFlowManager, 
            INeonFruitsGameService neonFruitsService,
            bool debugLogging = false)
        {
            this.userFlowManager = userFlowManager ?? throw new ArgumentNullException(nameof(userFlowManager));
            this.neonFruitsService = neonFruitsService ?? throw new ArgumentNullException(nameof(neonFruitsService));
            this.debugLogging = debugLogging;
        }

        #region Game Session Management

        /// <summary>
        /// Создает новую игровую сессию NeonFruits
        /// </summary>
        public async UniTask<GameSessionDto> CreateGameSessionAsync(CancellationToken ct)
        {
            try
            {
                if (debugLogging)
                    Debug.Log("[NeonFruitsFlowManager] Creating new game session");

                var session = await neonFruitsService.CreateSessionsAsync(ct);
                currentSessionId = session.SessionId;

                if (debugLogging)
                    Debug.Log($"[NeonFruitsFlowManager] Game session created: {currentSessionId}");

                return session;
            }
            catch (Exception ex)
            {
                if (debugLogging)
                    Debug.LogError($"[NeonFruitsFlowManager] Failed to create game session: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Инициализирует игру с Loading UI
        /// </summary>
        public async UniTask<NFGameStatusResponseDto> InitializeGameWithUIAsync(string sessionId, CancellationToken ct)
        {
            try
            {
                if (debugLogging)
                    Debug.Log($"[NeonFruitsFlowManager] Initializing game with UI for session: {sessionId}");

                // Создаем задачу инициализации
                var initializeTask = neonFruitsService.InitializeGameAsync(sessionId, ct);
                
                // Показываем Loading UI через UserFlowManager (используем рефлексию для доступа к private методу)
                var result = await ShowLoadingWithReflection(initializeTask, ct);

                if (debugLogging)
                    Debug.Log($"[NeonFruitsFlowManager] Game initialized. Status: {result.Status}, Balance: {result.Balance}");

                return result;
            }
            catch (Exception ex)
            {
                if (debugLogging)
                    Debug.LogError($"[NeonFruitsFlowManager] Failed to initialize game: {ex.Message}");
                throw;
            }
        }

        #endregion

        #region Game Actions

        /// <summary>
        /// Выполняет спин с Loading UI
        /// </summary>
        public async UniTask<NeonFruitsSpinResponseDto> SpinWithUIAsync(
            NeonFruitsSpinRequestDto spinRequest, 
            CancellationToken ct)
        {
            try
            {
                if (debugLogging)
                    Debug.Log($"[NeonFruitsFlowManager] Starting spin. Bet: {spinRequest.BetAmount}, Lines: {spinRequest.Lines}");

                // Выполняем спин
                var spinTask = neonFruitsService.SpinAsync<NeonFruitsSpinRequestDto, NeonFruitsSpinResponseDto>(
                    spinRequest, ct);
                
                // Показываем Loading UI
                var result = await ShowLoadingWithReflection(spinTask, ct);

                if (debugLogging)
                {
                    Debug.Log($"[NeonFruitsFlowManager] Spin completed:");
                    Debug.Log($"  - Win Amount: {result.WinAmount}");
                    Debug.Log($"  - New Balance: {result.Balance}");
                    Debug.Log($"  - Free Spins: {result.FreeSpinsTriggered} ({result.FreeSpinsCount})");
                    Debug.Log($"  - Reel Results: [{string.Join(", ", result.ReelResults)}]");
                }

                return result;
            }
            catch (Exception ex)
            {
                if (debugLogging)
                    Debug.LogError($"[NeonFruitsFlowManager] Spin failed: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Активирует бонус с Loading UI
        /// </summary>
        public async UniTask<NFGameStatusResponseDto> ActivateBonusWithUIAsync(string sessionId, CancellationToken ct)
        {
            try
            {
                if (debugLogging)
                    Debug.Log($"[NeonFruitsFlowManager] Activating bonus for session: {sessionId}");

                var bonusTask = neonFruitsService.ActivateBonusAsync(sessionId, ct);
                var result = await ShowLoadingWithReflection(bonusTask, ct);

                if (debugLogging)
                    Debug.Log($"[NeonFruitsFlowManager] Bonus activated. New status: {result.Status}");

                return result;
            }
            catch (Exception ex)
            {
                if (debugLogging)
                    Debug.LogError($"[NeonFruitsFlowManager] Failed to activate bonus: {ex.Message}");
                throw;
            }
        }

        #endregion

        #region Game Information

        /// <summary>
        /// Получает статус игры
        /// </summary>
        public async UniTask<NFGameStatusResponseDto> GetGameStatusAsync(string sessionId, CancellationToken ct)
        {
            try
            {
                var status = await neonFruitsService.GetGameStatusAsync(sessionId, ct);
                
                if (debugLogging)
                    Debug.Log($"[NeonFruitsFlowManager] Game status: {status.Status}, Balance: {status.Balance}");

                return status;
            }
            catch (Exception ex)
            {
                if (debugLogging)
                    Debug.LogError($"[NeonFruitsFlowManager] Failed to get game status: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Получает данные игрового пользователя
        /// </summary>
        public async UniTask<GameUserDto> GetGameUserAsync(CancellationToken ct)
        {
            try
            {
                var user = await neonFruitsService.GetUserAsync(ct);
                
                if (debugLogging)
                    Debug.Log($"[NeonFruitsFlowManager] Game user balance: {user.Balance}");

                return user;
            }
            catch (Exception ex)
            {
                if (debugLogging)
                    Debug.LogError($"[NeonFruitsFlowManager] Failed to get game user: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Завершает игровую сессию
        /// </summary>
        public async UniTask<NFGameStatusResponseDto> EndGameSessionAsync(string sessionId, CancellationToken ct)
        {
            try
            {
                if (debugLogging)
                    Debug.Log($"[NeonFruitsFlowManager] Ending game session: {sessionId}");

                var result = await neonFruitsService.EndGameSessionAsync(sessionId, ct);
                
                if (sessionId == currentSessionId)
                    currentSessionId = null;

                if (debugLogging)
                    Debug.Log($"[NeonFruitsFlowManager] Game session ended. Final balance: {result.Balance}");

                return result;
            }
            catch (Exception ex)
            {
                if (debugLogging)
                    Debug.LogError($"[NeonFruitsFlowManager] Failed to end game session: {ex.Message}");
                throw;
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Показывает Loading UI используя рефлексию для доступа к private методу UserFlowManager
        /// </summary>
        private async UniTask<T> ShowLoadingWithReflection<T>(UniTask<T> task, CancellationToken ct)
        {
            try
            {
                // Пытаемся получить доступ к private методу ShowLoadingAsync через рефлексию
                var method = userFlowManager.GetType().GetMethod("ShowLoadingAsync", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (method != null)
                {
                    // Делаем generic версию метода
                    var genericMethod = method.MakeGenericMethod(typeof(T));
                    var result = (UniTask<T>)genericMethod.Invoke(userFlowManager, new object[] { task, ct });
                    return await result;
                }
                else
                {
                    if (debugLogging)
                        Debug.LogWarning("[NeonFruitsFlowManager] ShowLoadingAsync method not found, executing without UI");
                    
                    return await task;
                }
            }
            catch (Exception ex)
            {
                if (debugLogging)
                    Debug.LogWarning($"[NeonFruitsFlowManager] Failed to show loading UI: {ex.Message}, executing without UI");
                
                return await task;
            }
        }

        /// <summary>
        /// Получает текущий ID сессии
        /// </summary>
        public string GetCurrentSessionId()
        {
            return currentSessionId;
        }

        /// <summary>
        /// Проверяет, есть ли активная сессия
        /// </summary>
        public bool HasActiveSession()
        {
            return !string.IsNullOrEmpty(currentSessionId);
        }

        #endregion
    }
}
