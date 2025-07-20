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
    /// SettingsPresenter содержит ВСЮ бизнес-логику настроек пользователя.
    /// Управляет состоянием кнопок провайдеров и обрабатывает навигацию!
    /// </summary>
    public class SettingsPresenter : BasePresenter<SettingsView>
    {
        private readonly INavigationService navigationService;
        private SettingsViewModel currentViewModel;
        
        public SettingsPresenter(SettingsView view, INavigationService navigationService) 
            : base(view)
        {
            this.navigationService = navigationService;
            
            // Подписываемся на события View
            View.OnCloseRequested += HandleCloseRequested;
            View.OnChangeNameRequested += HandleChangeNameRequested;
            View.OnChangeEmailRequested += HandleChangeEmailRequested;
            View.OnDeleteAccountRequested += HandleDeleteAccountRequested;
            View.OnAddGoogleRequested += HandleAddGoogleRequested;
            View.OnAddAppleRequested += HandleAddAppleRequested;
            View.OnAddTelegramRequested += HandleAddTelegramRequested;
        }

        public override async Task ShowAsync()
        {
            await base.ShowAsync();
            
            // Устанавливаем ViewModel по умолчанию
            currentViewModel = SettingsViewModel.Default;
            await UpdateViewFromViewModel();
        }
        
        /// <summary>
        /// Показать SettingsView с данными пользователя
        /// </summary>
        public async Task ShowWithUserDataAsync(
            string userName, 
            string userEmail,
            bool hasGoogleProvider = false,
            bool hasAppleProvider = false, 
            bool hasTelegramProvider = false)
        {
            currentViewModel = SettingsViewModel.WithUserData(
                userName, userEmail, hasGoogleProvider, hasAppleProvider, hasTelegramProvider);
            
            await base.ShowAsync();
            await UpdateViewFromViewModel();
        }
        
        /// <summary>
        /// Обновить View на основе ViewModel (перенесено из SettingsView!)
        /// </summary>
        private async Task UpdateViewFromViewModel()
        {
            if (currentViewModel == null) return;
            
            // Устанавливаем ViewModel в View (для базовых данных)
            View.SetViewModel(currentViewModel);
            
            // Управляем состоянием кнопок провайдеров (перенесено из View!)
            // Кнопка активна только если провайдер НЕ подключен
            View.SetGoogleButtonEnabled(!currentViewModel.HasGoogleProvider);
            View.SetAppleButtonEnabled(!currentViewModel.HasAppleProvider);
            View.SetTelegramButtonEnabled(!currentViewModel.HasTelegramProvider);
            
            await Task.CompletedTask;
        }
        
        /// <summary>
        /// Обработка закрытия настроек
        /// </summary>
        private async void HandleCloseRequested()
        {
            try
            {
                // Возвращаемся к предыдущему экрану (например, UserView)
                // await navigationService.ShowAsync<UserView>();
                
                await HideAsync();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Close settings failed: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Обработка запроса изменения имени
        /// </summary>
        private async void HandleChangeNameRequested()
        {
            try
            {
                // Переход к ChangeNameView с текущим именем
                // await navigationService.ShowAsync<ChangeNameView>(
                //     ChangeNameViewModel.WithCurrentName(currentViewModel.UserName));
                
                Debug.Log($"Change name requested for: {currentViewModel.UserName}");
                
                // Пока просто логируем
            }
            catch (Exception ex)
            {
                Debug.LogError($"Change name navigation failed: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Обработка запроса изменения email
        /// </summary>
        private async void HandleChangeEmailRequested()
        {
            try
            {
                // Переход к ChangeEmailView с текущим email
                // await navigationService.ShowAsync<ChangeEmailView>(
                //     ChangeEmailViewModel.WithCurrentEmail(currentViewModel.UserEmail));
                
                Debug.Log($"Change email requested for: {currentViewModel.UserEmail}");
                
                // Пока просто логируем
            }
            catch (Exception ex)
            {
                Debug.LogError($"Change email navigation failed: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Обработка запроса удаления аккаунта
        /// </summary>
        private async void HandleDeleteAccountRequested()
        {
            try
            {
                // Переход к DeleteAccountView
                // await navigationService.ShowAsync<DeleteAccountView>();
                
                Debug.Log($"Delete account requested for: {currentViewModel.UserName}");
                
                // Пока просто логируем
            }
            catch (Exception ex)
            {
                Debug.LogError($"Delete account navigation failed: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Обработка добавления Google провайдера
        /// </summary>
        private async void HandleAddGoogleRequested()
        {
            if (currentViewModel.HasGoogleProvider)
            {
                Debug.LogWarning("Google provider already connected");
                return;
            }
            
            try
            {
                // Здесь будет вызов сервиса добавления провайдера
                // await providerService.AddGoogleProviderAsync();
                
                Debug.Log("Add Google provider requested");
                
                // После успешного добавления - обновляем ViewModel
                // await RefreshUserData();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Add Google provider failed: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Обработка добавления Apple провайдера
        /// </summary>
        private async void HandleAddAppleRequested()
        {
            if (currentViewModel.HasAppleProvider)
            {
                Debug.LogWarning("Apple provider already connected");
                return;
            }
            
            try
            {
                Debug.Log("Add Apple provider requested");
                // await providerService.AddAppleProviderAsync();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Add Apple provider failed: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Обработка добавления Telegram провайдера
        /// </summary>
        private async void HandleAddTelegramRequested()
        {
            if (currentViewModel.HasTelegramProvider)
            {
                Debug.LogWarning("Telegram provider already connected");
                return;
            }
            
            try
            {
                Debug.Log("Add Telegram provider requested");
                // await providerService.AddTelegramProviderAsync();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Add Telegram provider failed: {ex.Message}");
            }
        }
        
        protected override async Task OnDisposeAsync()
        {
            // Отписываемся от событий
            if (View != null)
            {
                View.OnCloseRequested -= HandleCloseRequested;
                View.OnChangeNameRequested -= HandleChangeNameRequested;
                View.OnChangeEmailRequested -= HandleChangeEmailRequested;
                View.OnDeleteAccountRequested -= HandleDeleteAccountRequested;
                View.OnAddGoogleRequested -= HandleAddGoogleRequested;
                View.OnAddAppleRequested -= HandleAddAppleRequested;
                View.OnAddTelegramRequested -= HandleAddTelegramRequested;
            }
            
            await base.OnDisposeAsync();
        }
    }
}
