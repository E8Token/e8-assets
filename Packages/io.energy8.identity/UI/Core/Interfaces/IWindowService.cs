using System;
using System.Threading.Tasks;

namespace Energy8.Identity.UI.Core.Interfaces
{
    /// <summary>
    /// Сервис управления UI окном.
    /// Отвечает только за открытие/закрытие окна, не за навигацию между Views.
    /// </summary>
    public interface IWindowService
    {
        /// <summary>
        /// Открыть UI окно
        /// </summary>
        Task OpenAsync();
        
        /// <summary>
        /// Закрыть UI окно
        /// </summary>
        Task CloseAsync();
        
        /// <summary>
        /// Переключить состояние окна (открыто/закрыто)
        /// </summary>
        Task ToggleAsync();
        
        /// <summary>
        /// Установить состояние окна
        /// </summary>
        /// <param name="isOpen">Должно ли быть окно открыто</param>
        Task SetStateAsync(bool isOpen);
        
        /// <summary>
        /// Открыто ли окно
        /// </summary>
        bool IsOpen { get; }
        
        /// <summary>
        /// Событие изменения состояния окна
        /// </summary>
        event Action<bool> OnStateChanged; // isOpen
    }
}
