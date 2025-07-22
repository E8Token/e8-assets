using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Dto;
using Game.Factory;
using Game.Services;
using UnityEngine;

namespace Game.Controllers
{
    /// <summary>
    /// Основной контроллер для управления игрой NeonFruits
    /// Демонстрирует полный игровой цикл с интеграцией UI
    /// </summary>
    public class NeonFruitsGameController : MonoBehaviour
    {
        [Header("Game Settings")]
        [SerializeField] private string gameEndpoint = "neon-fruits";
        [SerializeField] private bool enableDebugLogging = true;
        [SerializeField] private long defaultBetAmount = 100;
        [SerializeField] private int defaultLines = 20;

        [Header("Auto Play Settings")]
        [SerializeField] private bool autoPlayEnabled = false;
        [SerializeField] private int autoPlayCount = 10;

        // Сервисы
        private INeonFruitsGameService neonFruitsService;
        
        // Состояние игры
        private string currentSessionId;
        private bool gameInitialized = false;
        private CancellationTokenSource cancellationTokenSource;

        #region Unity Lifecycle

        private void Start()
        {
            Initialize();
        }

        private void OnDestroy()
        {
            cancellationTokenSource?.Cancel();
            cancellationTokenSource?.Dispose();
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Инициализирует игровые сервисы
        /// </summary>
        private void Initialize()
        {
            try
            {
                cancellationTokenSource = new CancellationTokenSource();

                if (enableDebugLogging)
                    Debug.Log("[NeonFruitsGameController] Initializing game controller");

                // Создаем игровой сервис
                neonFruitsService = NeonFruitsGameServiceFactory.CreateService(gameEndpoint);

                if (enableDebugLogging)
                    Debug.Log("[NeonFruitsGameController] Game controller initialized successfully");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[NeonFruitsGameController] Failed to initialize: {ex.Message}");
            }
        }

        #endregion

        #region Public API (для UI кнопок)

        /// <summary>
        /// Запускает новую игру
        /// </summary>
        public async void StartNewGame()
        {
            try
            {
                await StartNewGameAsync(cancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[NeonFruitsGameController] Failed to start new game: {ex.Message}");
            }
        }

        /// <summary>
        /// Выполняет спин
        /// </summary>
        public async void Spin()
        {
            try
            {
                await SpinAsync(defaultBetAmount, defaultLines, cancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[NeonFruitsGameController] Failed to spin: {ex.Message}");
            }
        }

        /// <summary>
        /// Активирует бонус
        /// </summary>
        public async void ActivateBonus()
        {
            try
            {
                await ActivateBonusAsync(cancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[NeonFruitsGameController] Failed to activate bonus: {ex.Message}");
            }
        }

        /// <summary>
        /// Завершает игру
        /// </summary>
        public async void EndGame()
        {
            try
            {
                await EndGameAsync(cancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[NeonFruitsGameController] Failed to end game: {ex.Message}");
            }
        }

        #endregion

        #region Game Logic

        /// <summary>
        /// Запускает новую игровую сессию
        /// </summary>
        public async UniTask StartNewGameAsync(CancellationToken ct)
        {
            if (neonFruitsService == null)
            {
                Debug.LogError("[NeonFruitsGameController] NeonFruitsGameService is null");
                return;
            }

            try
            {
                if (enableDebugLogging)
                    Debug.Log("[NeonFruitsGameController] Starting new game");

                // 1. Создаем игровую сессию
                var session = await neonFruitsService.CreateSessionsAsync(ct);
                currentSessionId = session.SessionId;

                // 2. Инициализируем игру  
                var gameStatus = await neonFruitsService.InitializeGameAsync(currentSessionId, ct);
                gameInitialized = true;

                if (enableDebugLogging)
                {
                    Debug.Log($"[NeonFruitsGameController] Game started successfully");
                    Debug.Log($"  - Session ID: {currentSessionId}");
                    Debug.Log($"  - Initial Balance: {gameStatus.Balance}");
                    Debug.Log($"  - Bonus Available: {gameStatus.BonusAvailable}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[NeonFruitsGameController] Failed to start game: {ex.Message}");
                gameInitialized = false;
                throw;
            }
        }

        /// <summary>
        /// Выполняет спин в слоте
        /// </summary>
        public async UniTask<NeonFruitsSpinResponseDto> SpinAsync(long betAmount, int lines, CancellationToken ct)
        {
            if (!ValidateGameState())
                throw new InvalidOperationException("Game not properly initialized");

            try
            {
                if (enableDebugLogging)
                    Debug.Log($"[NeonFruitsGameController] Spinning with bet: {betAmount}, lines: {lines}");

                // Создаем запрос спина
                var spinRequest = new NeonFruitsSpinRequestDto(
                    currentSessionId, 
                    betAmount, 
                    lines, 
                    autoPlayEnabled, 
                    autoPlayCount);

                // Выполняем спин
                var result = await neonFruitsService.SpinAsync<NeonFruitsSpinRequestDto, NeonFruitsSpinResponseDto>(
                    spinRequest, ct);

                if (enableDebugLogging)
                {
                    Debug.Log($"[NeonFruitsGameController] Spin result:");
                    Debug.Log($"  - Win: {result.WinAmount}");
                    Debug.Log($"  - Balance: {result.Balance}");
                    Debug.Log($"  - Free Spins: {result.FreeSpinsTriggered}");
                }

                return result;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[NeonFruitsGameController] Spin failed: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Активирует бонусную игру
        /// </summary>
        public async UniTask<NFGameStatusResponseDto> ActivateBonusAsync(CancellationToken ct)
        {
            if (!ValidateGameState())
                throw new InvalidOperationException("Game not properly initialized");

            try
            {
                if (enableDebugLogging)
                    Debug.Log("[NeonFruitsGameController] Activating bonus");

                var result = await neonFruitsService.ActivateBonusAsync(currentSessionId, ct);

                if (enableDebugLogging)
                    Debug.Log($"[NeonFruitsGameController] Bonus activated. Status: {result.Status}");

                return result;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[NeonFruitsGameController] Failed to activate bonus: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Завершает текущую игровую сессию
        /// </summary>
        public async UniTask EndGameAsync(CancellationToken ct)
        {
            if (string.IsNullOrEmpty(currentSessionId))
                return;

            try
            {
                if (enableDebugLogging)
                    Debug.Log($"[NeonFruitsGameController] Ending game session: {currentSessionId}");

                var finalStatus = await neonFruitsService.EndGameSessionAsync(currentSessionId, ct);
                
                // Сбрасываем состояние
                currentSessionId = null;
                gameInitialized = false;

                if (enableDebugLogging)
                    Debug.Log($"[NeonFruitsGameController] Game ended. Final balance: {finalStatus.Balance}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[NeonFruitsGameController] Failed to end game: {ex.Message}");
                throw;
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Проверяет валидность состояния игры
        /// </summary>
        private bool ValidateGameState()
        {
            if (neonFruitsService == null)
            {
                Debug.LogError("[NeonFruitsGameController] NeonFruitsGameService is null");
                return false;
            }

            if (!gameInitialized)
            {
                Debug.LogError("[NeonFruitsGameController] Game not initialized. Call StartNewGame() first");
                return false;
            }

            if (string.IsNullOrEmpty(currentSessionId))
            {
                Debug.LogError("[NeonFruitsGameController] No active session. Call StartNewGame() first");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Получает текущее состояние игры для отладки
        /// </summary>
        [ContextMenu("Get Game State")]
        public void LogGameState()
        {
            Debug.Log($"[NeonFruitsGameController] Current Game State:");
            Debug.Log($"  - Session ID: {currentSessionId ?? "None"}");
            Debug.Log($"  - Game Initialized: {gameInitialized}");
            Debug.Log($"  - Service Ready: {neonFruitsService != null}");
        }

        #endregion
    }
}
