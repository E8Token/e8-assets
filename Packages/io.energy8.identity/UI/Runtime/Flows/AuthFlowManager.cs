using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Energy8.Identity.UI.Runtime.Services;
using Energy8.Identity.UI.Runtime.Canvas;
using Energy8.Identity.UI.Runtime.State;
using Energy8.Identity.UI.Runtime.Error;
using Energy8.Identity.UI.Runtime.Views.Implementations;
using Energy8.Identity.UI.Runtime.Views.Models;
using Energy8.Identity.UI.Runtime.Extensions;
using Energy8.Identity.Shared.Core.Exceptions;
using Energy8.Identity.Shared.Core.Error;

#if UNITY_WEBGL && !UNITY_EDITOR
using Energy8.Identity.Auth.Core.Models;
#else
using Firebase.Auth;
#endif

namespace Energy8.Identity.UI.Runtime.Flows
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
        private readonly bool debugLogging;
        
        private bool isShowingAuthFlow = false; // Перенос флага из строки 56
        
        public AuthFlowManager(
            IIdentityService identityService,
            ICanvasManager canvasManager,
            IStateManager stateManager,
            IErrorHandler errorHandler,
            bool debugLogging)
        {
            this.identityService = identityService;
            this.canvasManager = canvasManager;
            this.stateManager = stateManager;
            this.errorHandler = errorHandler;
            this.debugLogging = debugLogging;
        }
        
        #region Main Auth Flow (точный перенос из строк 457-574)
        
        /// <summary>
        /// Запуск основного потока авторизации
        /// Точный перенос ShowAuthFlow (строки 457-574)
        /// </summary>
        public async UniTask StartAuthFlowAsync(CancellationToken ct)
        {
            if (debugLogging)
                Debug.Log("[AuthFlowManager] Starting auth flow");
                
            if (isShowingAuthFlow)
            {
                Debug.LogWarning("[AuthFlowManager] ShowAuthFlow already running, skipping duplicate call");
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
                        if (debugLogging)
                            Debug.Log("ShowAuthFlow cancelled");
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
                Debug.Log($"[AuthFlowManager] Starting email flow for: {email}");
                
                // Запуск email flow с Loading
                await ShowLoadingAsync(identityService.StartEmailFlow(email, ct), ct);
                
                Debug.Log($"[AuthFlowManager] Email flow started, now showing code input");

                // Подтверждение кода
                string code = null;
                string emailForCode = email;

                while (code == null)
                {
                    Debug.Log($"[AuthFlowManager] Showing CodeView");
                    
                    var codeResult = await GetViewManager().Show<CodeView, CodeViewParams, CodeViewResult>(
                        new CodeViewParams(), ct);

                    Debug.Log($"[AuthFlowManager] CodeView result: {codeResult.Code}");

                    if (codeResult.Code == "RESEND")
                    {
                        Debug.Log($"[AuthFlowManager] Resending email flow");
                        await ShowLoadingAsync(identityService.StartEmailFlow(emailForCode, ct), ct);
                        continue;
                    }

                    code = codeResult.Code;
                }

                Debug.Log($"[AuthFlowManager] Confirming email code");
                await ShowLoadingAsync(identityService.ConfirmEmailCode(code, ct), ct);
                Debug.Log($"[AuthFlowManager] Email auth completed successfully");
            }
            catch (Energy8Exception e8Exception)
            {
                Debug.LogError($"[AuthFlowManager] Email auth failed: {e8Exception.Message}");
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
        
        private Views.Management.ViewManager GetViewManager()
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
            Debug.Log($"[AuthFlowManager] ShowLoadingAsync<T> called");
            
            var viewManager = GetViewManager();
            if (viewManager == null)
            {
                Debug.LogWarning($"[AuthFlowManager] ViewManager is null, executing task without loading");
                return await task;
            }

            Debug.Log($"[AuthFlowManager] Creating LoadingView");
            
            // Создаем CancellationTokenSource для управления LoadingView
            using var loadingCts = new CancellationTokenSource();
            
            // Показываем LoadingView с отдельным токеном (закроется когда токен отменится)
            var loadingParams = new LoadingViewParams(UniTask.CompletedTask);
            viewManager.Show<LoadingView, LoadingViewParams, LoadingViewResult>(loadingParams, loadingCts.Token).Forget();
            
            Debug.Log($"[AuthFlowManager] LoadingView started, executing main task");
            
            try
            {
                // Выполняем основную задачу
                var result = await task;
                Debug.Log($"[AuthFlowManager] Main task completed successfully");
                return result;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AuthFlowManager] Main task failed: {ex.Message}");
                throw;
            }
            finally
            {
                // Закрываем LoadingView отменой токена (в любом случае)
                Debug.Log($"[AuthFlowManager] Closing LoadingView");
                loadingCts.Cancel();
            }
        }
        
        /// <summary>
        /// Показывает LoadingView во время выполнения async операции без результата
        /// </summary>
        private async UniTask ShowLoadingAsync(UniTask task, CancellationToken ct)
        {
            Debug.Log($"[AuthFlowManager] ShowLoadingAsync (no result) called");
            
            var viewManager = GetViewManager();
            if (viewManager == null)
            {
                Debug.LogWarning($"[AuthFlowManager] ViewManager is null, executing task without loading");
                await task;
                return;
            }

            Debug.Log($"[AuthFlowManager] Creating LoadingView (no result)");
            
            // Создаем CancellationTokenSource для управления LoadingView
            using var loadingCts = new CancellationTokenSource();
            
            // Показываем LoadingView с отдельным токеном
            var loadingParams = new LoadingViewParams(UniTask.CompletedTask);
            viewManager.Show<LoadingView, LoadingViewParams, LoadingViewResult>(loadingParams, loadingCts.Token).Forget();
            
            Debug.Log($"[AuthFlowManager] LoadingView started, executing main task (no result)");
            
            try
            {
                // Выполняем основную задачу
                await task;
                Debug.Log($"[AuthFlowManager] Main task completed successfully (no result)");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AuthFlowManager] Main task failed (no result): {ex.Message}");
                throw;
            }
            finally
            {
                // Закрываем LoadingView отменой токена (в любом случае)
                Debug.Log($"[AuthFlowManager] Closing LoadingView (no result)");
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
