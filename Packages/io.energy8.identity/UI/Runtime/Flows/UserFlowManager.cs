using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Energy8.Identity.User.Core.Services;
using Energy8.Identity.UI.Runtime.Services;
using Energy8.Identity.UI.Runtime.Canvas;
using Energy8.Identity.UI.Runtime.State;
using Energy8.Identity.UI.Runtime.Error;
using Energy8.Identity.UI.Runtime.Views.Implementations;
using Energy8.Identity.UI.Runtime.Views.Implementations.User;
using Energy8.Identity.UI.Runtime.Views.Models;
using Energy8.Identity.Shared.Core.Contracts.Dto.User;
using Energy8.Identity.Shared.Core.Exceptions;
using Energy8.Identity.Shared.Core.Error;
using Firebase.Auth;

namespace Energy8.Identity.UI.Runtime.Flows
{
    /// <summary>
    /// Управляет потоками авторизованного пользователя.
    /// Profile, Settings, Account management.
    /// Точный перенос ShowUserFlow, ShowSettings и account management методов
    /// </summary>
    public class UserFlowManager : IUserFlowManager
    {
        private readonly IUserService userService;
        private readonly IIdentityService identityService;
        private readonly ICanvasManager canvasManager;
        private readonly IStateManager stateManager;
        private readonly IErrorHandler errorHandler;
        private readonly bool debugLogging;
        
        private bool isShowingUserFlow = false; // Перенос флага из строки 57
        
        public UserFlowManager(
            IUserService userService,
            IIdentityService identityService,
            ICanvasManager canvasManager,
            IStateManager stateManager,
            IErrorHandler errorHandler,
            bool debugLogging)
        {
            this.userService = userService;
            this.identityService = identityService;
            this.canvasManager = canvasManager;
            this.stateManager = stateManager;
            this.errorHandler = errorHandler;
            this.debugLogging = debugLogging;
        }
        
        #region Main User Flow (точный перенос из строк 576-631)
        
        /// <summary>
        /// Запуск основного пользовательского потока
        /// Точный перенос ShowUserFlow логики (строки 576-631)
        /// </summary>
        public async UniTask StartUserFlowAsync(CancellationToken ct)
        {
            if (isShowingUserFlow)
            {
                Debug.LogWarning("ShowUserFlow already running, skipping duplicate call");
                return;
            }
            
            isShowingUserFlow = true;
            
            // Переход в состояние пользовательского потока
            stateManager.TransitionTo(IdentityState.UserFlowActive);
            
            try
            {
                // Не закрываем окно автоматически - пользователь может хотеть видеть профиль
                
                while (!ct.IsCancellationRequested)
                {
                    try
                    {
                        // Проверки состояния (из строк 587-595)
                        if (!ValidateUserFlowState(ct))
                            return;
                        
                        var viewManager = GetViewManager();
                        if (viewManager == null)
                        {
                            await WaitAndContinue(ct);
                            continue;
                        }

                        // Получение пользователя (строки 597-601)
                        Func<CancellationToken, UniTask<UserDto>> getUser = (ct) => userService
                            .GetUserAsync(ct);

                        var user = await getUser.WithErrorHandler(errorHandler.ShowErrorAsync, ct);

                        // Показ UserView (строки 603-606)
                        var result = await viewManager
                            .Show<UserView, UserViewParams, UserViewResult>(
                                new UserViewParams(user.Name), ct);

                        // Обработка действий пользователя (строки 608-618)
                        bool shouldContinue = await ProcessUserAction(result.Action, ct);
                        if (!shouldContinue)
                            return;
                    }
                    catch (OperationCanceledException)
                    {
                        if (debugLogging)
                            Debug.Log("ShowUserFlow cancelled");
                        return;
                    }
                    catch (SignOutRequiredException)
                    {
                        await identityService.SignOut(ct);
                        continue;
                    }
                }
            }
            finally
            {
                isShowingUserFlow = false;
            }
        }
        
        private async UniTask<bool> ProcessUserAction(UserAction action, CancellationToken ct)
        {
            switch (action)
            {
                case UserAction.OpenSettings:
                    await ShowSettingsAsync(ct);
                    return true;

                case UserAction.SignOut:
                    await ShowLoadingAsync(identityService.SignOut(ct), ct);
                    return false;
                    
                default:
                    return true;
            }
        }
        
        #endregion
        
        #region Settings Flow (точный перенос из строк 633-706)
        
