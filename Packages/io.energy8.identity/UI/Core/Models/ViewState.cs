using System;

namespace Energy8.Identity.UI.Core.Models
{
    /// <summary>
    /// Состояние представления
    /// </summary>
    public class ViewState
    {
        /// <summary>
        /// Уникальный идентификатор состояния
        /// </summary>
        public Guid Id { get; } = Guid.NewGuid();
        
        /// <summary>
        /// Тип представления
        /// </summary>
        public Type ViewType { get; set; }
        
        /// <summary>
        /// Текущий статус представления
        /// </summary>
        public ViewStatus Status { get; set; } = ViewStatus.Created;
        
        /// <summary>
        /// Видимо ли представление
        /// </summary>
        public bool IsVisible { get; set; }
        
        /// <summary>
        /// Интерактивно ли представление
        /// </summary>
        public bool IsInteractable { get; set; } = true;
        
        /// <summary>
        /// Время создания состояния
        /// </summary>
        public DateTime CreatedAt { get; } = DateTime.UtcNow;
        
        /// <summary>
        /// Время последнего обновления
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Дополнительные данные состояния
        /// </summary>
        public object Data { get; set; }
        
        /// <summary>
        /// Сообщение об ошибке (если есть)
        /// </summary>
        public string ErrorMessage { get; set; }
        
        public ViewState(Type viewType)
        {
            ViewType = viewType ?? throw new ArgumentNullException(nameof(viewType));
        }
        
        /// <summary>
        /// Обновить состояние
        /// </summary>
        public void UpdateStatus(ViewStatus status, string errorMessage = null)
        {
            Status = status;
            ErrorMessage = errorMessage;
            UpdatedAt = DateTime.UtcNow;
        }
        
        /// <summary>
        /// Установить данные
        /// </summary>
        public void SetData(object data)
        {
            Data = data;
            UpdatedAt = DateTime.UtcNow;
        }
    }
    
    /// <summary>
    /// Статусы представления (упрощенная версия для последовательной навигации)
    /// </summary>
    public enum ViewStatus
    {
        /// <summary>
        /// Представление создано
        /// </summary>
        Created,
        
        /// <summary>
        /// Представление готово к показу
        /// </summary>
        Ready,
        
        /// <summary>
        /// Представление показывается (анимация)
        /// </summary>
        Showing,
        
        /// <summary>
        /// Представление активно и видимо
        /// </summary>
        Active,
        
        /// <summary>
        /// Представление скрывается (анимация)
        /// </summary>
        Hiding,
        
        /// <summary>
        /// Представление скрыто
        /// </summary>
        Hidden,
        
        /// <summary>
        /// Ошибка в представлении
        /// </summary>
        Error,
        
        /// <summary>
        /// Представление уничтожено
        /// </summary>
        Destroyed
    }
}
