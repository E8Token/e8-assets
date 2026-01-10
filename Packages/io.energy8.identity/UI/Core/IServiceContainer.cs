
using System;

namespace Energy8.Identity.UI.Core
{
    /// <summary>
    /// Интерфейс для Dependency Injection контейнера
    /// </summary>
    public interface IServiceContainer
    {
        /// <summary>
        /// Регистрация singleton сервиса
        /// </summary>
        void RegisterSingleton<TInterface, TImplementation>() 
            where TImplementation : class, TInterface 
            where TInterface : class;
            
        /// <summary>
        /// Регистрация singleton с фабрикой
        /// </summary>
        void RegisterSingleton<T>(Func<T> factory) where T : class;
        
        /// <summary>
        /// Разрешение зависимости
        /// </summary>
        T Resolve<T>() where T : class;
        
        /// <summary>
        /// Конфигурация всех сервисов
        /// </summary>
        void ConfigureServices(bool isLite);
    }
}
