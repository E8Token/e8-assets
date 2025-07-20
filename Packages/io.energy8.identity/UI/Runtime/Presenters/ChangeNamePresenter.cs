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
    /// ChangeNamePresenter содержит ВСЮ бизнес-логику изменения имени.
    /// Использует ValidationService для валидации длины имени!
    /// </summary>
    public class ChangeNamePresenter : BasePresenter<ChangeNameView>
    {
        private readonly INavigationService navigationService;
        private ChangeNameViewModel currentViewModel;
        
        public ChangeNamePresenter(ChangeNameView view, INavigationService navigationService) 
            : base(view)
        {
            this.navigationService = navigationService;
            
            // Подписываемся на события View
            View.OnNameChanged += HandleNameChanged;
            View.OnNameSubmitted += HandleNameSubmitted;
            View.OnCloseRequested += HandleCloseRequested;
        }

        public override async Task ShowAsync()
        {
            await base.ShowAsync();
            
            // Устанавливаем ViewModel по умолчанию
            currentViewModel = ChangeNameViewModel.Default;
            View.SetViewModel(currentViewModel);
            
            // Фокусируемся на поле ввода
            View.FocusNameInput();
            
            // Проверяем текущее имя при показе
            HandleNameChanged(View.GetCurrentName());
        }
        
        /// <summary>
        /// Показать ChangeNameView с текущим именем
        /// </summary>
        public async Task ShowWithCurrentNameAsync(string currentName)
        {
            currentViewModel = ChangeNameViewModel.WithCurrentName(currentName);
            
            await base.ShowAsync();
            View.SetViewModel(currentViewModel);
            View.FocusNameInput();
            
            HandleNameChanged(View.GetCurrentName());
        }
        
        /// <summary>
        /// Обработка изменения имени - используем ValidationService!
        /// </summary>
        private void HandleNameChanged(string name)
        {
            // Используем ValidationService вместо жестко зашитой логики!
            int minLength = currentViewModel?.MinNameLength ?? 3;
            bool isValid = ValidationService.IsValidName(name, minLength);
            View.SetNextButtonEnabled(isValid);
            
            // Проверяем что новое имя отличается от текущего
            if (!string.IsNullOrEmpty(currentViewModel?.CurrentName) && 
                string.Equals(name?.Trim(), currentViewModel.CurrentName?.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                View.ShowNameError("New name must be different from current name");
                View.SetNextButtonEnabled(false);
                return;
            }
            
            if (!string.IsNullOrEmpty(name) && !isValid)
            {
                View.ShowNameError($"Name must be longer than {minLength} characters");
            }
            else
            {
                View.ClearNameError();
            }
        }
        
        /// <summary>
        /// Обработка подтверждения нового имени
        /// </summary>
        private async void HandleNameSubmitted(string newName)
        {
            int minLength = currentViewModel?.MinNameLength ?? 3;
            if (!ValidationService.IsValidName(newName, minLength))
            {
                View.ShowNameError($"Name must be longer than {minLength} characters");
                return;
            }
            
            // Проверяем что имя не такое же как текущее
            if (!string.IsNullOrEmpty(currentViewModel?.CurrentName) && 
                string.Equals(newName?.Trim(), currentViewModel.CurrentName?.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                View.ShowNameError("New name must be different from current name");
                return;
            }
            
            try
            {
                // Здесь будет вызов сервиса изменения имени
                // await userService.ChangeNameAsync(newName);
                
                Debug.Log($"Name change requested: '{currentViewModel?.CurrentName}' -> '{newName}'");
                
                // Переход к предыдущему экрану или показ успеха
                // await navigationService.ShowAsync<SettingsView>();
                
                // Пока просто скрываем View
                await HideAsync();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Name change failed: {ex.Message}");
                View.ShowNameError("Failed to change name. Please try again.");
            }
        }
        
        /// <summary>
        /// Обработка закрытия окна изменения имени
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
                View.OnNameChanged -= HandleNameChanged;
                View.OnNameSubmitted -= HandleNameSubmitted;
                View.OnCloseRequested -= HandleCloseRequested;
            }
            
            await base.OnDisposeAsync();
        }
    }
}
