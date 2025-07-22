using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Dto;
using Game.Factory;
using Game.Services;
using UnityEngine;

namespace Game.Examples
{
    /// <summary>
    /// Простой пример использования NeonFruitsGameService без UI интеграции
    /// Полезен для тестирования API или использования в headless режиме
    /// </summary>
    public class SimpleNeonFruitsExample : MonoBehaviour
    {
        [Header("Game Configuration")]
        [SerializeField] private string gameEndpoint = "neon-fruits";
        [SerializeField] private long betAmount = 100;
        [SerializeField] private int lines = 20;

        private INeonFruitsGameService gameService;
        private CancellationTokenSource cts;

        private async void Start()
        {
            cts = new CancellationTokenSource();
            await RunGameExample();
        }

        private void OnDestroy()
        {
            cts?.Cancel();
            cts?.Dispose();
        }

        /// <summary>
        /// Запускает полный пример игрового цикла
        /// </summary>
        private async UniTask RunGameExample()
        {
            try
            {
                Debug.Log("=== NeonFruits Game Example Started ===");

                // 1. Создаем игровой сервис
                gameService = NeonFruitsGameServiceFactory.CreateService(gameEndpoint);
                Debug.Log("✓ Game service created");

                // 2. Получаем данные пользователя
                var user = await gameService.GetUserAsync(cts.Token);
                Debug.Log($"✓ User data: Balance = {user.Balance}");

                // 3. Создаем игровую сессию
                var session = await gameService.CreateSessionsAsync(cts.Token);
                Debug.Log($"✓ Game session created: {session.SessionId}");

                // 4. Инициализируем игру
                var gameStatus = await gameService.InitializeGameAsync(session.SessionId, cts.Token);
                Debug.Log($"✓ Game initialized: Status = {gameStatus.Status}, Balance = {gameStatus.Balance}");

                // 5. Выполняем несколько спинов
                for (int i = 1; i <= 3; i++)
                {
                    Debug.Log($"\n--- Spin #{i} ---");
                    await PerformSpin(session.SessionId, betAmount, lines);
                    
                    // Небольшая пауза между спинами
                    await UniTask.Delay(1000, cancellationToken: cts.Token);
                }

                // 6. Проверяем статус игры
                var finalStatus = await gameService.GetGameStatusAsync(session.SessionId, cts.Token);
                Debug.Log($"\n✓ Final game status: Balance = {finalStatus.Balance}, Bonus = {finalStatus.BonusAvailable}");

                // 7. Завершаем сессию
                var endResult = await gameService.EndGameSessionAsync(session.SessionId, cts.Token);
                Debug.Log($"✓ Game session ended: Final balance = {endResult.Balance}");

                Debug.Log("=== NeonFruits Game Example Completed ===");
            }
            catch (Exception ex)
            {
                Debug.LogError($"❌ Game example failed: {ex.Message}");
                Debug.LogException(ex);
            }
        }

        /// <summary>
        /// Выполняет один спин
        /// </summary>
        private async UniTask PerformSpin(string sessionId, long bet, int linesCount)
        {
            try
            {
                // Создаем запрос спина
                var spinRequest = new NeonFruitsSpinRequestDto(sessionId, bet, linesCount);

                // Выполняем спин
                var spinResult = await gameService.SpinAsync<NeonFruitsSpinRequestDto, NeonFruitsSpinResponseDto>(
                    spinRequest, cts.Token);

                // Выводим результаты
                Debug.Log($"  Bet: {bet}, Win: {spinResult.WinAmount}, Balance: {spinResult.Balance}");
                
                if (spinResult.ReelResults != null && spinResult.ReelResults.Length > 0)
                {
                    Debug.Log($"  Reels: [{string.Join(", ", spinResult.ReelResults)}]");
                }

                if (spinResult.FreeSpinsTriggered)
                {
                    Debug.Log($"  🎰 FREE SPINS TRIGGERED! Count: {spinResult.FreeSpinsCount}");
                }

                if (spinResult.WinAmount > 0)
                {
                    Debug.Log($"  🎉 WIN! Amount: {spinResult.WinAmount}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"  ❌ Spin failed: {ex.Message}");
            }
        }

        #region Manual Testing Buttons (for Inspector)

        [ContextMenu("Test Create Service")]
        public void TestCreateService()
        {
            try
            {
                gameService = NeonFruitsGameServiceFactory.CreateService(gameEndpoint);
                Debug.Log("✓ Service created successfully");
            }
            catch (Exception ex)
            {
                Debug.LogError($"❌ Failed to create service: {ex.Message}");
            }
        }

        [ContextMenu("Test Get User")]
        public async void TestGetUser()
        {
            if (gameService == null)
            {
                Debug.LogError("Game service not initialized");
                return;
            }

            try
            {
                var user = await gameService.GetUserAsync(cts.Token);
                Debug.Log($"✓ User balance: {user.Balance}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"❌ Failed to get user: {ex.Message}");
            }
        }

        [ContextMenu("Test Create Session")]
        public async void TestCreateSession()
        {
            if (gameService == null)
            {
                Debug.LogError("Game service not initialized");
                return;
            }

            try
            {
                var session = await gameService.CreateSessionsAsync(cts.Token);
                Debug.Log($"✓ Session created: {session.SessionId}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"❌ Failed to create session: {ex.Message}");
            }
        }

        #endregion
    }
}
