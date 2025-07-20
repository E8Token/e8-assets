namespace Energy8.Identity.UI.Runtime.ViewModels
{
    /// <summary>
    /// ViewModel для LoadingView - содержит данные о состоянии загрузки
    /// </summary>
    public class LoadingViewModel
    {
        /// <summary>
        /// Текст для отображения во время загрузки
        /// </summary>
        public string LoadingText { get; set; } = "Loading...";
        
        /// <summary>
        /// Текст ошибки (если есть)
        /// </summary>
        public string ErrorText { get; set; }
        
        /// <summary>
        /// Показывать ли спиннер загрузки
        /// </summary>
        public bool ShowSpinner { get; set; } = true;
        
        /// <summary>
        /// Есть ли ошибка
        /// </summary>
        public bool HasError => !string.IsNullOrEmpty(ErrorText);
        
        /// <summary>
        /// Создать ViewModel по умолчанию
        /// </summary>
        public static LoadingViewModel Default => new LoadingViewModel();
        
        /// <summary>
        /// Создать ViewModel с кастомным текстом
        /// </summary>
        public static LoadingViewModel WithText(string loadingText)
        {
            return new LoadingViewModel
            {
                LoadingText = loadingText
            };
        }
        
        /// <summary>
        /// Создать ViewModel с ошибкой
        /// </summary>
        public static LoadingViewModel WithError(string errorText)
        {
            return new LoadingViewModel
            {
                ErrorText = errorText,
                ShowSpinner = false
            };
        }
    }
}
