using UnityEngine;
using Energy8.Identity.UI.Runtime.DI;
using Game.Factory;
using Game.Services;

namespace Game.Integration
{
    /// <summary>
    /// Пример интеграции NeonFruits игрового сервиса с Identity DI контейнером
    /// Показывает, как заменить базовый GameService на кастомный NeonFruitsGameService
    /// </summary>
    public class NeonFruitsServiceIntegration : MonoBehaviour
    {
        [Header("Game Service Configuration")]
        [SerializeField] private string gameEndpoint = "neon-fruits";
        [SerializeField] private bool enableDebugLogging = true;
        
        /// <summary>
        /// Конфигурирует DI контейнер с кастомным NeonFruits сервисом
        /// Вызовите этот метод ДО инициализации Identity системы
        /// </summary>
        public static void ConfigureWithNeonFruitsService(
            IdentityServiceContainer container, 
            bool debugLogging, 
            bool isLite,
            string gameEndpoint = "neon-fruits")
        {
            // Создаем кастомный игровой сервис
            var neonFruitsService = NeonFruitsGameServiceFactory.CreateService(gameEndpoint);
            
            // Конфигурируем DI контейнер с кастомным сервисом
            container.ConfigureServices(debugLogging, isLite, neonFruitsService);
            
            Debug.Log($"✅ Identity DI configured with NeonFruits service (endpoint: {gameEndpoint})");
        }
        
        /// <summary>
        /// Альтернативный способ - добавить кастомный сервис после базовой конфигурации
        /// </summary>
        public static void AddNeonFruitsServiceToDI(IdentityServiceContainer container, string gameEndpoint = "neon-fruits")
        {
            // Создаем и регистрируем кастомный сервис как дополнительный
            var neonFruitsService = NeonFruitsGameServiceFactory.CreateService(gameEndpoint);
            container.RegisterSingleton<INeonFruitsGameService>(() => neonFruitsService);
            
            Debug.Log($"✅ NeonFruitsGameService added to DI container");
        }
        
        [ContextMenu("Test Integration")]
        private void TestIntegration()
        {
            var container = new IdentityServiceContainer();
            ConfigureWithNeonFruitsService(container, enableDebugLogging, false, gameEndpoint);
            
            try
            {
                // Проверяем, что можем получить базовый игровой сервис
                var gameService = container.Resolve<Energy8.Identity.Game.Core.Services.IGameService>();
                Debug.Log($"✅ Base IGameService resolved: {gameService.GetType().Name}");
                
                // Проверяем UserFlowManager
                var userFlowManager = container.Resolve<Energy8.Identity.UI.Runtime.Management.Flows.IUserFlowManager>();
                Debug.Log($"✅ IUserFlowManager resolved: {userFlowManager.GetType().Name}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"❌ Integration test failed: {ex.Message}");
            }
        }
    }
}
