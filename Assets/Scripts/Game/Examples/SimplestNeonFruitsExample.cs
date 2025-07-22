using UnityEngine;
using Game.Factory;
using Game.Services;
using Cysharp.Threading.Tasks;

namespace Game.Examples
{
    /// <summary>
    /// Простейший пример использования NeonFruits сервиса без DI интеграции
    /// Демонстрирует прямое использование всех ваших методов
    /// </summary>
    public class SimplestNeonFruitsExample : MonoBehaviour
    {
        [Header("Game Settings")]
        [SerializeField] private string gameEndpoint = "neon-fruits";
        [SerializeField] private long betAmount = 100;
        [SerializeField] private int lines = 20;

        private INeonFruitsGameService gameService;
        private string currentSessionId;

        private void Start()
        {
            InitializeGameService();
        }

        private void InitializeGameService()
        {
            try
            {
                // Создаем игровой сервис одной строчкой
                gameService = NeonFruitsGameServiceFactory.CreateService(gameEndpoint);
                Debug.Log("✅ NeonFruitsGameService created successfully!");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"❌ Failed to create game service: {ex.Message}");
            }
        }

        #region Context Menu Methods (ваши методы!)

        [ContextMenu("1. Get User")]
        public async void TestGetUser()
        {
            if (gameService == null)
            {
                Debug.LogError("Game service not initialized!");
                return;
            }

            try
            {
                var user = await gameService.GetUserAsync(this.GetCancellationTokenOnDestroy());
                Debug.Log($"✅ User Balance: {user.Balance}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"❌ GetUser failed: {ex.Message}");
            }
        }

        [ContextMenu("2. Create Session + Initialize")]
        public async void TestCreateAndInitialize()
        {
            if (gameService == null)
            {
                Debug.LogError("Game service not initialized!");
                return;
            }

            try
            {
                var ct = this.GetCancellationTokenOnDestroy();

                // CreateSessionsAsync (ваш переопределенный метод)
                var session = await gameService.CreateSessionsAsync(ct);
                currentSessionId = session.SessionId;
                Debug.Log($"✅ Session created: {currentSessionId}");

                // InitializeGameAsync (ваш кастомный метод)
                var gameStatus = await gameService.InitializeGameAsync(currentSessionId, ct);
                Debug.Log($"✅ Game initialized:");
                Debug.Log($"   Balance: {gameStatus.Balance}");
                Debug.Log($"   Status: {gameStatus.Status}");
                Debug.Log($"   Bonus Available: {gameStatus.BonusAvailable}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"❌ Create/Initialize failed: {ex.Message}");
            }
        }

        [ContextMenu("3. Spin")]
        public async void TestSpin()
        {
            if (gameService == null || string.IsNullOrEmpty(currentSessionId))
            {
                Debug.LogError("Game service not initialized or no session!");
                return;
            }

            try
            {
                // SpinAsync (ваш generic кастомный метод)
                var spinRequest = new Game.Dto.NeonFruitsSpinRequestDto(currentSessionId, betAmount, lines);
                var result = await gameService.SpinAsync<Game.Dto.NeonFruitsSpinRequestDto, Game.Dto.NeonFruitsSpinResponseDto>(
                    spinRequest, this.GetCancellationTokenOnDestroy());

                Debug.Log($"✅ Spin completed:");
                Debug.Log($"   Bet: {betAmount}");
                Debug.Log($"   Win: {result.WinAmount}");
                Debug.Log($"   New Balance: {result.Balance}");
                Debug.Log($"   Free Spins: {result.FreeSpinsTriggered} ({result.FreeSpinsCount})");
                
                if (result.ReelResults != null && result.ReelResults.Length > 0)
                {
                    Debug.Log($"   Reels: [{string.Join(", ", result.ReelResults)}]");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"❌ Spin failed: {ex.Message}");
            }
        }

        [ContextMenu("4. Activate Bonus")]
        public async void TestActivateBonus()
        {
            if (gameService == null || string.IsNullOrEmpty(currentSessionId))
            {
                Debug.LogError("Game service not initialized or no session!");
                return;
            }

            try
            {
                // ActivateBonusAsync (ваш кастомный метод)
                var bonusResult = await gameService.ActivateBonusAsync(currentSessionId, this.GetCancellationTokenOnDestroy());
                Debug.Log($"✅ Bonus activated:");
                Debug.Log($"   Status: {bonusResult.Status}");
                Debug.Log($"   Balance: {bonusResult.Balance}");
                Debug.Log($"   Bonus Available: {bonusResult.BonusAvailable}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"❌ ActivateBonus failed: {ex.Message}");
            }
        }

        [ContextMenu("5. Get Game Status")]
        public async void TestGetStatus()
        {
            if (gameService == null || string.IsNullOrEmpty(currentSessionId))
            {
                Debug.LogError("Game service not initialized or no session!");
                return;
            }

            try
            {
                // GetGameStatusAsync (дополнительный метод)
                var status = await gameService.GetGameStatusAsync(currentSessionId, this.GetCancellationTokenOnDestroy());
                Debug.Log($"✅ Game status:");
                Debug.Log($"   Status: {status.Status}");
                Debug.Log($"   Balance: {status.Balance}");
                Debug.Log($"   Bonus Available: {status.BonusAvailable}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"❌ GetStatus failed: {ex.Message}");
            }
        }

        [ContextMenu("6. End Game Session")]
        public async void TestEndGame()
        {
            if (gameService == null || string.IsNullOrEmpty(currentSessionId))
            {
                Debug.LogError("Game service not initialized or no session!");
                return;
            }

            try
            {
                // EndGameSessionAsync (дополнительный метод)
                var finalStatus = await gameService.EndGameSessionAsync(currentSessionId, this.GetCancellationTokenOnDestroy());
                Debug.Log($"✅ Game session ended:");
                Debug.Log($"   Final Balance: {finalStatus.Balance}");
                Debug.Log($"   Final Status: {finalStatus.Status}");
                
                currentSessionId = null;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"❌ EndGame failed: {ex.Message}");
            }
        }

        [ContextMenu("🎮 Run Full Game Cycle")]
        public async void TestFullGameCycle()
        {
            Debug.Log("🎮 Starting full NeonFruits game cycle...");

            await System.Threading.Tasks.Task.Delay(100);
            TestGetUser();
            
            await System.Threading.Tasks.Task.Delay(1000);
            TestCreateAndInitialize();
            
            await System.Threading.Tasks.Task.Delay(1000);
            TestGetStatus();
            
            await System.Threading.Tasks.Task.Delay(1000);
            TestSpin();
            
            await System.Threading.Tasks.Task.Delay(1000);
            TestSpin(); // Второй спин
            
            await System.Threading.Tasks.Task.Delay(1000);
            TestActivateBonus();
            
            await System.Threading.Tasks.Task.Delay(1000);
            TestEndGame();

            Debug.Log("🎉 Full game cycle completed!");
        }

        #endregion

        #region Debug Info

        [ContextMenu("Show Current State")]
        public void ShowCurrentState()
        {
            Debug.Log("=== NeonFruits Service State ===");
            Debug.Log($"Service Initialized: {gameService != null}");
            Debug.Log($"Current Session: {currentSessionId ?? "None"}");
            Debug.Log($"Game Endpoint: {gameEndpoint}");
            Debug.Log($"Default Bet: {betAmount}");
            Debug.Log($"Default Lines: {lines}");
        }

        #endregion
    }
}
