using System.Threading.Tasks;
using Energy8.Identity.UI.Core.Interfaces.Strategies;

namespace Energy8.Identity.UI.Runtime.Strategies
{
    /// <summary>
    /// Стратегия авторизации через Telegram.
    /// Извлечено из IdentityUIController (10+ строк Telegram логики).
    /// Простая авторизация без дополнительных шагов.
    /// </summary>
    public class TelegramAuthStrategy : IAuthStrategy
    {
        private readonly object identityService; // IIdentityService
        
        public string Name => "Telegram";
        public bool SupportsAccountLinking => true;
        
        public TelegramAuthStrategy(object identityService)
        {
            this.identityService = identityService;
        }
        
        /// <summary>
        /// Выполнить Telegram авторизацию
        /// (Извлечено из IdentityUIController.ShowAuthFlow Telegram case)
        /// </summary>
        public async Task<AuthStrategyResult> SignInAsync()
        {
            try
            {
                // Вызываем identityService.SignInWithTelegramAsync(false) через рефлексию
                var authResult = await SignInWithTelegram(false);
                return AuthStrategyResult.Success();
            }
            catch (System.Exception ex)
            {
                return AuthStrategyResult.Error($"Telegram authentication failed: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Связать Telegram аккаунт с существующим пользователем
        /// (Для добавления провайдера в настройках)
        /// </summary>
        public async Task<AuthStrategyResult> LinkAccountAsync()
        {
            try
            {
                // Вызываем identityService.SignInWithTelegramAsync(true) для связывания
                var authResult = await SignInWithTelegram(true);
                return AuthStrategyResult.Success();
            }
            catch (System.Exception ex)
            {
                return AuthStrategyResult.Error($"Telegram account linking failed: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Telegram авторизация не требует отмены (происходит мгновенно)
        /// </summary>
        public async Task CancelAsync()
        {
            // Telegram auth не требует специальной отмены
            await Task.CompletedTask;
        }
        
        #region Private Methods
        
        private async Task<object> SignInWithTelegram(bool isLinking)
        {
            try
            {
                // Вызываем identityService.SignInWithTelegramAsync(isLinking) через рефлексию
                var method = identityService.GetType().GetMethod("SignInWithTelegramAsync");
                if (method != null)
                {
                    var task = method.Invoke(identityService, new object[] { 
                        isLinking, 
                        System.Threading.CancellationToken.None 
                    });
                    
                    if (task is Task<object> asyncTask)
                    {
                        return await asyncTask;
                    }
                }
                
                throw new System.InvalidOperationException("Failed to call SignInWithTelegramAsync");
            }
            catch (System.Exception ex)
            {
                throw new System.InvalidOperationException($"Telegram sign in failed: {ex.Message}", ex);
            }
        }
        
        #endregion
    }
}
