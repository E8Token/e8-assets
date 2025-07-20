namespace Energy8.Identity.UI.Runtime.State
{
    /// <summary>
    /// Состояния Identity системы для замены разбросанных булевых флагов
    /// </summary>
    public enum IdentityState
    {
        /// <summary>
        /// Система не инициализирована
        /// </summary>
        Uninitialized,
        
        /// <summary>
        /// Происходит инициализация системы
        /// </summary>
        Initializing,
        
        /// <summary>
        /// Пользователь не авторизован
        /// </summary>
        SignedOut,
        
        /// <summary>
        /// Идет процесс авторизации
        /// </summary>
        AuthenticationInProgress,
        
        /// <summary>
        /// Пользователь авторизован
        /// </summary>
        SignedIn,
        
        /// <summary>
        /// Активен пользовательский поток
        /// </summary>
        UserFlowActive,
        
        /// <summary>
        /// Открыты настройки пользователя
        /// </summary>
        SettingsOpen,
        
        /// <summary>
        /// Произошла ошибка в системе
        /// </summary>
        Error
    }
}
