using System;
using System.Threading.Tasks;

namespace Energy8.Identity.UI.Core.Interfaces
{
    /// <summary>
    /// Фабрика для создания представлений и презентеров.
    /// Инкапсулирует логику создания и связывания View с Presenter.
    /// </summary>
    public interface IViewFactory
    {
        /// <summary>
        /// Создать представление указанного типа
        /// </summary>
        /// <typeparam name="TView">Тип представления</typeparam>
        /// <returns>Экземпляр представления</returns>
        TView CreateView<TView>() where TView : IView;
        
        /// <summary>
        /// Создать представление по типу (для динамического создания)
        /// </summary>
        /// <param name="viewType">Тип представления</param>
        /// <returns>Экземпляр представления</returns>
        IView CreateView(Type viewType);
        
        /// <summary>
        /// Создать презентер для указанного представления
        /// </summary>
        /// <typeparam name="TPresenter">Тип презентера</typeparam>
        /// <typeparam name="TView">Тип представления</typeparam>
        /// <param name="view">Представление</param>
        /// <returns>Экземпляр презентера</returns>
        TPresenter CreatePresenter<TPresenter, TView>(TView view) 
            where TPresenter : IPresenter<TView> 
            where TView : IView;
        
        /// <summary>
        /// Создать полную связку View + Presenter
        /// </summary>
        /// <typeparam name="TPresenter">Тип презентера</typeparam>
        /// <typeparam name="TView">Тип представления</typeparam>
        /// <returns>Презентер с уже созданным и связанным представлением</returns>
        TPresenter CreateViewPresenterPair<TPresenter, TView>() 
            where TPresenter : IPresenter<TView> 
            where TView : IView;
        
        /// <summary>
        /// Уничтожить представление и освободить ресурсы
        /// </summary>
        /// <param name="view">Представление для уничтожения</param>
        Task DestroyViewAsync(IView view);
        
        /// <summary>
        /// Проверить, зарегистрирован ли тип представления
        /// </summary>
        /// <param name="viewType">Тип представления</param>
        /// <returns>True если зарегистрирован</returns>
        bool IsViewRegistered(Type viewType);
    }
}
