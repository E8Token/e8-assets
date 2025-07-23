using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Dto;
using Game.Factory;
using Game.Services;
using Energy8.Identity.UI.Runtime.Controllers;
using Energy8.Identity.Http.Core;
using UnityEngine;

namespace Game.Controllers
{
    /// <summary>
    /// Простой контроллер для управления игровым сервисом NeonFruits
    /// Использует HttpClient из Identity системы для переиспользования соединений.
    /// </summary>
    public class NeonFruitsGameController : MonoBehaviour
    {
        [Header("Game Settings")]
        [SerializeField] private string gameEndpoint = "neon-fruits";
        [SerializeField] private bool enableDebugLogging = true;

        // Игровой сервис
        private INeonFruitsGameService gameService;
        private CancellationTokenSource cancellationTokenSource;

        private void Start()
        {
            Initialize();
        }

        private void OnDestroy()
        {
            cancellationTokenSource?.Cancel();
            cancellationTokenSource?.Dispose();
        }

        /// <summary>
        /// Инициализирует игровой сервис
        /// </summary>
        private void Initialize()
        {
            try
            {
                cancellationTokenSource = new CancellationTokenSource();
                
                // Получаем HttpClient из Identity системы для переиспользования
                if (IdentityOrchestrator.Instance != null)
                {
                    var httpClient = IdentityOrchestrator.Instance.ServiceContainer.Resolve<IHttpClient>();
                    gameService = NeonFruitsGameServiceFactory.CreateService(httpClient, gameEndpoint);
                }
                else
                {
                    // Fallback: создаем новый HttpClient если Identity система не инициализирована
                    Debug.LogWarning("[NeonFruitsGameController] IdentityOrchestrator not found, creating new HttpClient");
                    gameService = NeonFruitsGameServiceFactory.CreateService(gameEndpoint);
                }
                
                if (enableDebugLogging)
                    Debug.Log("[NeonFruitsGameController] Game service initialized");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[NeonFruitsGameController] Failed to initialize: {ex.Message}");
            }
        }

        /// <summary>
        /// Получает данные пользователя
        /// </summary>
        [ContextMenu("Get User Data")]
        public async void GetUserData()
        {
            try
            {
                var userData = await gameService.GetUserAsync(cancellationTokenSource.Token);
                if (enableDebugLogging)
                    Debug.Log($"User Balance: {userData.Balance}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to get user data: {ex.Message}");
            }
        }

        /// <summary>
        /// Создает игровую сессию
        /// </summary>
        [ContextMenu("Create Session")]
        public async void CreateSession()
        {
            try
            {
                var session = await gameService.CreateSessionsAsync(cancellationTokenSource.Token);
                if (enableDebugLogging)
                    Debug.Log($"Session created: {session.SessionId}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to create session: {ex.Message}");
            }
        }

        /// <summary>
        /// Инициализирует игру
        /// </summary>
        [ContextMenu("Initialize Game")]
        public async void InitializeGame()
        {
            try
            {
                var session = await gameService.CreateSessionsAsync(cancellationTokenSource.Token);
                var gameStatus = await gameService.InitializeGameAsync(session.SessionId, cancellationTokenSource.Token);
                if (enableDebugLogging)
                    Debug.Log($"Game initialized. Balance: {gameStatus.Balance}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to initialize game: {ex.Message}");
            }
        }

        /// <summary>
        /// Выполняет спин
        /// </summary>
        [ContextMenu("Perform Spin")]
        public async void PerformSpin()
        {
            try
            {
                var session = await gameService.CreateSessionsAsync(cancellationTokenSource.Token);
                var spinRequest = new NeonFruitsSpinRequestDto(session.SessionId, 100, 20, false, 0);
                var result = await gameService.SpinAsync<NeonFruitsSpinRequestDto, NeonFruitsSpinResponseDto>(
                    spinRequest, cancellationTokenSource.Token);
                
                if (enableDebugLogging)
                    Debug.Log($"Spin result - Win: {result.WinAmount}, Balance: {result.Balance}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to perform spin: {ex.Message}");
            }
        }

        /// <summary>
        /// Активирует бонус
        /// </summary>
        [ContextMenu("Activate Bonus")]
        public async void ActivateBonus()
        {
            try
            {
                var session = await gameService.CreateSessionsAsync(cancellationTokenSource.Token);
                var result = await gameService.ActivateBonusAsync(session.SessionId, cancellationTokenSource.Token);
                
                if (enableDebugLogging)
                    Debug.Log($"Bonus activated. Status: {result.Status}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to activate bonus: {ex.Message}");
            }
        }
    }
}
