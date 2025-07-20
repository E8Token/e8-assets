namespace Energy8.Identity.UI.Runtime.ViewModels
{
    /// <summary>
    /// ViewModel для SignInView.
    /// Содержит данные для предзаполнения или конфигурации UI.
    /// </summary>
    public class SignInViewModel
    {
        /// <summary>
        /// Предзаполненный email (опционально)
        /// </summary>
        public string PrefilledEmail { get; }
        
        /// <summary>
        /// Показать ли кнопку Google
        /// </summary>
        public bool ShowGoogleButton { get; }
        
        /// <summary>
        /// Показать ли кнопку Apple
        /// </summary>
        public bool ShowAppleButton { get; }
        
        /// <summary>
        /// Показать ли кнопку Telegram
        /// </summary>
        public bool ShowTelegramButton { get; }
        
        public SignInViewModel(
            string prefilledEmail = null,
            bool showGoogleButton = true,
            bool showAppleButton = true, 
            bool showTelegramButton = true)
        {
            PrefilledEmail = prefilledEmail;
            ShowGoogleButton = showGoogleButton;
            ShowAppleButton = showAppleButton;
            ShowTelegramButton = showTelegramButton;
        }
        
        /// <summary>
        /// Пустая ViewModel по умолчанию (как Legacy)
        /// </summary>
        public static SignInViewModel Default => new SignInViewModel();
    }
}
