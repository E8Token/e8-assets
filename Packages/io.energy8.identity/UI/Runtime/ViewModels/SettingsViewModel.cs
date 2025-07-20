namespace Energy8.Identity.UI.Runtime.ViewModels
{
    /// <summary>
    /// ViewModel для SettingsView.
    /// Содержит данные пользователя и информацию о провайдерах.
    /// </summary>
    public class SettingsViewModel
    {
        /// <summary>
        /// Имя пользователя для отображения
        /// </summary>
        public string UserName { get; }
        
        /// <summary>
        /// Email пользователя для отображения
        /// </summary>
        public string UserEmail { get; }
        
        /// <summary>
        /// Есть ли подключенный Google провайдер
        /// </summary>
        public bool HasGoogleProvider { get; }
        
        /// <summary>
        /// Есть ли подключенный Apple провайдер
        /// </summary>
        public bool HasAppleProvider { get; }
        
        /// <summary>
        /// Есть ли подключенный Telegram провайдер
        /// </summary>
        public bool HasTelegramProvider { get; }
        
        /// <summary>
        /// Заголовок страницы настроек
        /// </summary>
        public string Title { get; }
        
        public SettingsViewModel(
            string userName,
            string userEmail,
            bool hasGoogleProvider = false,
            bool hasAppleProvider = false,
            bool hasTelegramProvider = false,
            string title = "Settings")
        {
            UserName = userName;
            UserEmail = userEmail;
            HasGoogleProvider = hasGoogleProvider;
            HasAppleProvider = hasAppleProvider;
            HasTelegramProvider = hasTelegramProvider;
            Title = title;
        }
        
        /// <summary>
        /// Пустая ViewModel по умолчанию
        /// </summary>
        public static SettingsViewModel Default => new SettingsViewModel("User", "user@example.com");
        
        /// <summary>
        /// ViewModel с пользовательскими данными
        /// </summary>
        public static SettingsViewModel WithUserData(
            string userName, 
            string userEmail,
            bool hasGoogle = false,
            bool hasApple = false, 
            bool hasTelegram = false) =>
            new SettingsViewModel(userName, userEmail, hasGoogle, hasApple, hasTelegram);
    }
}
