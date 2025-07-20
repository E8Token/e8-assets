using System.Threading.Tasks;

namespace Energy8.Identity.UI.Core.Interfaces.Strategies
{
    /// <summary>
    /// Интерфейс стратегии авторизации.
    /// Реализует Strategy pattern для различных методов авторизации.
    /// Каждый провайдер (Email, Google, Apple, Telegram) имеет свою стратегию.
    /// </summary>
    public interface IAuthStrategy
    {
        /// <summary>
        /// Название стратегии (Email, Google, Apple, Telegram)
        /// </summary>
        string Name { get; }
        
        /// <summary>
        /// Выполнить авторизацию с использованием этой стратегии
        /// </summary>
        Task<AuthStrategyResult> SignInAsync();
        
        /// <summary>
        /// Выполнить авторизацию для связывания аккаунта (добавления провайдера)
        /// </summary>
        Task<AuthStrategyResult> LinkAccountAsync();
        
        /// <summary>
        /// Поддерживает ли стратегия связывание аккаунтов
        /// </summary>
        bool SupportsAccountLinking { get; }
        
        /// <summary>
        /// Отменить текущую операцию авторизации
        /// </summary>
        Task CancelAsync();
    }
    
    /// <summary>
    /// Результат выполнения стратегии авторизации
    /// </summary>
    public class AuthStrategyResult
    {
        /// <summary>
        /// Успешна ли авторизация
        /// </summary>
        public bool IsSuccess { get; set; }
        
        /// <summary>
        /// Сообщение об ошибке (если IsSuccess = false)
        /// </summary>
        public string ErrorMessage { get; set; }
        
        /// <summary>
        /// Требуется ли дополнительная верификация (например, код для Email)
        /// </summary>
        public bool RequiresVerification { get; set; }
        
        /// <summary>
        /// Данные для верификации (например, email для отправки кода)
        /// </summary>
        public object VerificationData { get; set; }
        
        /// <summary>
        /// Создать успешный результат
        /// </summary>
        public static AuthStrategyResult Success() => new AuthStrategyResult { IsSuccess = true };
        
        /// <summary>
        /// Создать результат с требованием верификации
        /// </summary>
        public static AuthStrategyResult NeedsVerification(object data) => 
            new AuthStrategyResult { IsSuccess = false, RequiresVerification = true, VerificationData = data };
        
        /// <summary>
        /// Создать результат с ошибкой
        /// </summary>
        public static AuthStrategyResult Error(string message) => 
            new AuthStrategyResult { IsSuccess = false, ErrorMessage = message };
    }
}
