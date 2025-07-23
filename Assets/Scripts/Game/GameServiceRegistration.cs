using Game.Factory;
using Game.Services;
using Energy8.Identity.UI.Core;
using Energy8.Identity.Http.Core;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// Регистрация игрового сервиса в DI контейнере
    /// </summary>
    public static class GameServiceRegistration
    {
        /// <summary>
        /// Регистрирует NeonFruitsGameService в DI контейнере
        /// </summary>
        /// <param name="container">DI контейнер</param>
        /// <param name="gameEndpoint">Эндпоинт игрового API</param>
        public static void RegisterGameService(IServiceContainer container, string gameEndpoint = "neon-fruits")
        {
            try
            {
                // Регистрируем сервис в DI контейнере с фабрикой
                container.RegisterSingleton<INeonFruitsGameService>(() => 
                {
                    var httpClient = container.Resolve<IHttpClient>();
                    return NeonFruitsGameServiceFactory.CreateService(httpClient, gameEndpoint);
                });
                
                Debug.Log($"[GameServiceRegistration] NeonFruitsGameService registered with endpoint: {gameEndpoint}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[GameServiceRegistration] Failed to register game service: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// Получает зарегистрированный игровой сервис из DI контейнера
        /// </summary>
        /// <param name="container">DI контейнер</param>
        /// <returns>Экземпляр INeonFruitsGameService</returns>
        public static INeonFruitsGameService GetGameService(IServiceContainer container)
        {
            try
            {
                return container.Resolve<INeonFruitsGameService>();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[GameServiceRegistration] Failed to resolve game service: {ex.Message}");
                throw;
            }
        }
    }
}