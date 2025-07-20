using System.Threading.Tasks;
using Energy8.Identity.UI.Core.Interfaces.Strategies;

namespace Energy8.Identity.UI.Runtime.Strategies
{
    /// <summary>
    /// Стратегия авторизации через Google.
    /// Извлечено из IdentityUIController (10+ строк Google логики).
    /// Простая авторизация без дополнительных шагов.
    /// </summary>
    public class GoogleAuthStrategy : IAuthStrategy
    {
        private readonly object identityService; // IIdentityService
        
        public string Name => "Google";
        public bool SupportsAccountLinking => true;
        
        public GoogleAuthStrategy(object identityService)
        {
            this.identityService = identityService;
        }
        
        /// <summary>
        /// Выполнить Google авторизацию
        /// (Извлечено из IdentityUIController.ShowAuthFlow Google case)
        /// </summary>
        public async Task<AuthStrategyResult> SignInAsync()
        {
            try
            {
                // Вызываем identityService.SignInWithGoogle(false) через рефлексию
                var authResult = await SignInWithGoogle(false);
                return AuthStrategyResult.Success();
            }
            catch (System.Exception ex)
            {
                return AuthStrategyResult.Error($"Google authentication failed: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Связать Google аккаунт с существующим пользователем
        /// (Для добавления провайдера в настройках)
        /// </summary>
        public async Task<AuthStrategyResult> LinkAccountAsync()
        {
            try
            {
                // Вызываем identityService.SignInWithGoogle(true) для связывания
                var authResult = await SignInWithGoogle(true);
                return AuthStrategyResult.Success();
            }
            catch (System.Exception ex)
            {
                return AuthStrategyResult.Error($"Google account linking failed: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Google авторизация не требует отмены (происходит мгновенно)
        /// </summary>
        public async Task CancelAsync()
        {
            // Google auth не требует специальной отмены
            await Task.CompletedTask;
        }
        
        #region Private Methods
        
        private async Task<object> SignInWithGoogle(bool isLinking)
        {
            try
            {
                // Вызываем identityService.SignInWithGoogle(isLinking) через рефлексию
                var method = identityService.GetType().GetMethod("SignInWithGoogle");
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
                
                throw new System.InvalidOperationException("Failed to call SignInWithGoogle");
            }
            catch (System.Exception ex)
            {
                throw new System.InvalidOperationException($"Google sign in failed: {ex.Message}", ex);
            }
        }
        
        #endregion
    }
}
