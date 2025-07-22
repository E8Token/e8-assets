using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Dto;
using Game.Factory;
using Game.Services;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Examples
{
    /// <summary>
    /// Простой UI пример с кнопками для тестирования всех методов NeonFruitsGameService
    /// Демонстрирует прямой доступ к методам без лишних оберток
    /// </summary>
    public class SimpleNeonFruitsUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button startGameButton;
        [SerializeField] private Button spinButton;
        [SerializeField] private Button activateBonusButton;
        [SerializeField] private Button getStatusButton;
        [SerializeField] private Button endGameButton;
        [SerializeField] private Button getUserButton;
        
        [Header("Game Settings")]
        [SerializeField] private string gameEndpoint = "neon-fruits";
        [SerializeField] private long betAmount = 100;
        [SerializeField] private int lines = 20;
        
        [Header("Display")]
        [SerializeField] private Text statusText;
        [SerializeField] private Text balanceText;
        [SerializeField] private Text sessionText;

        private INeonFruitsGameService gameService;
        private string currentSessionId;
        private CancellationTokenSource cts;

        private void Start()
        {
            InitializeUI();
            InitializeGameService();
        }

        private void OnDestroy()
        {
            cts?.Cancel();
            cts?.Dispose();
        }

        private void InitializeUI()
        {
            cts = new CancellationTokenSource();

            // Настраиваем кнопки
            if (startGameButton) startGameButton.onClick.AddListener(() => StartGame().Forget());
            if (spinButton) spinButton.onClick.AddListener(() => Spin().Forget());
            if (activateBonusButton) activateBonusButton.onClick.AddListener(() => ActivateBonus().Forget());
            if (getStatusButton) getStatusButton.onClick.AddListener(() => GetStatus().Forget());
            if (endGameButton) endGameButton.onClick.AddListener(() => EndGame().Forget());
            if (getUserButton) getUserButton.onClick.AddListener(() => GetUser().Forget());

            UpdateUI("Ready to play", "0", "No session");
        }

        private void InitializeGameService()
        {
            try
            {
                gameService = NeonFruitsGameServiceFactory.CreateService(gameEndpoint);
                Debug.Log("✅ NeonFruitsGameService created successfully");
            }
            catch (Exception ex)
            {
                Debug.LogError($"❌ Failed to create game service: {ex.Message}");
                UpdateUI($"Error: {ex.Message}", "0", "Failed");
            }
        }

        #region Button Handlers (Ваши методы!)

        /// <summary>
        /// 1. Получение данных пользователя
        /// </summary>
        private async UniTaskVoid GetUser()
        {
            try
            {
                UpdateUI("Getting user data...", "-", "-");
                
                var user = await gameService.GetUserAsync(cts.Token);
                
                UpdateUI("User data received", user.Balance.ToString(), currentSessionId ?? "No session");
                Debug.Log($"✅ User Balance: {user.Balance}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"❌ GetUser failed: {ex.Message}");
                UpdateUI($"GetUser Error: {ex.Message}", "-", "-");
            }
        }

        /// <summary>
        /// 2. Создание сессии + инициализация игры
        /// </summary>
        private async UniTaskVoid StartGame()
        {
            try
            {
                UpdateUI("Creating session...", "-", "-");

                // CreateSessionsAsync (переопределенный метод)
                var session = await gameService.CreateSessionsAsync(cts.Token);
                currentSessionId = session.SessionId;
                
                UpdateUI("Initializing game...", "-", currentSessionId);

                // InitializeGameAsync (ваш кастомный метод)
                var gameStatus = await gameService.InitializeGameAsync(currentSessionId, cts.Token);
                
                UpdateUI($"Game ready! Bonus: {gameStatus.BonusAvailable}", 
                    gameStatus.Balance.ToString(), currentSessionId);
                
                Debug.Log($"✅ Game started:");
                Debug.Log($"   Session: {currentSessionId}");
                Debug.Log($"   Balance: {gameStatus.Balance}");
                Debug.Log($"   Status: {gameStatus.Status}");
                Debug.Log($"   Bonus Available: {gameStatus.BonusAvailable}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"❌ StartGame failed: {ex.Message}");
                UpdateUI($"Start Error: {ex.Message}", "-", "Failed");
            }
        }

        /// <summary>
        /// 3. Выполнение спина (ваш главный метод!)
        /// </summary>
        private async UniTaskVoid Spin()
        {
            if (string.IsNullOrEmpty(currentSessionId))
            {
                Debug.LogWarning("No active session. Start game first!");
                UpdateUI("No session! Start game first", "-", "-");
                return;
            }

            try
            {
                UpdateUI("Spinning...", "-", currentSessionId);

                // Создаем запрос спина
                var spinRequest = new NeonFruitsSpinRequestDto(currentSessionId, betAmount, lines);
                
                // SpinAsync (ваш кастомный generic метод!)
                var result = await gameService.SpinAsync<NeonFruitsSpinRequestDto, NeonFruitsSpinResponseDto>(
                    spinRequest, cts.Token);

                var status = $"Win: {result.WinAmount} | FS: {result.FreeSpinsTriggered}";
                UpdateUI(status, result.Balance.ToString(), currentSessionId);

                Debug.Log($"✅ Spin completed:");
                Debug.Log($"   Bet: {betAmount} | Win: {result.WinAmount}");
                Debug.Log($"   Balance: {result.Balance}");
                Debug.Log($"   Free Spins: {result.FreeSpinsTriggered} ({result.FreeSpinsCount})");
                Debug.Log($"   Reels: [{string.Join(", ", result.ReelResults ?? new int[0])}]");
            }
            catch (Exception ex)
            {
                Debug.LogError($"❌ Spin failed: {ex.Message}");
                UpdateUI($"Spin Error: {ex.Message}", "-", currentSessionId);
            }
        }

        /// <summary>
        /// 4. Активация бонуса
        /// </summary>
        private async UniTaskVoid ActivateBonus()
        {
            if (string.IsNullOrEmpty(currentSessionId))
            {
                Debug.LogWarning("No active session. Start game first!");
                return;
            }

            try
            {
                UpdateUI("Activating bonus...", "-", currentSessionId);

                // ActivateBonusAsync (ваш кастомный метод)
                var result = await gameService.ActivateBonusAsync(currentSessionId, cts.Token);
                
                UpdateUI($"Bonus activated! Status: {result.Status}", 
                    result.Balance.ToString(), currentSessionId);

                Debug.Log($"✅ Bonus activated:");
                Debug.Log($"   Status: {result.Status}");
                Debug.Log($"   Balance: {result.Balance}");
                Debug.Log($"   Bonus Available: {result.BonusAvailable}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"❌ ActivateBonus failed: {ex.Message}");
                UpdateUI($"Bonus Error: {ex.Message}", "-", currentSessionId);
            }
        }

        /// <summary>
        /// 5. Получение статуса игры
        /// </summary>
        private async UniTaskVoid GetStatus()
        {
            if (string.IsNullOrEmpty(currentSessionId))
            {
                Debug.LogWarning("No active session. Start game first!");
                return;
            }

            try
            {
                UpdateUI("Getting game status...", "-", currentSessionId);

                // GetGameStatusAsync (ваш кастомный метод)
                var status = await gameService.GetGameStatusAsync(currentSessionId, cts.Token);
                
                UpdateUI($"Status: {status.Status} | Bonus: {status.BonusAvailable}", 
                    status.Balance.ToString(), currentSessionId);

                Debug.Log($"✅ Game status:");
                Debug.Log($"   Status: {status.Status}");
                Debug.Log($"   Balance: {status.Balance}");
                Debug.Log($"   Bonus Available: {status.BonusAvailable}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"❌ GetStatus failed: {ex.Message}");
                UpdateUI($"Status Error: {ex.Message}", "-", currentSessionId);
            }
        }

        /// <summary>
        /// 6. Завершение игровой сессии
        /// </summary>
        private async UniTaskVoid EndGame()
        {
            if (string.IsNullOrEmpty(currentSessionId))
            {
                Debug.LogWarning("No active session to end!");
                return;
            }

            try
            {
                UpdateUI("Ending game...", "-", currentSessionId);

                // EndGameSessionAsync (ваш кастомный метод)
                var finalStatus = await gameService.EndGameSessionAsync(currentSessionId, cts.Token);
                
                UpdateUI("Game ended", finalStatus.Balance.ToString(), "Session ended");
                
                Debug.Log($"✅ Game ended:");
                Debug.Log($"   Final Balance: {finalStatus.Balance}");
                Debug.Log($"   Final Status: {finalStatus.Status}");
                
                currentSessionId = null;
            }
            catch (Exception ex)
            {
                Debug.LogError($"❌ EndGame failed: {ex.Message}");
                UpdateUI($"End Error: {ex.Message}", "-", "Error");
            }
        }

        #endregion

        #region UI Updates

        private void UpdateUI(string status, string balance, string session)
        {
            if (statusText) statusText.text = $"Status: {status}";
            if (balanceText) balanceText.text = $"Balance: {balance}";
            if (sessionText) sessionText.text = $"Session: {session}";
        }

        #endregion

        #region Context Menu для тестирования в Editor

        [ContextMenu("Quick Test All Methods")]
        private async void QuickTestAllMethods()
        {
            try
            {
                Debug.Log("=== Quick Test Started ===");
                
                // 1. Get User
                await GetUserTask();
                await UniTask.Delay(500);
                
                // 2. Start Game (Create Session + Initialize)
                await StartGameTask();
                await UniTask.Delay(500);
                
                // 3. Get Status
                await GetStatusTask();
                await UniTask.Delay(500);
                
                // 4. Spin
                await SpinTask();
                await UniTask.Delay(500);
                
                // 5. End Game
                await EndGameTask();
                
                Debug.Log("=== Quick Test Completed ===");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Quick test failed: {ex.Message}");
            }
        }

        // Helper methods that return UniTask instead of UniTaskVoid
        private async UniTask GetUserTask()
        {
            try
            {
                UpdateUI("Getting user data...", "-", "-");
                var user = await gameService.GetUserAsync(cts.Token);
                UpdateUI("User data received", user.Balance.ToString(), currentSessionId ?? "No session");
                Debug.Log($"✅ User Balance: {user.Balance}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"❌ GetUser failed: {ex.Message}");
                UpdateUI($"GetUser Error: {ex.Message}", "-", "-");
            }
        }

        private async UniTask StartGameTask()
        {
            try
            {
                UpdateUI("Creating session...", "-", "-");
                var session = await gameService.CreateSessionsAsync(cts.Token);
                currentSessionId = session.SessionId;
                
                UpdateUI("Initializing game...", "-", currentSessionId);
                var gameStatus = await gameService.InitializeGameAsync(currentSessionId, cts.Token);
                
                UpdateUI($"Game ready! Bonus: {gameStatus.BonusAvailable}", 
                    gameStatus.Balance.ToString(), currentSessionId);
                
                Debug.Log($"✅ Game started: Session: {currentSessionId}, Balance: {gameStatus.Balance}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"❌ StartGame failed: {ex.Message}");
                UpdateUI($"Start Error: {ex.Message}", "-", "Failed");
            }
        }

        private async UniTask GetStatusTask()
        {
            if (string.IsNullOrEmpty(currentSessionId))
            {
                Debug.LogWarning("No active session. Start game first!");
                return;
            }

            try
            {
                UpdateUI("Getting game status...", "-", currentSessionId);
                var status = await gameService.GetGameStatusAsync(currentSessionId, cts.Token);
                
                UpdateUI($"Status: {status.Status} | Bonus: {status.BonusAvailable}", 
                    status.Balance.ToString(), currentSessionId);

                Debug.Log($"✅ Game status: {status.Status}, Balance: {status.Balance}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"❌ GetStatus failed: {ex.Message}");
                UpdateUI($"Status Error: {ex.Message}", "-", currentSessionId);
            }
        }

        private async UniTask SpinTask()
        {
            if (string.IsNullOrEmpty(currentSessionId))
            {
                Debug.LogWarning("No active session. Start game first!");
                return;
            }

            try
            {
                UpdateUI("Spinning...", "-", currentSessionId);
                var spinRequest = new NeonFruitsSpinRequestDto(currentSessionId, betAmount, lines);
                
                var result = await gameService.SpinAsync<NeonFruitsSpinRequestDto, NeonFruitsSpinResponseDto>(
                    spinRequest, cts.Token);

                var status = $"Win: {result.WinAmount} | FS: {result.FreeSpinsTriggered}";
                UpdateUI(status, result.Balance.ToString(), currentSessionId);

                Debug.Log($"✅ Spin completed: Bet: {betAmount}, Win: {result.WinAmount}, Balance: {result.Balance}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"❌ Spin failed: {ex.Message}");
                UpdateUI($"Spin Error: {ex.Message}", "-", currentSessionId);
            }
        }

        private async UniTask EndGameTask()
        {
            if (string.IsNullOrEmpty(currentSessionId))
            {
                Debug.LogWarning("No active session to end!");
                return;
            }

            try
            {
                UpdateUI("Ending game...", "-", currentSessionId);
                var finalStatus = await gameService.EndGameSessionAsync(currentSessionId, cts.Token);
                
                UpdateUI("Game ended", finalStatus.Balance.ToString(), "Session ended");
                Debug.Log($"✅ Game ended: Final Balance: {finalStatus.Balance}");
                
                currentSessionId = null;
            }
            catch (Exception ex)
            {
                Debug.LogError($"❌ EndGame failed: {ex.Message}");
                UpdateUI($"End Error: {ex.Message}", "-", "Error");
            }
        }

        #endregion
    }
}
