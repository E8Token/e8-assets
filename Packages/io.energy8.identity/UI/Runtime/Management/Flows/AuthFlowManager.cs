using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Energy8.Identity.UI.Runtime.Services;
using Energy8.Identity.UI.Runtime.Views.Implementations;
using Energy8.Identity.UI.Core.Views.Models;
using Energy8.Identity.Shared.Core.Exceptions;
using Energy8.Identity.Shared.Core.Error;
using Energy8.Identity.UI.Core.Management;
using Energy8.Identity.UI.Core;



#if UNITY_WEBGL && !UNITY_EDITOR
using Energy8.Identity.Auth.Core.Models;
#endif

namespace Energy8.Identity.UI.Runtime.Management.Flows
{
    /// <summary>
    /// Управляет всеми авторизационными потоками.
    /// Email, Google, Apple, Telegram authentication.
    /// Точный перенос ShowAuthFlow логики (строки 457-574)
    /// </summary>
    public class AuthFlowManager : IAuthFlowManager
    {
        private readonly IIdentityService identityService;
        private readonly ICanvasManager canvasManager;
        private readonly IStateManager stateManager;
        private readonly IErrorHandler errorHandler;
        
        private bool isShowingAuthFlow = false; // Перенос флага из строки 56
        
        public AuthFlowManager(
            IIdentityService identityService,
            ICanvasManager canvasManager,
            IStateManager stateManager,
            IErrorHandler errorHandler)
        {
            this.identityService = identityService;
            this.canvasManager = canvasManager;
            this.stateManager = stateManager;
            this.errorHandler = errorHandler;
        }
        
        #region Main Auth Flow (точный перенос из строк 457-574)
        
        /// <summary>
        /// Запуск основного потока авторизации
        /// Точный перенос ShowAuthFlow (строки 457-574)
        /// </summary>
        public async UniTask StartAuthFlowAsync(CancellationToken ct)
        {
            if (isShowingAuthFlow)
            {
                return;
            }
            
            isShowingAuthFlow = true;
            
            // Переход в состояние авторизации
            stateManager.TransitionTo(IdentityState.AuthFlowActive);
            
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    try
                    {
                        // Проверки состояния (из строк 465-473)
                        if (!ValidateAuthFlowState(ct))
                            return;
                        
                        var viewManager = GetViewManager();
                        if (viewManager == null)
                        {
                            Debug.LogError("[AuthFlowManager] ViewManager is null!");
                            await WaitAndContinue(ct);
                            continue;
                        }

                        canvasManager.SetOpenState(true);

                        var result = await viewManager
                            .Show<SignInView, SignInViewParams, SignInViewResult>(
                                new SignInViewParams(), ct);

                        // Обработка методов авторизации (строки 504-561)
                        await ProcessSignInMethod(result, ct);

                        return;
                    }
                    catch (OperationCanceledException)
                    {
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
                isShowingAuthFlow = false;
            }
        }
        
        #endregion
        
        #region Authentication Methods (точный перенос из строк 504-561)
        
        private async UniTask ProcessSignInMethod(SignInViewResult result, CancellationToken ct)
        {
            switch (result.Method)
            {
                case SignInMethod.Email:
                    await HandleEmailAuth(result.Email, ct);
                    break;

                case SignInMethod.Google:
                    await HandleGoogleAuth(ct);
                    break;

                case SignInMethod.Apple:
                    await HandleAppleAuth(ct);
                    break;

                case SignInMethod.Telegram:
                    await HandleTelegramAuth(ct);
                    break;
            }
        }
        
        /// <summary>
        /// Обработка авторизации по email
        /// Упрощенный перенос Email flow логики (строки 504-537)
        /// </summary>
        private async UniTask HandleEmailAuth(string email, CancellationToken ct)
        {
            // Упрощенная версия без WithErrorHandler пока не исправим все зависимости
            try
            {
                // Запуск email flow с Loading
                await ShowLoadingAsync(identityService.StartEmailFlow(email, ct), ct);

                // Подтверждение кода
                string code = null;
                string emailForCode = email;

                while (code == null)
                {
                    var codeResult = await GetViewManager().Show<CodeView, CodeViewParams, CodeViewResult>(
                        new CodeViewParams(), ct);

                    if (codeResult.Code == "RESEND")
                    {
                        await ShowLoadingAsync(identityService.StartEmailFlow(emailForCode, ct), ct);
                        continue;
                    }

                    code = codeResult.Code;
                }

                await ShowLoadingAsync(identityService.ConfirmEmailCode(code, ct), ct);
            }
            catch (Energy8Exception e8Exception)
            {
                await errorHandler.ShowErrorAsync(e8Exception, ct);
            }
        }
        
        /// <summary>
        /// Обработка Google авторизации
        /// Упрощенный перенос Google auth (строки 539-544)
        /// </summary>
        private async UniTask HandleGoogleAuth(CancellationToken ct)
        {
            try
            {
                await ShowLoadingAsync(identityService.SignInWithGoogle(false, ct), ct);
            }
            catch (Energy8Exception e8Exception)
            {
                await errorHandler.ShowErrorAsync(e8Exception, ct);
            }
        }
        
        /// <summary>
        /// Обработка Apple авторизации
        /// Упрощенный перенос Apple auth (строки 546-551)
        /// </summary>
        private async UniTask HandleAppleAuth(CancellationToken ct)
        {
            try
            {
                await ShowLoadingAsync(identityService.SignInWithApple(false, ct), ct);
            }
            catch (Energy8Exception e8Exception)
            {
                await errorHandler.ShowErrorAsync(e8Exception, ct);
            }
        }
        
        /// <summary>
        /// Обработка Telegram авторизации
        /// Упрощенный перенос Telegram auth (строки 553-559)
        /// </summary>
        private async UniTask HandleTelegramAuth(CancellationToken ct)
        {
            try
            {
                await ShowLoadingAsync(identityService.SignInWithTelegramAsync(false, ct), ct);
            }
            catch (Energy8Exception e8Exception)
            {
                await errorHandler.ShowErrorAsync(e8Exception, ct);
            }
        }
        
        #endregion
        
        #region Helpers
        
        private IViewManager GetViewManager()
        {
            return canvasManager.GetViewManager();
        }
        
        private bool ValidateAuthFlowState(CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
                return false;
                
            // Дополнительные проверки состояния можно добавить здесь
            return stateManager.CurrentState == IdentityState.AuthFlowActive;
        }
        
        /// <summary>
        /// Показывает LoadingView во время выполнения async операции
        /// </summary>
        private async UniTask<T> ShowLoadingAsync<T>(UniTask<T> task, CancellationToken ct)
        {
            var viewManager = GetViewManager();
            if (viewManager == null)
            {
                return await task;
            }

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
            await UniTask.Delay(1000, cancellationToken: ct);
        }
        
        #endregion
    }
}
