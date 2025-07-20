using System;
using System.Threading.Tasks;

namespace Energy8.Identity.UI.Core.Interfaces
{
    /// <summary>
    /// Базовый интерфейс для всех презентеров.
    /// Presenter содержит ВСЮ бизнес-логику и управляет View.
    /// </summary>
    public interface IPresenter
    {
        /// <summary>
        /// Инициализировать презентер
        /// </summary>
        Task InitializeAsync();
        
        /// <summary>
        /// Показать представление
        /// </summary>
        Task ShowAsync();
        
        /// <summary>
        /// Скрыть представление
        /// </summary>
        Task HideAsync();
        
        /// <summary>
        /// Освободить ресурсы
        /// </summary>
        Task DisposeAsync();
        
        /// <summary>
        /// Состояние активности презентера
        /// </summary>
        bool IsActive { get; }
    }
    
    /// <summary>
    /// Типизированный интерфейс презентера с конкретной View
    /// </summary>
    /// <typeparam name="TView">Тип представления</typeparam>
    public interface IPresenter<out TView> : IPresenter 
        where TView : IView
    {
        /// <summary>
        /// Управляемое представление
        /// </summary>
        TView View { get; }
    }
}
