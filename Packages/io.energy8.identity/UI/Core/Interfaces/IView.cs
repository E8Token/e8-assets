using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Energy8.Identity.UI.Core.Interfaces
{
    /// <summary>
    /// Базовый интерфейс для всех UI представлений.
    /// View отвечает ТОЛЬКО за отображение данных и получение пользовательского ввода.
    /// НЕ содержит бизнес-логику!
    /// </summary>
    public interface IView
    {
        /// <summary>
        /// Показать представление
        /// </summary>
        Task ShowAsync();
        
        /// <summary>
        /// Скрыть представление
        /// </summary>
        Task HideAsync();
        
        /// <summary>
        /// Состояние видимости представления
        /// </summary>
        bool IsVisible { get; }
        
        /// <summary>
        /// Состояние интерактивности представления
        /// </summary>
        bool IsInteractable { get; set; }
        
        /// <summary>
        /// GameObject представления
        /// </summary>
        GameObject GameObject { get; }
        
        /// <summary>
        /// RectTransform для управления позицией и размером
        /// </summary>
        RectTransform RectTransform { get; }
    }
    
    /// <summary>
    /// Типизированный интерфейс View с ViewModel
    /// </summary>
    /// <typeparam name="TViewModel">Тип модели данных для отображения</typeparam>
    public interface IView<in TViewModel> : IView
    {
        /// <summary>
        /// Установить модель данных для отображения
        /// </summary>
        /// <param name="viewModel">Модель данных</param>
        void SetViewModel(TViewModel viewModel);
    }
}
