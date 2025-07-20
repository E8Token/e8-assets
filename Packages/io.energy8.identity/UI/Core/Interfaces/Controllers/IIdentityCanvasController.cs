using System;

namespace Energy8.Identity.UI.Core.Interfaces.Controllers
{
    /// <summary>
    /// Интерфейс для Legacy IdentityCanvasController.
    /// Позволяет WindowService работать с Canvas без жесткой зависимости.
    /// </summary>
    public interface IIdentityCanvasController
    {
        /// <summary>
        /// Открыто ли окно Canvas
        /// </summary>
        bool IsOpen { get; }
        
        /// <summary>
        /// Установить состояние открытия/закрытия
        /// </summary>
        /// <param name="isOpen">Открыть или закрыть</param>
        void SetOpenState(bool isOpen);
        
        /// <summary>
        /// Получить ViewManager
        /// </summary>
        /// <returns>ViewManager для управления Views</returns>
        object GetViewManager();
        
        /// <summary>
        /// Событие изменения состояния открытия/закрытия
        /// </summary>
        event Action<bool> OnOpenStateChanged;
    }
}
