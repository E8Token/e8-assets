using System.Text.RegularExpressions;

namespace Energy8.Identity.UI.Runtime.Services
{
    /// <summary>
    /// Общий сервис валидации для всех Views.
    /// Убирает дублирование валидации между SignInView и ChangeEmailView!
    /// </summary>
    public static class ValidationService
    {
        // Тот же паттерн что использовался в Legacy SignInView и ChangeEmailView
        private const string EMAIL_PATTERN = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        
        /// <summary>
        /// Валидация email (общая для SignInView и ChangeEmailView)
        /// </summary>
        /// <param name="email">Email для валидации</param>
        /// <returns>True если email валидный</returns>
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;
                
            return Regex.IsMatch(email, EMAIL_PATTERN);
        }
        
        /// <summary>
        /// Валидация имени пользователя (для ChangeNameView)
        /// </summary>
        /// <param name="name">Имя для валидации</param>
        /// <param name="minLength">Минимальная длина (по умолчанию 3 как в Legacy)</param>
        /// <returns>True если имя валидное</returns>
        public static bool IsValidName(string name, int minLength = 3)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;
                
            return name.Trim().Length > minLength;
        }
        
        /// <summary>
        /// Валидация проверочного кода
        /// </summary>
        /// <param name="code">Код для валидации</param>
        /// <param name="expectedLength">Ожидаемая длина кода</param>
        /// <returns>True если код валидный</returns>
        public static bool IsValidCode(string code, int expectedLength = 6)
        {
            if (string.IsNullOrWhiteSpace(code))
                return false;
                
            if (code.Length != expectedLength)
                return false;
                
            // Проверяем что все символы - цифры
            foreach (char c in code)
            {
                if (!char.IsDigit(c))
                    return false;
            }
            
            return true;
        }
    }
}
