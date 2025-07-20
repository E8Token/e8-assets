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
    /// ChangeEmailPresenter содержит ВСЮ бизнес-логику изменения email.
    /// Использует общий ValidationService вместо дублирования кода!
    /// </summary>
    public class ChangeEmailPresenter : BasePresenter<ChangeEmailView>
    {
        private readonly INavigationService navigationService;
        private ChangeEmailViewModel currentViewModel;
        
        public ChangeEmailPresenter(ChangeEmailView view, INavigationService navigationService) 
            : base(view)
        {
            this.navigationService = navigationService;
            
            // Подписываемся на события View
            View.OnEmailChanged += HandleEmailChanged;
            View.OnEmailSubmitted += HandleEmailSubmitted;
            View.OnCloseRequested += HandleCloseRequested;
        }

        public override async Task ShowAsync()
        {
            await base.ShowAsync();
            
            // Устанавливаем ViewModel по умолчанию
            currentViewModel = ChangeEmailViewModel.Default;
            View.SetViewModel(currentViewModel);
            
            // Фокусируемся на поле ввода
            View.FocusEmailInput();
            
            // Проверяем текущий email при показе
            HandleEmailChanged(View.GetCurrentEmail());
        }
        
        /// <summary>
        /// Показать ChangeEmailView с текущим email
        /// </summary>
        public async Task ShowWithCurrentEmailAsync(string currentEmail)
        {
            currentViewModel = ChangeEmailViewModel.WithCurrentEmail(currentEmail);
            
            await base.ShowAsync();
            View.SetViewModel(currentViewModel);
            View.FocusEmailInput();
            
            HandleEmailChanged(View.GetCurrentEmail());
        }
        
        /// <summary>
        /// Обработка изменения email - НЕ дублируем валидацию!
        /// </summary>
        private void HandleEmailChanged(string email)
        {
            // Используем ОБЩИЙ сервис валидации вместо дублирования!
            bool isValid = ValidationService.IsValidEmail(email);
            View.SetNextButtonEnabled(isValid);
            
            // Проверяем что новый email отличается от текущего
            if (!string.IsNullOrEmpty(currentViewModel?.CurrentEmail) && 
                string.Equals(email, currentViewModel.CurrentEmail, StringComparison.OrdinalIgnoreCase))
            {
                View.ShowEmailError("New email must be different from current email");
                View.SetNextButtonEnabled(false);
                return;
            }
            
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
        /// Обработка подтверждения нового email
        /// </summary>
        private async void HandleEmailSubmitted(string newEmail)
        {
            if (!ValidationService.IsValidEmail(newEmail))
            {
                View.ShowEmailError("Please enter a valid email address");
                return;
            }
            
            // Проверяем что email не такой же как текущий
            if (!string.IsNullOrEmpty(currentViewModel?.CurrentEmail) && 
                string.Equals(newEmail, currentViewModel.CurrentEmail, StringComparison.OrdinalIgnoreCase))
            {
                View.ShowEmailError("New email must be different from current email");
                return;
            }
            
            try
            {
                // Здесь будет вызов сервиса изменения email
                // await userService.ChangeEmailAsync(newEmail);
                
                Debug.Log($"Email change requested: {currentViewModel?.CurrentEmail} -> {newEmail}");
                
                // Возможно потребуется подтверждение по коду
                // await navigationService.ShowAsync<CodeView>(CodeViewModel.ForEmail(newEmail));
                
                // Пока просто скрываем View
                await HideAsync();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Email change failed: {ex.Message}");
                View.ShowEmailError("Failed to change email. Please try again.");
            }
        }
        
        /// <summary>
        /// Обработка закрытия окна изменения email
        /// </summary>
        private async void HandleCloseRequested()
        {
            try
            {
                // Возвращаемся к предыдущему экрану (например, SettingsView)
                // await navigationService.ShowAsync<SettingsView>();
                
                await HideAsync();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Close failed: {ex.Message}");
            }
        }
        
        protected override async Task OnDisposeAsync()
        {
            // Отписываемся от событий
            if (View != null)
            {
                View.OnEmailChanged -= HandleEmailChanged;
                View.OnEmailSubmitted -= HandleEmailSubmitted;
                View.OnCloseRequested -= HandleCloseRequested;
            }
            
            await base.OnDisposeAsync();
        }
    }
}
