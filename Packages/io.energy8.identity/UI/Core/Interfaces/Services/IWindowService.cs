using System;
using System.Threading.Tasks;

namespace Energy8.Identity.UI.Core.Interfaces.Services
{
    /// <summary>
    /// Интерфейс сервиса управления окном.
    /// Ответственность: ТОЛЬКО управление открытием/закрытием UI окна.
    /// НЕ содержит бизнес-логику, НЕ управляет навигацией!
    /// </summary>
    public interface IWindowService : IDisposable
    {
        /// <summary>
        /// Открыто ли окно в данный момент
        /// </summary>
        bool IsOpen { get; }
        
        /// <summary>
        /// Открыть UI окно
        /// </summary>
        Task OpenAsync();
        
        /// <summary>
        /// Закрыть UI окно
        /// </summary>
        Task CloseAsync();
        
        /// <summary>
        /// Переключить состояние окна (открыть/закрыть)
        /// </summary>
        Task ToggleAsync();
        
        /// <summary>
        /// Установить Canvas контроллер для управления
        /// </summary>
        /// <param name="canvasController">Canvas контроллер</param>
        void SetCanvasController(object canvasController);
    }
}
