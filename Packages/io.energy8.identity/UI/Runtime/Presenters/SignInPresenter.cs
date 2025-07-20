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
    /// SignInPresenter содержит ВСЮ бизнес-логику авторизации.
    /// Переносит валидацию email и бизнес-решения ИЗ View в Presenter!
    /// </summary>
    public class SignInPresenter : BasePresenter<SignInView>
    {
        private readonly INavigationService navigationService;
        
        public SignInPresenter(SignInView view, INavigationService navigationService) 
            : base(view)
        {
            this.navigationService = navigationService;
            
            // Подписываемся на события View
            View.OnEmailChanged += HandleEmailChanged;
            View.OnEmailSubmitted += HandleEmailSubmitted;
            View.OnGoogleRequested += HandleGoogleRequested;
            View.OnAppleRequested += HandleAppleRequested;
            View.OnTelegramRequested += HandleTelegramRequested;
        }

        public override async Task ShowAsync()
        {
            await base.ShowAsync();
            
            // Устанавливаем ViewModel по умолчанию
            View.SetViewModel(SignInViewModel.Default);
            
            // Проверяем текущий email при показе
            HandleEmailChanged(View.GetCurrentEmail());
        }
        
        /// <summary>
        /// Обработка изменения email (перенесено из SignInView!)
        /// </summary>
        private void HandleEmailChanged(string email)
        {
            // Используем ОБЩИЙ ValidationService!
            bool isValid = ValidationService.IsValidEmail(email);
            View.SetNextButtonEnabled(isValid);
            
            if (!string.IsNullOrEmpty(email) && !isValid)
            {
                View.ShowEmailError("Invalid email format");
            }
            else
            {
                View.ClearEmailError();
            }
        }
        
        /// <summary>
        /// Обработка подтверждения email
        /// </summary>
        private async void HandleEmailSubmitted(string email)
        {
            if (!ValidationService.IsValidEmail(email))
            {
                View.ShowEmailError("Please enter a valid email");
                return;
            }
            
            try
            {
                // Здесь будет вызов сервиса авторизации
                // await authService.SignInWithEmailAsync(email);
                
                // Переход к следующему экрану (например, CodeView)
                // await navigationService.ShowAsync<CodeView>(new CodeViewModel(email));
                
                Debug.Log($"Email sign-in requested: {email}");
                
                // Пока просто скрываем View
                await HideAsync();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Email sign-in failed: {ex.Message}");
                View.ShowEmailError("Sign-in failed. Please try again.");
            }
        }
        
        /// <summary>
        /// Обработка авторизации через Google
        /// </summary>
        private async void HandleGoogleRequested()
        {
            try
            {
                // Здесь будет вызов сервиса авторизации
                // await authService.SignInWithGoogleAsync();
                
                Debug.Log("Google sign-in requested");
                
                // Можно показать LoadingView во время авторизации
                // await navigationService.ShowAsync<LoadingView>();
                
                await HideAsync();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Google sign-in failed: {ex.Message}");
                // Можно показать ErrorView
            }
        }
        
        /// <summary>
        /// Обработка авторизации через Apple
        /// </summary>
        private async void HandleAppleRequested()
        {
            try
            {
                Debug.Log("Apple sign-in requested");
                await HideAsync();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Apple sign-in failed: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Обработка авторизации через Telegram
        /// </summary>
        private async void HandleTelegramRequested()
        {
            try
            {
                Debug.Log("Telegram sign-in requested");
                await HideAsync();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Telegram sign-in failed: {ex.Message}");
            }
        }
        
        protected override async Task OnDisposeAsync()
        {
            // Отписываемся от событий
            if (View != null)
            {
                View.OnEmailChanged -= HandleEmailChanged;
                View.OnEmailSubmitted -= HandleEmailSubmitted;
                View.OnGoogleRequested -= HandleGoogleRequested;
                View.OnAppleRequested -= HandleAppleRequested;
                View.OnTelegramRequested -= HandleTelegramRequested;
            }
            
            await base.OnDisposeAsync();
        }
    }
}
