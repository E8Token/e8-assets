using Energy8.Identity.Http.Core;
using Energy8.Identity.Http.Runtime.Factory;
using Energy8.Identity.Configuration.Core;
using Game.Services;
using UnityEngine;

namespace Game.Factory
{
    /// <summary>
    /// Фабрика для создания экземпляров NeonFruitsGameService
    /// </summary>
    public static class NeonFruitsGameServiceFactory
    {
        /// <summary>
        /// Создает NeonFruitsGameService с дефолтными настройками
        /// </summary>
        /// <param name="gameEndpoint">Эндпоинт игрового API (по умолчанию "neon-fruits")</param>
        /// <returns>Экземпляр INeonFruitsGameService</returns>
        public static INeonFruitsGameService CreateService(string gameEndpoint = "neon-fruits")
        {
            try
            {
                if (IdentityConfiguration.EnableDebugLogging)
                {
                    Debug.Log($"[NeonFruitsGameServiceFactory] Creating service with endpoint: {gameEndpoint}");
                }

                var httpClient = HttpClientFactory.CreateDefaultClient();
                return new NeonFruitsGameService(httpClient, gameEndpoint);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[NeonFruitsGameServiceFactory] Failed to create service: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Создает NeonFruitsGameService с кастомным HttpClient
        /// </summary>
        /// <param name="httpClient">Кастомный HTTP клиент</param>
        /// <param name="gameEndpoint">Эндпоинт игрового API (по умолчанию "neon-fruits")</param>
        /// <returns>Экземпляр INeonFruitsGameService</returns>
        public static INeonFruitsGameService CreateService(IHttpClient httpClient, string gameEndpoint = "neon-fruits")
        {
            if (httpClient == null)
            {
                if (IdentityConfiguration.EnableDebugLogging)
                {
                    Debug.LogError("[NeonFruitsGameServiceFactory] HttpClient is null");
                }
                throw new System.ArgumentNullException(nameof(httpClient));
            }

            if (string.IsNullOrWhiteSpace(gameEndpoint))
            {
                if (IdentityConfiguration.EnableDebugLogging)
                {
                    Debug.LogWarning("[NeonFruitsGameServiceFactory] GameEndpoint is null or empty, using default 'neon-fruits'");
                }
                gameEndpoint = "neon-fruits";
            }

            if (IdentityConfiguration.EnableDebugLogging)
            {
                Debug.Log($"[NeonFruitsGameServiceFactory] Creating service with custom HttpClient and endpoint: {gameEndpoint}");
            }

            return new NeonFruitsGameService(httpClient, gameEndpoint);
        }

        /// <summary>
        /// Создает тестовый сервис для разработки
        /// </summary>
        /// <param name="gameEndpoint">Тестовый эндпоинт (по умолчанию "test-neon-fruits")</param>
        /// <returns>Экземпляр INeonFruitsGameService для тестирования</returns>
        public static INeonFruitsGameService CreateTestService(string gameEndpoint = "test-neon-fruits")
        {
            try
            {
                if (IdentityConfiguration.EnableDebugLogging)
                {
                    Debug.Log($"[NeonFruitsGameServiceFactory] Creating TEST service with endpoint: {gameEndpoint}");
                }

                // Для тестового сервиса можно использовать специальный тестовый HttpClient
                var httpClient = HttpClientFactory.CreateTestClient();
                return new NeonFruitsGameService(httpClient, gameEndpoint);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[NeonFruitsGameServiceFactory] Failed to create test service: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Создает сервис с полной кастомной конфигурацией
        /// </summary>
        /// <param name="httpClient">HTTP клиент</param>
        /// <param name="gameEndpoint">Игровой эндпоинт</param>
        /// <param name="enableLogging">Включить логирование</param>
        /// <returns>Сконфигурированный сервис</returns>
        public static INeonFruitsGameService CreateCustomService(
            IHttpClient httpClient, 
            string gameEndpoint, 
            bool enableLogging = true)
        {
            if (httpClient == null)
                throw new System.ArgumentNullException(nameof(httpClient));

            if (string.IsNullOrWhiteSpace(gameEndpoint))
                throw new System.ArgumentException("Game endpoint cannot be null or empty", nameof(gameEndpoint));

            if (enableLogging)
            {
                Debug.Log($"[NeonFruitsGameServiceFactory] Creating custom service with endpoint: {gameEndpoint}");
            }

            return new NeonFruitsGameService(httpClient, gameEndpoint);
        }
    }
}
