using System.Threading.Tasks;

namespace Energy8.Identity.UI.Core.Interfaces.Controllers
{
    /// <summary>
    /// Интерфейс главного UI координатора.
    /// Ответственность: ТОЛЬКО координация и инициализация системы.
    /// НЕ содержит бизнес-логику!
    /// </summary>
    public interface IUICoordinator
    {
        /// <summary>
        /// Инициализирована ли система
        /// </summary>
        bool IsInitialized { get; }
        
        /// <summary>
        /// Инициализировать UI систему
        /// </summary>
        Task InitializeAsync();
        
        /// <summary>
        /// Получить сервис из DI контейнера
        /// </summary>
        T GetService<T>() where T : class;
        
        /// <summary>
        /// Проверить, доступен ли сервис
        /// </summary>
        bool HasService<T>() where T : class;
    }
}
