using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Energy8.Identity.UI.Core.Components;
using Energy8.Identity.UI.Core.Interfaces;
using Energy8.Identity.UI.Runtime.Views;
using Energy8.Identity.UI.Runtime.ViewModels;

namespace Energy8.Identity.UI.Runtime.Presenters
{
    /// <summary>
    /// DeleteAccountPresenter содержит ВСЮ логику таймера и подтверждения удаления.
    /// Переносит async логику ИЗ View в Presenter - правильная архитектура!
    /// </summary>
    public class DeleteAccountPresenter : BasePresenter<DeleteAccountView>
    {
        private readonly INavigationService navigationService;
        private CancellationTokenSource countdownCancellation;
        private bool isCountdownCompleted;
        
        public DeleteAccountPresenter(DeleteAccountView view, INavigationService navigationService) 
            : base(view)
        {
            this.navigationService = navigationService;
            
            View.OnDeleteConfirmed += HandleDeleteConfirmed;
            View.OnCancelled += HandleCancelled;
        }

        public override async Task ShowAsync()
        {
            await base.ShowAsync();
            
            // Устанавливаем ViewModel по умолчанию
            View.SetViewModel(DeleteAccountViewModel.Default);
            
            // Запускаем таймер countdown (перенесено из View!)
            await StartCountdownAsync();
        }
        
        /// <summary>
        /// Запустить countdown таймер (перенесено из DeleteAccountView!)
        /// </summary>
        private async Task StartCountdownAsync()
        {
            isCountdownCompleted = false;
            countdownCancellation?.Cancel();
            countdownCancellation = new CancellationTokenSource();
            
            // Получаем время ожидания из ViewModel
            var viewModel = DeleteAccountViewModel.Default;
            int waitTime = viewModel.WaitTimeSeconds;
            
            try
            {
                // Отключаем кнопку удаления
                View.SetDeleteButtonEnabled(false);
                
                // Countdown как в Legacy
                for (int i = waitTime; i > 0 && !countdownCancellation.Token.IsCancellationRequested; i--)
                {
                    View.UpdateCountdown(i);
                    await Task.Delay(1000, countdownCancellation.Token);
                }
                
                // Таймер завершен - включаем кнопку
                if (!countdownCancellation.Token.IsCancellationRequested)
                {
                    isCountdownCompleted = true;
                    View.SetDeleteButtonEnabled(true);
                }
            }
            catch (OperationCanceledException)
            {
                // Таймер был отменен (например, при закрытии View)
                Debug.Log("DeleteAccount countdown cancelled");
            }
        }
        
        private async void HandleDeleteConfirmed()
        {
            if (!isCountdownCompleted)
            {
                // Кнопка еще не должна быть доступна
                return;
            }
            
            try
            {
                // Здесь будет вызов сервиса удаления аккаунта
                // await deleteAccountService.DeleteAsync();
                
                // Переход к следующему экрану (например, SignInView)
                // await navigationService.ReplaceAsync<SignInView>();
                
                // Пока просто закрываем
                await HideAsync();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to delete account: {ex.Message}");
                
                // Можно показать ErrorView
                // await navigationService.ShowAsync<ErrorView>(new ErrorViewModel(ex.Message));
            }
        }
        
        private async void HandleCancelled()
        {
            // Отменяем удаление аккаунта
            await HideAsync();
        }
        
        protected override async Task OnHideAsync()
        {
            // Отменяем таймер при скрытии View
            countdownCancellation?.Cancel();
            await base.OnHideAsync();
        }
        
        protected override async Task OnDisposeAsync()
        {
            // Очищаем ресурсы
            countdownCancellation?.Cancel();
            countdownCancellation?.Dispose();
            
            if (View != null)
            {
                View.OnDeleteConfirmed -= HandleDeleteConfirmed;
                View.OnCancelled -= HandleCancelled;
            }
            
            await base.OnDisposeAsync();
        }
    }
}
