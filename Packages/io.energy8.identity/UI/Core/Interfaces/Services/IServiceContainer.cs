using System;

namespace Energy8.Identity.UI.Core.Interfaces.Services
{
    /// <summary>
    /// Интерфейс DI контейнера для управления зависимостями.
    /// Ответственность: ТОЛЬКО регистрация и разрешение зависимостей.
    /// Заменяет Service Locator anti-pattern на правильный Dependency Injection!
    /// </summary>
    public interface IServiceContainer
    {
        /// <summary>
        /// Зарегистрировать сервис как Transient (новый экземпляр каждый раз)
        /// </summary>
        /// <typeparam name="TInterface">Интерфейс сервиса</typeparam>
        /// <typeparam name="TImplementation">Реализация сервиса</typeparam>
        void Register<TInterface, TImplementation>() 
            where TImplementation : class, TInterface
            where TInterface : class;
        
        /// <summary>
        /// Зарегистрировать сервис как Singleton (один экземпляр на всю жизнь контейнера)
        /// </summary>
        /// <typeparam name="TInterface">Интерфейс сервиса</typeparam>
        /// <typeparam name="TImplementation">Реализация сервиса</typeparam>
        void RegisterSingleton<TInterface, TImplementation>() 
            where TImplementation : class, TInterface
            where TInterface : class;
        
        /// <summary>
        /// Зарегистрировать уже существующий экземпляр как Singleton
        /// </summary>
        /// <typeparam name="TInterface">Интерфейс сервиса</typeparam>
        /// <param name="instance">Экземпляр для регистрации</param>
        void RegisterInstance<TInterface>(TInterface instance) 
            where TInterface : class;
        
        /// <summary>
        /// Зарегистрировать сервис с помощью фабрики
        /// </summary>
        /// <typeparam name="TInterface">Интерфейс сервиса</typeparam>
        /// <param name="factory">Функция создания экземпляра</param>
        void RegisterFactory<TInterface>(Func<IServiceContainer, TInterface> factory)
            where TInterface : class;
        
        /// <summary>
        /// Разрешить зависимость (получить экземпляр сервиса)
        /// </summary>
        /// <typeparam name="T">Тип сервиса</typeparam>
        /// <returns>Экземпляр сервиса</returns>
        T Resolve<T>() where T : class;
        
        /// <summary>
        /// Попытаться разрешить зависимость
        /// </summary>
        /// <typeparam name="T">Тип сервиса</typeparam>
        /// <param name="service">Экземпляр сервиса (если найден)</param>
        /// <returns>true если сервис найден, false если нет</returns>
        bool TryResolve<T>(out T service) where T : class;
        
        /// <summary>
        /// Проверить, зарегистрирован ли сервис
        /// </summary>
        /// <typeparam name="T">Тип сервиса</typeparam>
        /// <returns>true если зарегистрирован</returns>
        bool IsRegistered<T>() where T : class;
        
        /// <summary>
        /// Очистить все регистрации
        /// </summary>
        void Clear();
    }
}
