namespace Energy8.Identity.UI.Runtime.ViewModels
{
    /// <summary>
    /// ViewModel для ErrorView.
    /// Содержит данные об ошибке и доступных действиях.
    /// </summary>
    public class ErrorViewModel
    {
        /// <summary>
        /// Заголовок ошибки
        /// </summary>
        public string Header { get; }
        
        /// <summary>
        /// Описание ошибки
        /// </summary>
        public string Description { get; }
        
        /// <summary>
        /// Можно ли продолжить работу (показать кнопку Close)
        /// </summary>
        public bool CanProceed { get; }
        
        /// <summary>
        /// Можно ли повторить попытку (показать кнопку Try Again)
        /// </summary>
        public bool CanRetry { get; }
        
        /// <summary>
        /// Необходимо ли выйти из системы (показать кнопку Sign Out)
        /// </summary>
        public bool MustSignOut { get; }
        
        /// <summary>
        /// Показать ли кнопку обращения в поддержку
        /// </summary>
        public bool ShowContact { get; }
        
        /// <summary>
        /// Email службы поддержки
        /// </summary>
        public string SupportEmail { get; }
        
        public ErrorViewModel(
            string header,
            string description,
            bool canProceed = false,
            bool canRetry = false,
            bool mustSignOut = false,
            bool showContact = false,
            string supportEmail = "energy8sup@gmail.com")
        {
            Header = header;
            Description = description;
            CanProceed = canProceed;
            CanRetry = canRetry;
            MustSignOut = mustSignOut;
            ShowContact = showContact;
            SupportEmail = supportEmail;
        }
        
        /// <summary>
        /// Простая ошибка с возможностью закрытия
        /// </summary>
        public static ErrorViewModel SimpleError(string header, string description) =>
            new ErrorViewModel(header, description, canProceed: true);
        
        /// <summary>
        /// Ошибка с возможностью повтора
        /// </summary>
        public static ErrorViewModel RetryableError(string header, string description) =>
            new ErrorViewModel(header, description, canProceed: true, canRetry: true);
        
        /// <summary>
        /// Критическая ошибка с необходимостью выхода
        /// </summary>
        public static ErrorViewModel CriticalError(string header, string description) =>
            new ErrorViewModel(header, description, mustSignOut: true, showContact: true);
        
        /// <summary>
        /// Ошибка сети с возможностью повтора
        /// </summary>
        public static ErrorViewModel NetworkError() =>
            new ErrorViewModel(
                "Connection Error",
                "Unable to connect to the server. Please check your internet connection and try again.",
                canProceed: true, 
                canRetry: true);
    }
}
