using UnityEngine;
using Energy8.Identity.UI.Runtime.DI;
using Game.Integration;
using Game.Services;

namespace Game.Examples
{
    /// <summary>
    /// Пример настройки Identity системы с кастомным NeonFruits сервисом
    /// Используйте это как отправную точку для интеграции
    /// </summary>
    public class GameInitializationExample : MonoBehaviour
    {
        [Header("Identity Configuration")]
        [SerializeField] private bool debugLogging = true;
        [SerializeField] private bool isLiteVersion = false;
        
        [Header("Game Configuration")]
        [SerializeField] private string neonFruitsEndpoint = "neon-fruits";
        
        private IdentityServiceContainer container;

        private void Awake()
        {
            // Инициализируем DI контейнер
            InitializeIdentityWithCustomGameService();
        }

        /// <summary>
        /// Инициализирует Identity систему с кастомным игровым сервисом
        /// </summary>
        private void InitializeIdentityWithCustomGameService()
        {
            try
            {
                Debug.Log("🚀 Initializing Identity system with NeonFruits service...");
                
                // 1. Создаем DI контейнер
                container = new IdentityServiceContainer();
                
                // 2. Конфигурируем с кастомным сервисом
                NeonFruitsServiceIntegration.ConfigureWithNeonFruitsService(
                    container, 
                    debugLogging, 
                    isLiteVersion, 
                    neonFruitsEndpoint);
                
                Debug.Log("✅ Identity system initialized successfully!");
                
                // 3. Тестируем доступ к сервисам
                TestServiceAccess();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"❌ Failed to initialize Identity system: {ex.Message}");
            }
        }

        /// <summary>
        /// Тестирует доступ к различным сервисам через DI
        /// </summary>
        private void TestServiceAccess()
        {
            try
            {
                Debug.Log("🧪 Testing service access...");
                
                // Получаем базовый игровой сервис (теперь это NeonFruits!)
                var gameService = container.Resolve<Energy8.Identity.Game.Core.Services.IGameService>();
                Debug.Log($"✅ IGameService: {gameService.GetType().Name}");
                
                // Получаем UserFlowManager (с встроенной поддержкой игр)
                var userFlowManager = container.Resolve<Energy8.Identity.UI.Runtime.Management.Flows.IUserFlowManager>();
                Debug.Log($"✅ IUserFlowManager: {userFlowManager.GetType().Name}");
                
                // Получаем другие важные сервисы
                var identityService = container.Resolve<Energy8.Identity.UI.Runtime.Services.IIdentityService>();
                Debug.Log($"✅ IIdentityService: {identityService.GetType().Name}");
                
                var canvasManager = container.Resolve<Energy8.Identity.UI.Core.Management.ICanvasManager>();
                Debug.Log($"✅ ICanvasManager: {canvasManager.GetType().Name}");
                
                Debug.Log("✅ All services accessible through DI!");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"❌ Service access test failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Пример получения игрового сервиса для использования в коде
        /// </summary>
        [ContextMenu("Get Game Service Example")]
        public void GetGameServiceExample()
        {
            if (container == null)
            {
                Debug.LogWarning("Container not initialized!");
                return;
            }

            try
            {
                // Получаем игровой сервис из DI
                var gameService = container.Resolve<Energy8.Identity.Game.Core.Services.IGameService>();
                Debug.Log($"Game service type: {gameService.GetType().Name}");
                
                // Если это NeonFruits сервис, можем кастовать к расширенному интерфейсу
                if (gameService is INeonFruitsGameService neonFruitsService)
                {
                    Debug.Log("✅ Game service is NeonFruitsGameService - all custom methods available!");
                    // Теперь можно использовать: neonFruitsService.InitializeGameAsync(), SpinAsync(), etc.
                }
                else
                {
                    Debug.Log("ℹ️ Game service is basic IGameService");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to get game service: {ex.Message}");
            }
        }

        /// <summary>
        /// Пример использования UserFlowManager с игровой интеграцией
        /// </summary>
        [ContextMenu("Test User Flow With Games")]
        public async void TestUserFlowWithGames()
        {
            if (container == null)
            {
                Debug.LogWarning("Container not initialized!");
                return;
            }

            try
            {
                var userFlowManager = container.Resolve<Energy8.Identity.UI.Runtime.Management.Flows.IUserFlowManager>();
                var cts = new System.Threading.CancellationTokenSource();

                Debug.Log("🎮 Testing UserFlowManager with game integration...");

                // UserFlowManager теперь автоматически получит игровые данные при показе профиля
                // Это произойдет в методе StartUserFlowAsync()
                // await userFlowManager.StartUserFlowAsync(cts.Token);
                
                Debug.Log("✅ UserFlowManager ready for game integration!");
                
                // Не забываем освобождать ресурсы
                cts.Dispose();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"UserFlow test failed: {ex.Message}");
            }
        }

        #region Public API для других скриптов

        /// <summary>
        /// Получить DI контейнер для использования в других скриптах
        /// </summary>
        public IdentityServiceContainer GetContainer()
        {
            return container;
        }

        /// <summary>
        /// Получить игровой сервис из DI
        /// </summary>
        public T GetGameService<T>() where T : class
        {
            return container?.Resolve<T>();
        }

        /// <summary>
        /// Получить любой сервис из DI
        /// </summary>
        public T GetService<T>() where T : class
        {
            return container?.Resolve<T>();
        }

        #endregion
    }
}
