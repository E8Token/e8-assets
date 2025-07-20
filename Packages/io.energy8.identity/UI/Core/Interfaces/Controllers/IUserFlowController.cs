using System;
using System.Threading.Tasks;

namespace Energy8.Identity.UI.Core.Interfaces.Controllers
{
    /// <summary>
    /// Интерфейс контроллера пользовательского потока.
    /// Ответственность: ТОЛЬКО логика управления пользователем и настройками.
    /// НЕ управляет UI, НЕ управляет окнами - только бизнес-логика пользователя!
    /// </summary>
    public interface IUserFlowController : IDisposable
    {
        /// <summary>
        /// Запустить пользовательский поток (показ профиля)
        /// </summary>
        Task StartAsync();
        
        /// <summary>
        /// Показать настройки пользователя
        /// </summary>
        Task ShowSettingsAsync();
        
        /// <summary>
        /// Обработать изменение имени пользователя
        /// </summary>
        /// <param name="newName">Новое имя</param>
        Task HandleNameChangeAsync(string newName);
        
        /// <summary>
        /// Обработать изменение email пользователя
        /// </summary>
        /// <param name="newEmail">Новый email</param>
        Task HandleEmailChangeAsync(string newEmail);
        
        /// <summary>
        /// Обработать удаление аккаунта
        /// </summary>
        Task HandleAccountDeletionAsync();
        
        /// <summary>
        /// Обработать добавление Google провайдера
        /// </summary>
        Task HandleAddGoogleProviderAsync();
        
        /// <summary>
        /// Обработать добавление Apple провайдера
        /// </summary>
        Task HandleAddAppleProviderAsync();
        
        /// <summary>
        /// Обработать добавление Telegram провайдера
        /// </summary>
        Task HandleAddTelegramProviderAsync();
        
        /// <summary>
        /// Обработать выход из системы
        /// </summary>
        Task HandleSignOutAsync();
        
        /// <summary>
        /// Остановить пользовательский поток
        /// </summary>
        Task StopAsync();
        
        #region Events
        
        /// <summary>
        /// Событие выхода пользователя
        /// </summary>
        event Action OnUserSignedOut;
        
        /// <summary>
        /// Событие удаления аккаунта
        /// </summary>
        event Action OnAccountDeleted;
        
        /// <summary>
        /// Событие ошибки пользовательского потока
        /// </summary>
        event Action<string> OnUserFlowError;
        
        #endregion
    }
}
