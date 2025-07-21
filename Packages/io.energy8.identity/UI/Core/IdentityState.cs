namespace Energy8.Identity.UI.Core
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
        /// Предавторизационный этап (update, analytics, webgl и т.д.)
        /// </summary>
        PreAuthentication,

        /// <summary>
        /// Проверка авторизации (есть ли токен/сессия)
        /// </summary>
        AuthCheck,

        /// <summary>
        /// Пользователь не авторизован
        /// </summary>
        SignedOut,

        /// <summary>
        /// Пользователь авторизован
        /// </summary>
        SignedIn,

        /// <summary>
        /// Активен пользовательский поток
        /// </summary>
        UserFlowActive,

        /// <summary>
        /// Активен поток авторизации
        /// </summary>
        AuthFlowActive,

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
