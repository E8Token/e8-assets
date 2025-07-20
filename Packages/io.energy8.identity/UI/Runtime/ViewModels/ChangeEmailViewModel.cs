namespace Energy8.Identity.UI.Runtime.ViewModels
{
    /// <summary>
    /// ViewModel для ChangeEmailView.
    /// Содержит данные для отображения текущего и нового email.
    /// </summary>
    public class ChangeEmailViewModel
    {
        /// <summary>
        /// Текущий email пользователя (для отображения)
        /// </summary>
        public string CurrentEmail { get; }
        
        /// <summary>
        /// Предзаполненный новый email (опционально)
        /// </summary>
        public string PrefilledNewEmail { get; }
        
        /// <summary>
        /// Инструкция для пользователя
        /// </summary>
        public string Instructions { get; }
        
        public ChangeEmailViewModel(
            string currentEmail = null,
            string prefilledNewEmail = null,
            string instructions = null)
        {
            CurrentEmail = currentEmail;
            PrefilledNewEmail = prefilledNewEmail;
            Instructions = instructions ?? "Enter your new email address";
        }
        
        /// <summary>
        /// Пустая ViewModel по умолчанию (как Legacy)
        /// </summary>
        public static ChangeEmailViewModel Default => new ChangeEmailViewModel();
        
        /// <summary>
        /// ViewModel с текущим email (типичное использование)
        /// </summary>
        public static ChangeEmailViewModel WithCurrentEmail(string currentEmail) => 
            new ChangeEmailViewModel(currentEmail);
    }
}
