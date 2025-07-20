using System;

namespace Energy8.Identity.UI.Core.Models
{
    /// <summary>
    /// Модель запроса навигации между представлениями
    /// </summary>
    public class NavigationRequest
    {
        /// <summary>
        /// Тип целевого представления
        /// </summary>
        public Type ViewType { get; set; }
        
        /// <summary>
        /// Параметры для передачи представлению
        /// </summary>
        public object Parameters { get; set; }
        
        /// <summary>
        /// Режим навигации
        /// </summary>
        public NavigationMode Mode { get; set; }
        
        /// <summary>
        /// Должна ли навигация быть анимированной
        /// </summary>
        public bool IsAnimated { get; set; } = true;
        
        /// <summary>
        /// Продолжительность анимации (если применимо)
        /// </summary>
        public float AnimationDuration { get; set; } = 0.5f;
        
        /// <summary>
        /// Дополнительные данные контекста
        /// </summary>
        public object Context { get; set; }
        
        public NavigationRequest(Type viewType, NavigationMode mode = NavigationMode.Show)
        {
            ViewType = viewType ?? throw new ArgumentNullException(nameof(viewType));
            Mode = mode;
        }
        
        public NavigationRequest(Type viewType, object parameters, NavigationMode mode = NavigationMode.Show)
            : this(viewType, mode)
        {
            Parameters = parameters;
        }
    }
    
    /// <summary>
    /// Режимы навигации
    /// </summary>
    public enum NavigationMode
    {
        /// <summary>
        /// Показать новое представление (стандартный режим)
        /// </summary>
        Show,
        
        /// <summary>
        /// Заменить текущее представление без возможности вернуться назад
        /// </summary>
        Replace
    }
}
