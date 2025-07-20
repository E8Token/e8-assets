using System.Threading.Tasks;
using Energy8.Identity.UI.Core.Interfaces.Strategies;

namespace Energy8.Identity.UI.Runtime.Strategies
{
    /// <summary>
    /// Стратегия авторизации через Apple.
    /// Извлечено из IdentityUIController (10+ строк Apple логики).
    /// Простая авторизация без дополнительных шагов.
    /// </summary>
    public class AppleAuthStrategy : IAuthStrategy
    {
        private readonly object identityService; // IIdentityService
        
        public string Name => "Apple";
        public bool SupportsAccountLinking => true;
        
        public AppleAuthStrategy(object identityService)
        {
            this.identityService = identityService;
        }
        
        /// <summary>
        /// Выполнить Apple авторизацию
        /// (Извлечено из IdentityUIController.ShowAuthFlow Apple case)
        /// </summary>
        public async Task<AuthStrategyResult> SignInAsync()
        {
            try
            {
                // Вызываем identityService.SignInWithApple(false) через рефлексию
                var authResult = await SignInWithApple(false);
                return AuthStrategyResult.Success();
            }
            catch (System.Exception ex)
            {
                return AuthStrategyResult.Error($"Apple authentication failed: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Связать Apple аккаунт с существующим пользователем
        /// (Для добавления провайдера в настройках)
        /// </summary>
        public async Task<AuthStrategyResult> LinkAccountAsync()
        {
            try
            {
                // Вызываем identityService.SignInWithApple(true) для связывания
                var authResult = await SignInWithApple(true);
                return AuthStrategyResult.Success();
            }
            catch (System.Exception ex)
            {
                return AuthStrategyResult.Error($"Apple account linking failed: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Apple авторизация не требует отмены (происходит мгновенно)
        /// </summary>
        public async Task CancelAsync()
        {
            // Apple auth не требует специальной отмены
            await Task.CompletedTask;
        }
        
        #region Private Methods
        
        private async Task<object> SignInWithApple(bool isLinking)
        {
            try
            {
                // Вызываем identityService.SignInWithApple(isLinking) через рефлексию
                var method = identityService.GetType().GetMethod("SignInWithApple");
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
                
                throw new System.InvalidOperationException("Failed to call SignInWithApple");
            }
            catch (System.Exception ex)
            {
                throw new System.InvalidOperationException($"Apple sign in failed: {ex.Message}", ex);
            }
        }
        
        #endregion
    }
}
