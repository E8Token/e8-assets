using System.Threading.Tasks;

namespace Energy8.Identity.UI.Core.Interfaces.Services
{
    /// <summary>
    /// Интерфейс сервиса навигации между Views.
    /// Ответственность: ТОЛЬКО навигация и показ Views.
    /// НЕ содержит бизнес-логику, НЕ управляет окнами!
    /// </summary>
    public interface INavigationService
    {
        /// <summary>
        /// Показать View указанного типа
        /// </summary>
        /// <typeparam name="TView">Тип View для показа</typeparam>
        Task ShowAsync<TView>() where TView : class;
        
        /// <summary>
        /// Показать View с параметрами
        /// </summary>
        /// <typeparam name="TView">Тип View</typeparam>
        /// <typeparam name="TParams">Тип параметров</typeparam>
        /// <param name="parameters">Параметры для View</param>
        Task ShowAsync<TView, TParams>(TParams parameters) 
            where TView : class;
        
        /// <summary>
        /// Показать View с параметрами и получить результат
        /// </summary>
        /// <typeparam name="TView">Тип View</typeparam>
        /// <typeparam name="TParams">Тип параметров</typeparam>
        /// <typeparam name="TResult">Тип результата</typeparam>
        /// <param name="parameters">Параметры для View</param>
        /// <returns>Результат работы View</returns>
        Task<TResult> ShowAsync<TView, TParams, TResult>(TParams parameters)
            where TView : class;
        
        /// <summary>
        /// Заменить текущий View на новый
        /// </summary>
        /// <typeparam name="TView">Тип нового View</typeparam>
        Task ReplaceAsync<TView>() where TView : class;
        
        /// <summary>
        /// Вернуться к предыдущему View
        /// </summary>
        Task GoBackAsync();
        
        /// <summary>
        /// Можно ли вернуться назад
        /// </summary>
        bool CanGoBack { get; }
        
        /// <summary>
        /// Закрыть все Views
        /// </summary>
        Task CloseAllAsync();
    }
}