        /// <summary>
        /// Показ настроек пользователя
        /// Точный перенос ShowSettings логики (строки 633-706)
        /// </summary>
        public async UniTask ShowSettingsAsync(CancellationToken ct)
        {
            // Переход в состояние настроек
            stateManager.TransitionTo(IdentityState.SettingsOpen);
            
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    var viewManager = GetViewManager();
                    if (viewManager == null)
                    {
                        if (debugLogging)
                            Debug.LogWarning("No ViewManager available for settings");
                        return;
                    }

                    // Получение данных пользователя (строки 645-649)
                    Func<CancellationToken, UniTask<UserDto>> getUser = (ct) => userService
                        .GetUserAsync(ct);

                    var user = await getUser.WithErrorHandler(errorHandler.ShowErrorAsync, ct);

                    // Показ SettingsView (строки 651-658)
                    var result = await viewManager
                        .Show<SettingsView, SettingsViewParams, SettingsViewResult>(
                            new SettingsViewParams(
                                user.Name,
                                user.Email,
                                user.AuthProviders.Contains("google.com"),
                                user.AuthProviders.Contains("apple.com"),
                                user.AuthProviders.Contains("telegram.org")), ct);

                    // Обработка действий в настройках (строки 660-703)
                    bool shouldContinue = await ProcessSettingsAction(result.Action, ct);
                    if (!shouldContinue)
                    {
                        // Возвращаемся к пользовательскому потоку
                        stateManager.TransitionTo(IdentityState.UserFlowActive);
                        return;
                    }
                }
                catch (OperationCanceledException)
                {
                    if (debugLogging)
                        Debug.Log("ShowSettings cancelled");
                    return;
                }
                catch (SignOutRequiredException)
                {
                    await identityService.SignOut(ct);
                    continue;
                }
            }
        }
        
        private async UniTask<bool> ProcessSettingsAction(SettingsAction action, CancellationToken ct)
        {
            switch (action)
            {
                case SettingsAction.ChangeName:
                    await ShowChangeNameAsync(ct);
                    return true;

                case SettingsAction.ChangeEmail:
                    await ShowChangeEmailAsync(ct);
                    return true;

                case SettingsAction.DeleteAccount:
                    await ShowDeleteAccountAsync(ct);
                    await identityService.SignOut(ct);
                    return false;

                case SettingsAction.AddGoogleProvider:
                    await ShowLoadingAsync(AddProviderAsync(SignInMethod.Google, ct), ct);
                    return true;

                case SettingsAction.AddAppleProvider:
                    await ShowLoadingAsync(AddProviderAsync(SignInMethod.Apple, ct), ct);
                    return true;

                case SettingsAction.AddTelegramProvider:
                    await ShowLoadingAsync(AddProviderAsync(SignInMethod.Telegram, ct), ct);
                    return true;

                case SettingsAction.Close:
                    return false;
                    
                default:
                    return true;
            }
        }
        
        #endregion
        
        #region Account Management Methods (точный перенос из строк 715-781)
        
        /// <summary>
        /// Изменение имени пользователя
        /// Точный перенос ShowChangeName (строки 715-723)
        /// </summary>
        public async UniTask ShowChangeNameAsync(CancellationToken ct)
        {
            var viewManager = GetViewManager();
            if (viewManager == null)
                throw new InvalidOperationException("No ViewManager available");

            var result = await viewManager.Show<ChangeNameView, ChangeNameViewParams, ChangeNameViewResult>(
                new ChangeNameViewParams(), ct);

            await userService.UpdateNameAsync(result.Name, ct);
        }
        
        /// <summary>
        /// Изменение email пользователя
        /// Точный перенос ShowChangeEmail (строки 725-758)
        /// </summary>
        public async UniTask ShowChangeEmailAsync(CancellationToken ct)
        {
            var viewManager = GetViewManager();
            if (viewManager == null)
                throw new InvalidOperationException("No ViewManager available");

            var emailResult = await viewManager.Show<ChangeEmailView, ChangeEmailViewParams, ChangeEmailViewResult>(
                new ChangeEmailViewParams(), ct);

            string email = emailResult.Email;
            string token = null;

            Func<CancellationToken, UniTask<string>> requestEmailChange = async (ct) =>
            {
                return await userService.RequestEmailChangeAsync(email, ct);
            };

            token = await requestEmailChange.WithErrorHandler(errorHandler.ShowErrorAsync, ct);

            Func<CancellationToken, UniTask> confirmEmailCode = async (ct) =>
            {
                string code = null;
                while (code == null)
                {
                    var result = await viewManager.Show<CodeView, CodeViewParams, CodeViewResult>(
                        new CodeViewParams(), ct);

                    if (result.Code == "RESEND")
                    {
                        // Request a new email change code with the same email
                        token = await userService.RequestEmailChangeAsync(email, ct);
                        continue;
                    }

                    code = result.Code;
                }

                await userService
                    .ConfirmEmailChangeAsync(token, code, ct);
            };

            await confirmEmailCode.WithErrorHandler(errorHandler.ShowErrorAsync, ct);
        }
        
        /// <summary>
        /// Удаление аккаунта пользователя
        /// Точный перенос ShowDeleteAccount (строки 760-781)
        /// </summary>
        public async UniTask ShowDeleteAccountAsync(CancellationToken ct)
        {
            var viewManager = GetViewManager();
            if (viewManager == null)
                throw new InvalidOperationException("No ViewManager available");

            await viewManager.Show<DeleteAccountView, DeleteAccountViewParams, DeleteAccountViewResult>(
                new DeleteAccountViewParams(), ct);

            string token = null;
            string code = null;

            while (code == null)
            {
                token = await userService.RequestDeleteAccountAsync(ct);

                var result = await viewManager.Show<CodeView, CodeViewParams, CodeViewResult>(
                    new CodeViewParams(), ct);

                if (result.Code == "RESEND")
                {
                    // Request a new deletion verification code
                    continue;
                }

                code = result.Code;
            }

            await userService.ConfirmDeleteAccountAsync(token, code, ct);
            await identityService.SignOut(ct);
        }
        
        /// <summary>
        /// Добавление провайдера аутентификации
        /// Логика из строк 677-701
        /// </summary>
        private async UniTask AddProviderAsync(SignInMethod method, CancellationToken ct)
        {
            switch (method)
            {
                case SignInMethod.Google:
                    // Упрощенная версия - убираем WithLoading пока не будет extension
                    await identityService.SignInWithGoogle(true, ct);
                    break;

                case SignInMethod.Apple:
                    // Упрощенная версия - убираем WithLoading пока не будет extension  
                    await identityService.SignInWithApple(true, ct);
                    break;

                case SignInMethod.Telegram:
                    // Упрощенная версия - убираем WithLoading пока не будет extension
                    await identityService.SignInWithTelegramAsync(true, ct);
                    break;
            }
        }
        
        #endregion
        
        #region Helpers
        
        private Views.Management.ViewManager GetViewManager()
        {
            return canvasManager.GetViewManager();
        }
        
        private bool ValidateUserFlowState(CancellationToken ct)
        {
            return !ct.IsCancellationRequested;
        }
        
        /// <summary>
        /// Показывает LoadingView во время выполнения async операции
        /// </summary>
        private async UniTask<T> ShowLoadingAsync<T>(UniTask<T> task, CancellationToken ct)
        {
            var viewManager = GetViewManager();
            if (viewManager == null)
                return await task;

            // Создаем CancellationTokenSource для управления LoadingView
            using var loadingCts = new CancellationTokenSource();
            
            // Показываем LoadingView с отдельным токеном (закроется когда токен отменится)
            var loadingParams = new LoadingViewParams(UniTask.CompletedTask);
            viewManager.Show<LoadingView, LoadingViewParams, LoadingViewResult>(loadingParams, loadingCts.Token).Forget();
            
            try
            {
                // Выполняем основную задачу
                var result = await task;
                return result;
            }
            finally
            {
                // Закрываем LoadingView отменой токена (в любом случае)
                loadingCts.Cancel();
            }
        }
        
        /// <summary>
        /// Показывает LoadingView во время выполнения async операции без результата
        /// </summary>
        private async UniTask ShowLoadingAsync(UniTask task, CancellationToken ct)
        {
            var viewManager = GetViewManager();
            if (viewManager == null)
            {
                await task;
                return;
            }

            // Создаем CancellationTokenSource для управления LoadingView
            using var loadingCts = new CancellationTokenSource();
            
            // Показываем LoadingView с отдельным токеном
            var loadingParams = new LoadingViewParams(UniTask.CompletedTask);
            viewManager.Show<LoadingView, LoadingViewParams, LoadingViewResult>(loadingParams, loadingCts.Token).Forget();
            
            try
            {
                // Выполняем основную задачу
                await task;
            }
            finally
            {
                // Закрываем LoadingView отменой токена (в любом случае)
                loadingCts.Cancel();
            }
        }
        
        private async UniTask WaitAndContinue(CancellationToken ct)
        {
            if (debugLogging)
                Debug.LogWarning("No ViewManager available, waiting...");
            await UniTask.Delay(1000, cancellationToken: ct);
        }
        
        #endregion
    }
}
