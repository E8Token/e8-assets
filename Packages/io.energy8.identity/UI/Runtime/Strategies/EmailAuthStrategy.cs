using System.Threading;
using System.Threading.Tasks;
using Energy8.Identity.UI.Core.Interfaces.Strategies;
using Energy8.Identity.UI.Core.Interfaces.Services;

namespace Energy8.Identity.UI.Runtime.Strategies
{
    /// <summary>
    /// Стратегия авторизации через Email.
    /// Извлечено из IdentityUIController (50+ строк Email логики).
    /// Обрабатывает Email flow с верификацией кода.
    /// </summary>
    public class EmailAuthStrategy : IAuthStrategy
    {
        private readonly object identityService; // IIdentityService
        private readonly INavigationService navigation;
        private readonly object viewManager; // ViewManager
        
        public string Name => "Email";
        public bool SupportsAccountLinking => false;
        
        public EmailAuthStrategy(
            object identityService, 
            INavigationService navigation,
            object viewManager)
        {
            this.identityService = identityService;
            this.navigation = navigation;
            this.viewManager = viewManager;
        }
        
        /// <summary>
        /// Выполнить Email авторизацию с кодом верификации
        /// (Извлечено из IdentityUIController.ShowAuthFlow Email case)
        /// </summary>
        public async Task<AuthStrategyResult> SignInAsync()
        {
            try
            {
                // Получаем email из SignInView (должен быть передан заранее)
                var email = await GetEmailFromContext();
                if (string.IsNullOrEmpty(email))
                {
                    return AuthStrategyResult.Error("Email not provided");
                }
                
                // Шаг 1: Отправляем код на email
                await StartEmailFlow(email);
                
                // Шаг 2: Показываем CodeView для ввода кода
                await navigation.ShowAsync<object>(); // CodeView
                
                // Шаг 3: Обрабатываем верификацию кода с возможностью resend
                var authResult = await ConfirmEmailCode(email);
                
                return AuthStrategyResult.Success();
            }
            catch (System.Exception ex)
            {
                return AuthStrategyResult.Error($"Email authentication failed: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Email стратегия не поддерживает связывание аккаунтов
        /// </summary>
        public async Task<AuthStrategyResult> LinkAccountAsync()
        {
            return AuthStrategyResult.Error("Email strategy does not support account linking");
        }
        
        /// <summary>
        /// Отменить операцию Email авторизации
        /// </summary>
        public async Task CancelAsync()
        {
            // Email flow можно прервать в любой момент
            await Task.CompletedTask;
        }
        
        #region Private Methods
        
        private async Task<string> GetEmailFromContext()
        {
            // TODO: Получить email из результата SignInView
            // Временно возвращаем пустую строку
            return await Task.FromResult("");
        }
        
        private async Task StartEmailFlow(string email)
        {
            try
            {
                // Вызываем identityService.StartEmailFlow(email) через рефлексию
                var method = identityService.GetType().GetMethod("StartEmailFlow");
                if (method != null)
                {
                    var task = method.Invoke(identityService, new object[] { email, CancellationToken.None });
                    if (task is Task asyncTask)
                    {
                        await asyncTask;
                    }
                }
            }
            catch (System.Exception ex)
            {
                throw new System.InvalidOperationException($"Failed to start email flow: {ex.Message}", ex);
            }
        }
        
        private async Task<object> ConfirmEmailCode(string email)
        {
            string code = null;
            
            // Цикл обработки кода с поддержкой resend (из Legacy логики)
            while (code == null)
            {
                var codeResult = await ShowCodeView();
                
                if (IsResendCode(codeResult))
                {
                    // Повторно отправляем код
                    await StartEmailFlow(email);
                    continue;
                }
                
                code = GetCodeFromResult(codeResult);
            }
            
            // Подтверждаем код
            return await ConfirmCode(code);
        }
        
        private async Task<object> ShowCodeView()
        {
            try
            {
                // Вызываем viewManager.Show<CodeView>() через рефлексию  
                var method = viewManager.GetType().GetMethod("Show");
                if (method != null)
                {
                    var genericMethod = method.MakeGenericMethod(
                        typeof(object), // CodeView
                        typeof(object), // CodeViewParams  
                        typeof(object)  // CodeViewResult
                    );
                    
                    var task = genericMethod.Invoke(viewManager, new object[] { 
                        new object(), // CodeViewParams
                        CancellationToken.None 
                    });
                    
                    if (task is Task<object> asyncTask)
                    {
                        return await asyncTask;
                    }
                }
                
                throw new System.InvalidOperationException("Failed to show CodeView");
            }
            catch (System.Exception ex)
            {
                throw new System.InvalidOperationException($"Failed to show code view: {ex.Message}", ex);
            }
        }
        
        private bool IsResendCode(object codeResult)
        {
            try
            {
                var codeProperty = codeResult?.GetType().GetProperty("Code");
                var code = codeProperty?.GetValue(codeResult) as string;
                return code == "RESEND";
            }
            catch
            {
                return false;
            }
        }
        
        private string GetCodeFromResult(object codeResult)
        {
            try
            {
                var codeProperty = codeResult?.GetType().GetProperty("Code");
                return codeProperty?.GetValue(codeResult) as string;
            }
            catch
            {
                return null;
            }
        }
        
        private async Task<object> ConfirmCode(string code)
        {
            try
            {
                // Вызываем identityService.ConfirmEmailCode(code) через рефлексию
                var method = identityService.GetType().GetMethod("ConfirmEmailCode");
                if (method != null)
                {
                    var task = method.Invoke(identityService, new object[] { code, CancellationToken.None });
                    if (task is Task<object> asyncTask)
                    {
                        return await asyncTask;
                    }
                }
                
                throw new System.InvalidOperationException("Failed to confirm email code");
            }
            catch (System.Exception ex)
            {
                throw new System.InvalidOperationException($"Failed to confirm code: {ex.Message}", ex);
            }
        }
        
        #endregion
    }
}
