using System;
using Energy8.Identity.UI.Core.Controllers;


namespace Energy8.Identity.UI.Core.Management
{
    /// <summary>
    /// Интерфейс для управления Canvas и UI состоянием
    /// </summary>
    public interface ICanvasManager
    {
        /// <summary>
        /// Состояние открытия/закрытия UI
        /// </summary>
        bool IsOpen { get; }
        
        /// <summary>
        /// Событие изменения состояния открытия UI
        /// </summary>
        event Action<bool> OnOpenStateChanged;
        
        /// <summary>
        /// Устанавливает Canvas контроллер
        /// </summary>
        void SetCanvasController(IIdentityCanvasController canvasController);
        
        /// <summary>
        /// Переключает состояние открытия/закрытия UI
        /// </summary>
        void ToggleOpenState();
        
        /// <summary>
        /// Устанавливает состояние открытия/закрытия UI
        /// </summary>
        void SetOpenState(bool isOpen);
        
        /// <summary>
        /// Получает ViewManager из текущего Canvas контроллера
        /// </summary>
        IViewManager GetViewManager();
    }
}
