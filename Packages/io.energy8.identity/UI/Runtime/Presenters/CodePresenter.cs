using System;
using System.Threading.Tasks;
using UnityEngine;
using Energy8.Identity.UI.Core.Components;
using Energy8.Identity.UI.Core.Interfaces;
using Energy8.Identity.UI.Runtime.Views;
using Energy8.Identity.UI.Runtime.ViewModels;
using Energy8.Identity.UI.Runtime.Services;

namespace Energy8.Identity.UI.Runtime.Presenters
{
    /// <summary>
    /// CodePresenter содержит ВСЮ бизнес-логику ввода проверочного кода.
    /// Переносит валидацию длины и логику "RESEND" ИЗ View в Presenter!
    /// </summary>
    public class CodePresenter : BasePresenter<CodeView>
    {
        private readonly INavigationService navigationService;
        private CodeViewModel currentViewModel;
        
        public CodePresenter(CodeView view, INavigationService navigationService) 
            : base(view)
        {
            this.navigationService = navigationService;
            
            // Подписываемся на события View
            View.OnCodeChanged += HandleCodeChanged;
            View.OnCodeSubmitted += HandleCodeSubmitted;
            View.OnResendRequested += HandleResendRequested;
            View.OnCancelRequested += HandleCancelRequested;
        }

        public override async Task ShowAsync()
        {
            await base.ShowAsync();
            
            // Устанавливаем ViewModel по умолчанию
            currentViewModel = CodeViewModel.Default;
            View.SetViewModel(currentViewModel);
            
            // Фокусируемся на поле ввода
            View.FocusCodeInput();
            
            // Проверяем текущий код при показе
            HandleCodeChanged(View.GetCurrentCode());
        }
        
        /// <summary>
        /// Показать CodeView для конкретного email
        /// </summary>
        public async Task ShowForEmailAsync(string email)
        {
            currentViewModel = CodeViewModel.ForEmail(email);
            
            await base.ShowAsync();
            View.SetViewModel(currentViewModel);
            View.FocusCodeInput();
            
            HandleCodeChanged(View.GetCurrentCode());
        }
        
        /// <summary>
        /// Обработка изменения кода (перенесено из CodeView!)
        /// </summary>
        private void HandleCodeChanged(string code)
        {
            // Используем ОБЩИЙ ValidationService!
            int expectedLength = currentViewModel?.ExpectedCodeLength ?? 6;
            bool isValid = ValidationService.IsValidCode(code, expectedLength);
            View.SetNextButtonEnabled(isValid);
            
            if (!string.IsNullOrEmpty(code) && !isValid)
            {
                View.ShowCodeError($"Code must be {currentViewModel.ExpectedCodeLength} digits");
            }
            else
            {
                View.ClearCodeError();
            }
        }
        
        /// <summary>
        /// Обработка подтверждения кода
        /// </summary>
        private async void HandleCodeSubmitted(string code)
        {
            int expectedLength = currentViewModel?.ExpectedCodeLength ?? 6;
            if (!ValidationService.IsValidCode(code, expectedLength))
            {
                View.ShowCodeError($"Please enter a valid {expectedLength}-digit code");
                return;
            }
            
            try
            {
                // Здесь будет вызов сервиса проверки кода
                // await authService.VerifyCodeAsync(code, currentViewModel.Email);
                
                Debug.Log($"Code verification requested: {code} for email: {currentViewModel.Email}");
                
                // Переход к следующему экрану (например, UserView)
                // await navigationService.ReplaceAsync<UserView>();
                
                // Пока просто скрываем View
                await HideAsync();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Code verification failed: {ex.Message}");
                View.ShowCodeError("Invalid code. Please try again.");
                
                // Очищаем поле для повторного ввода
                View.ClearCode();
                View.FocusCodeInput();
            }
        }
        
        /// <summary>
        /// Обработка запроса повторной отправки (перенесено из CodeView!)
        /// </summary>
        private async void HandleResendRequested()
        {
            try
            {
                // Здесь будет вызов сервиса повторной отправки кода
                // await authService.ResendCodeAsync(currentViewModel.Email);
                
                Debug.Log($"Code resend requested for email: {currentViewModel.Email}");
                
                // Можно показать LoadingView во время отправки
                // await navigationService.ShowAsync<LoadingView>();
                
                // Очищаем поле и показываем сообщение
                View.ClearCode();
                View.ClearCodeError();
                View.FocusCodeInput();
                
                // Здесь можно показать уведомление об успешной отправке
                Debug.Log("Verification code sent successfully!");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Code resend failed: {ex.Message}");
                View.ShowCodeError("Failed to resend code. Please try again.");
            }
        }
        
        /// <summary>
        /// Обработка отмены ввода кода
        /// </summary>
        private async void HandleCancelRequested()
        {
            try
            {
                // Возвращаемся к предыдущему экрану (например, SignInView)
                // await navigationService.ShowAsync<SignInView>();
                
                await HideAsync();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Cancel failed: {ex.Message}");
            }
        }
        
        protected override async Task OnDisposeAsync()
        {
            // Отписываемся от событий
            if (View != null)
            {
                View.OnCodeChanged -= HandleCodeChanged;
                View.OnCodeSubmitted -= HandleCodeSubmitted;
                View.OnResendRequested -= HandleResendRequested;
                View.OnCancelRequested -= HandleCancelRequested;
            }
            
            await base.OnDisposeAsync();
        }
    }
}
