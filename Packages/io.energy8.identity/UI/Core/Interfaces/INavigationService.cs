using System;
using System.Threading.Tasks;
using Energy8.Identity.UI.Core.Models;

namespace Energy8.Identity.UI.Core.Interfaces
{
    /// <summary>
    /// Сервис навигации для простого последовательного показа представлений в одном окне.
    /// Управляет открытием/закрытием окна и переключением между Views.
    /// </summary>
    public interface INavigationService
    {
        /// <summary>
        /// Показать представление в окне
        /// </summary>
        /// <typeparam name="TView">Тип представления</typeparam>
        Task ShowAsync<TView>() where TView : IView;
        
        /// <summary>
        /// Показать представление с параметрами
        /// </summary>
        /// <typeparam name="TView">Тип представления</typeparam>
        /// <typeparam name="TParam">Тип параметров</typeparam>
        /// <param name="parameter">Параметры для передачи</param>
        Task ShowAsync<TView, TParam>(TParam parameter) where TView : IView;
        
        /// <summary>
        /// Показать представление по типу (для динамической навигации)
        /// </summary>
        /// <param name="viewType">Тип представления</param>
        /// <param name="parameter">Параметры (опционально)</param>
        Task ShowAsync(Type viewType, object parameter = null);
        
        /// <summary>
        /// Заменить текущее представление без возможности вернуться назад
        /// </summary>
        /// <typeparam name="TView">Тип представления</typeparam>
        Task ReplaceAsync<TView>() where TView : IView;
        
        /// <summary>
        /// Заменить текущее представление с параметрами
        /// </summary>
        /// <typeparam name="TView">Тип представления</typeparam>
        /// <typeparam name="TParam">Тип параметров</typeparam>
        /// <param name="parameter">Параметры</param>
        Task ReplaceAsync<TView, TParam>(TParam parameter) where TView : IView;
        
        /// <summary>
        /// Закрыть текущее окно (скрыть все Views)
        /// </summary>
        Task CloseWindowAsync();
        
        /// <summary>
        /// Открыто ли окно
        /// </summary>
        bool IsWindowOpen { get; }
        
        /// <summary>
        /// Текущее активное представление
        /// </summary>
        IView CurrentView { get; }
        
        /// <summary>
        /// Событие смены текущего представления
        /// </summary>
        event Action<IView, IView> OnViewChanged; // previousView, currentView
        
        /// <summary>
        /// Событие открытия/закрытия окна
        /// </summary>
        event Action<bool> OnWindowStateChanged; // isOpen
    }
}
