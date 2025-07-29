using Energy8.Identity.Configuration.Core;
using Energy8.Identity.Http.Core;
using Game.Services;
using Energy8.Identity.Http.Runtime.Clients;
using UnityEngine;

namespace Game.Factory
{
    /// <summary>
    /// Фабрика для создания экземпляров NeonFruitsGameService
    /// </summary>
    public static class NeonFruitsGameServiceFactory
    {
        /// <summary>
        /// Создает экземпляр NeonFruitsGameService с переданным HttpClient
        /// </summary>
        /// <param name="httpClient">HTTP клиент для API вызовов</param>
        /// <param name="gameEndpoint">Эндпоинт игрового API</param>
        /// <returns>Экземпляр INeonFruitsGameService</returns>
        public static INeonFruitsGameService CreateService(IHttpClient httpClient, string gameEndpoint = "neon-fruits")
        {
            try
            {
                if (httpClient == null)
                    throw new System.ArgumentNullException(nameof(httpClient));
                    
                return new NeonFruitsGameService(httpClient, gameEndpoint);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[NeonFruitsGameServiceFactory] Failed to create service: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// Создает экземпляр NeonFruitsGameService с новым HttpClient (для обратной совместимости)
        /// </summary>
        /// <param name="gameEndpoint">Эндпоинт игрового API</param>
        /// <returns>Экземпляр INeonFruitsGameService</returns>
        public static INeonFruitsGameService CreateService(string gameEndpoint = "neon-fruits")
        {
            try
            {
                var httpClient = new UnityHttpClient(IdentityConfiguration.SelectedIP);
                return CreateService(httpClient, gameEndpoint);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[NeonFruitsGameServiceFactory] Failed to create service: {ex.Message}");
                throw;
            }
        }
    }
}
