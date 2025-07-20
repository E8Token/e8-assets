namespace Energy8.Identity.UI.Runtime.ViewModels
{
    /// <summary>
    /// ViewModel для DeleteAccountView.
    /// Содержит данные для отображения информации об удалении аккаунта.
    /// </summary>
    public class DeleteAccountViewModel
    {
        /// <summary>
        /// Имя аккаунта для удаления (опционально)
        /// </summary>
        public string AccountName { get; }
        
        /// <summary>
        /// Email аккаунта для удаления (опционально)
        /// </summary>
        public string AccountEmail { get; }
        
        /// <summary>
        /// Время ожидания в секундах (для конфигурации)
        /// </summary>
        public int WaitTimeSeconds { get; }
        
        public DeleteAccountViewModel(string accountName = null, string accountEmail = null, int waitTimeSeconds = 10)
        {
            AccountName = accountName;
            AccountEmail = accountEmail;
            WaitTimeSeconds = waitTimeSeconds;
        }
        
        /// <summary>
        /// Пустая ViewModel (как Legacy - без параметров)
        /// </summary>
        public static DeleteAccountViewModel Default => new DeleteAccountViewModel();
    }
}
