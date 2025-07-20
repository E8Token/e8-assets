namespace Energy8.Identity.UI.Runtime.ViewModels
{
    /// <summary>
    /// ViewModel для CodeView.
    /// Содержит данные для отображения информации о проверочном коде.
    /// </summary>
    public class CodeViewModel
    {
        /// <summary>
        /// Email для которого вводится код (для отображения)
        /// </summary>
        public string Email { get; }
        
        /// <summary>
        /// Ожидаемая длина кода (для валидации в Presenter)
        /// </summary>
        public int ExpectedCodeLength { get; }
        
        /// <summary>
        /// Показать ли кнопку повторной отправки
        /// </summary>
        public bool ShowResendButton { get; }
        
        /// <summary>
        /// Инструкция для пользователя
        /// </summary>
        public string Instructions { get; }
        
        public CodeViewModel(
            string email = null,
            int expectedCodeLength = 6,  // Как в Legacy - по умолчанию 6
            bool showResendButton = true,
            string instructions = null)
        {
            Email = email;
            ExpectedCodeLength = expectedCodeLength;
            ShowResendButton = showResendButton;
            Instructions = instructions ?? "Enter the verification code sent to your email";
        }
        
        /// <summary>
        /// Пустая ViewModel по умолчанию (как Legacy)
        /// </summary>
        public static CodeViewModel Default => new CodeViewModel();
        
        /// <summary>
        /// ViewModel с email (типичное использование)
        /// </summary>
        public static CodeViewModel ForEmail(string email) => new CodeViewModel(email);
    }
}
