using Game.Factory;
using Game.Services;
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
        /// ВАЖНО: Требуется внешняя реализация IServiceContainer
        /// </summary>
        /// <param name="container">DI контейнер</param>
        /// <param name="gameEndpoint">Эндпоинт игрового API</param>
        public static void RegisterGameService(object container, string gameEndpoint = "neon-fruits")
        {
            Debug.Log($"[GameServiceRegistration] Manual registration required. Endpoint: {gameEndpoint}");
            // TODO: Реализовать регистрацию когда DI контейнер будет доступен
        }
        
        /// <summary>
        /// Создает экземпляр игрового сервиса напрямую
        /// </summary>
        /// <param name="gameEndpoint">Эндпоинт игрового API</param>
        /// <returns>Экземпляр INeonFruitsGameService</returns>
        public static INeonFruitsGameService CreateGameService(string gameEndpoint = "neon-fruits")
        {
            try
            {
                return NeonFruitsGameServiceFactory.CreateService(gameEndpoint);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[GameServiceRegistration] Failed to create game service: {ex.Message}");
                throw;
            }
        }
    }
}
