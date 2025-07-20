using System;
using System.Threading.Tasks;

namespace Energy8.Identity.UI.Core.Interfaces.Controllers
{
    /// <summary>
    /// Интерфейс контроллера потока авторизации.
    /// Ответственность: ТОЛЬКО логика авторизации (Email/Google/Apple/Telegram).
    /// НЕ управляет UI, НЕ управляет окнами - только бизнес-логика авторизации!
    /// </summary>
    public interface IAuthFlowController : IDisposable
    {
        /// <summary>
        /// Запустить поток авторизации
        /// </summary>
        Task StartAsync();
        
        /// <summary>
        /// Обработать авторизацию через Email
        /// </summary>
        /// <param name="email">Email адрес для авторизации</param>
        Task HandleEmailSignInAsync(string email);
        
        /// <summary>
        /// Обработать авторизацию через Google
        /// </summary>
        Task HandleGoogleSignInAsync();
        
        /// <summary>
        /// Обработать авторизацию через Apple
        /// </summary>
        Task HandleAppleSignInAsync();
        
        /// <summary>
        /// Обработать авторизацию через Telegram
        /// </summary>
        Task HandleTelegramSignInAsync();
        
        /// <summary>
        /// Обработать верификацию кода (для Email)
        /// </summary>
        /// <param name="code">Код верификации</param>
        Task HandleCodeVerificationAsync(string code);
        
        /// <summary>
        /// Обработать повторную отправку кода
        /// </summary>
        Task HandleCodeResendAsync();
        
        /// <summary>
        /// Остановить поток авторизации
        /// </summary>
        Task StopAsync();
        
        #region Events
        
        /// <summary>
        /// Событие успешной авторизации
        /// </summary>
        event Action OnAuthenticationSucceeded;
        
        /// <summary>
        /// Событие неудачной авторизации
        /// </summary>
        event Action<string> OnAuthenticationFailed;
        
        #endregion
    }
}
