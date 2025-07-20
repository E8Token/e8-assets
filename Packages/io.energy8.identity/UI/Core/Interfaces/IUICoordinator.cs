using System;
using System.Threading.Tasks;

namespace Energy8.Identity.UI.Core.Interfaces
{
    /// <summary>
    /// Упрощенный координатор UI системы.
    /// Заменяет монолитный IdentityUIController и содержит ТОЛЬКО координационную логику.
    /// Управляет простой последовательной навигацией в одном окне.
    /// Целевой размер: < 100 строк кода.
    /// </summary>
    public interface IUICoordinator
    {
        /// <summary>
        /// Инициализировать UI систему
        /// </summary>
        Task InitializeAsync();
        
        /// <summary>
        /// Получить сервис по типу
        /// </summary>
        /// <typeparam name="T">Тип сервиса</typeparam>
        /// <returns>Экземпляр сервиса</returns>
        T GetService<T>() where T : class;
        
        /// <summary>
        /// Показать представление (открывает окно если нужно)
        /// </summary>
        /// <typeparam name="TView">Тип представления</typeparam>
        Task ShowViewAsync<TView>() where TView : IView;
        
        /// <summary>
        /// Показать представление с параметрами
        /// </summary>
        /// <typeparam name="TView">Тип представления</typeparam>
        /// <typeparam name="TParam">Тип параметров</typeparam>
        /// <param name="parameter">Параметры</param>
        Task ShowViewAsync<TView, TParam>(TParam parameter) where TView : IView;
        
        /// <summary>
        /// Закрыть UI окно
        /// </summary>
        Task CloseAsync();
        
        /// <summary>
        /// Переключить состояние UI окна
        /// </summary>
        Task ToggleAsync();
        
        /// <summary>
        /// Открыто ли UI окно
        /// </summary>
        bool IsOpen { get; }
        
        /// <summary>
        /// Текущее активное представление
        /// </summary>
        IView CurrentView { get; }
        
        /// <summary>
        /// Событие изменения состояния окна
        /// </summary>
        event Action<bool> OnStateChanged;
        
        /// <summary>
        /// Событие смены представления
        /// </summary>
        event Action<IView, IView> OnViewChanged; // previousView, currentView
    }
}
