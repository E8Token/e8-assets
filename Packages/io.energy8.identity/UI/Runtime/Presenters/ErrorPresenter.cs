using System;
using System.Threading.Tasks;
using UnityEngine;
using Energy8.Identity.UI.Core.Components;
using Energy8.Identity.UI.Core.Interfaces;
using Energy8.Identity.UI.Runtime.Views;
using Energy8.Identity.UI.Runtime.ViewModels;

namespace Energy8.Identity.UI.Runtime.Presenters
{
    /// <summary>
    /// ErrorPresenter содержит ВСЮ бизнес-логику обработки ошибок.
    /// Переносит layout логику и системную интеграцию ИЗ View в Presenter!
    /// </summary>
    public class ErrorPresenter : BasePresenter<ErrorView>
    {
        private readonly INavigationService navigationService;
        private ErrorViewModel currentViewModel;
        private Action retryAction;
        
        public ErrorPresenter(ErrorView view, INavigationService navigationService) 
            : base(view)
        {
            this.navigationService = navigationService;
            
            // Подписываемся на события View
            View.OnCloseRequested += HandleCloseRequested;
            View.OnTryAgainRequested += HandleTryAgainRequested;
            View.OnSignOutRequested += HandleSignOutRequested;
            View.OnContactRequested += HandleContactRequested;
        }

        public override async Task ShowAsync()
        {
            await base.ShowAsync();
            
            // Показываем простую ошибку по умолчанию
            currentViewModel = ErrorViewModel.SimpleError("Error", "An error occurred");
            await SetupErrorUI();
        }
        
        /// <summary>
        /// Показать ErrorView с конкретной ошибкой
        /// </summary>
        public async Task ShowErrorAsync(ErrorViewModel errorViewModel, Action onRetry = null)
        {
            currentViewModel = errorViewModel;
            retryAction = onRetry;
            
            await base.ShowAsync();
            await SetupErrorUI();
        }
        
        /// <summary>
        /// Показать простую ошибку
        /// </summary>
        public async Task ShowSimpleErrorAsync(string header, string description)
        {
            await ShowErrorAsync(ErrorViewModel.SimpleError(header, description));
        }
        
        /// <summary>
        /// Показать ошибку с возможностью повтора
        /// </summary>
        public async Task ShowRetryableErrorAsync(string header, string description, Action onRetry)
        {
            await ShowErrorAsync(ErrorViewModel.RetryableError(header, description), onRetry);
        }
        
        /// <summary>
        /// Показать критическую ошибку
        /// </summary>
        public async Task ShowCriticalErrorAsync(string header, string description)
        {
            await ShowErrorAsync(ErrorViewModel.CriticalError(header, description));
        }
        
        /// <summary>
        /// Настроить UI ошибки (перенесено из ErrorView!)
        /// </summary>
        private async Task SetupErrorUI()
        {
            if (currentViewModel == null) return;
            
            // Устанавливаем ViewModel в View (для базовых данных)
            View.SetViewModel(currentViewModel);
            
            // Управляем видимостью кнопок (перенесено из View!)
            View.ShowCloseButton(currentViewModel.CanProceed);
            View.ShowTryAgainButton(currentViewModel.CanRetry);
            View.ShowSignOutButton(currentViewModel.MustSignOut);
            View.ShowContactButton(currentViewModel.ShowContact);
            
            // Обновляем layout после изменения видимости кнопок
            View.RefreshLayout();
            
            await Task.CompletedTask;
        }
        
        /// <summary>
        /// Обработка закрытия ошибки
        /// </summary>
        private async void HandleCloseRequested()
        {
            try
            {
                // Возвращаемся к предыдущему экрану или скрываем ошибку
                await HideAsync();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Close error failed: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Обработка повторной попытки
        /// </summary>
        private async void HandleTryAgainRequested()
        {
            try
            {
                // Скрываем ErrorView
                await HideAsync();
                
                // Выполняем action повтора если он задан
                retryAction?.Invoke();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Try again failed: {ex.Message}");
                
                // Можем показать другую ошибку
                await ShowSimpleErrorAsync("Retry Failed", ex.Message);
            }
        }
        
        /// <summary>
        /// Обработка выхода из системы
        /// </summary>
        private async void HandleSignOutRequested()
        {
            try
            {
                // Здесь будет вызов сервиса выхода из системы
                // await authService.SignOutAsync();
                
                Debug.Log("Sign out requested due to critical error");
                
                // Переход к SignInView
                // await navigationService.ReplaceAsync<SignInView>();
                
                await HideAsync();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Sign out failed: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Обработка обращения в поддержку (перенесено из ErrorView!)
        /// </summary>
        private void HandleContactRequested()
        {
            try
            {
                // Перенесенная системная интеграция из ErrorView!
                string supportEmail = currentViewModel?.SupportEmail ?? "energy8sup@gmail.com";
                string subject = Uri.EscapeDataString($"Support Request - {currentViewModel?.Header}");
                string body = Uri.EscapeDataString($"Error: {currentViewModel?.Description}");
                
                string mailtoUrl = $"mailto:{supportEmail}?subject={subject}&body={body}";
                
                Application.OpenURL(mailtoUrl);
                
                Debug.Log($"Opening support email: {supportEmail}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Contact support failed: {ex.Message}");
            }
        }
        
        protected override async Task OnDisposeAsync()
        {
            // Отписываемся от событий
            if (View != null)
            {
                View.OnCloseRequested -= HandleCloseRequested;
                View.OnTryAgainRequested -= HandleTryAgainRequested;
                View.OnSignOutRequested -= HandleSignOutRequested;
                View.OnContactRequested -= HandleContactRequested;
            }
            
            retryAction = null;
            
            await base.OnDisposeAsync();
        }
    }
}
