using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Energy8.Identity.UI.Runtime.Services;
using Energy8.Identity.UI.Core.Management;
using Energy8.Identity.UI.Core;

#if UNITY_WEBGL && !UNITY_EDITOR
using Energy8.Identity.Auth.Core.Models;
#else
using Firebase.Auth;
#endif

namespace Energy8.Identity.UI.Runtime.State
{
    /// <summary>
    /// Централизованное управление состоянием Identity системы.
    /// Заменяет разбросанные булевые флаги четкой State Machine.
    /// Перенос логики из IdentityUIController строки 55-57, 397-456, 148-177
    /// </summary>
    public class StateManager : IStateManager
    {
        private readonly IIdentityService identityService;
        private readonly IAnalyticsPermissionService analyticsPermissionService;

        public IdentityState CurrentState { get; private set; } = IdentityState.Uninitialized;
        public event Action<IdentityState, IdentityState> StateChanged;

        // Allowed state transitions for validation
        private readonly Dictionary<IdentityState, List<IdentityState>> allowedTransitions = new()
        {
            [IdentityState.Uninitialized] = new() { IdentityState.Initializing },
            [IdentityState.Initializing] = new() { IdentityState.PreAuthentication, IdentityState.Error },
            [IdentityState.PreAuthentication] = new() { IdentityState.AuthCheck, IdentityState.Error, IdentityState.PreAuthentication },
            [IdentityState.AuthCheck] = new() { IdentityState.SignedIn, IdentityState.SignedOut, IdentityState.Error },
            [IdentityState.SignedOut] = new() { IdentityState.AuthFlowActive, IdentityState.SignedIn, IdentityState.Error },
            [IdentityState.SignedIn] = new() { IdentityState.UserFlowActive, IdentityState.SignedOut, IdentityState.Error },
            [IdentityState.UserFlowActive] = new() { IdentityState.SettingsOpen, IdentityState.SignedOut, IdentityState.SignedIn, IdentityState.Error },
            [IdentityState.AuthFlowActive] = new() { IdentityState.SignedIn, IdentityState.SignedOut, IdentityState.Error },
            [IdentityState.SettingsOpen] = new() { IdentityState.UserFlowActive, IdentityState.SignedOut, IdentityState.Error },
            [IdentityState.Error] = new() { IdentityState.Uninitialized, IdentityState.Initializing }
        };

        public StateManager(IIdentityService identityService, IAnalyticsPermissionService analyticsPermissionService)
        {
            this.identityService = identityService;
            this.analyticsPermissionService = analyticsPermissionService;

            identityService.OnSignedIn += OnUserSignedIn;
            identityService.OnSignedOut += OnUserSignedOut;
        }

        /// <summary>
        /// Запускает начальный поток системы в зависимости от состояния аутентификации
        /// </summary>
        public async UniTask StartInitialFlowAsync()
        {
            try
            {
                TransitionTo(IdentityState.Initializing);
                // Проверяем разрешение на аналитику при старте
                if (analyticsPermissionService.ShouldShowAnalyticsPermissionRequest)
                {
                    await analyticsPermissionService.RequestAnalyticsPermissionAsync(default);
                }
                // Инициализируем сервис аутентификации
                await identityService.Initialize(default);
                // Не делаем автоматических переходов в SignedIn/SignedOut/UserFlowActive
            }
            catch (Exception ex)
            {
                TransitionTo(IdentityState.Error);
            }
        }

        #region State Machine

        public bool CanTransitionTo(IdentityState newState)
        {
            if (!allowedTransitions.ContainsKey(CurrentState))
                return false;

            return allowedTransitions[CurrentState].Contains(newState);
        }

        public void TransitionTo(IdentityState newState)
        {
            if (!CanTransitionTo(newState))
            {
                var errorMsg = $"Invalid state transition from {CurrentState} to {newState}";
                throw new InvalidOperationException(errorMsg);
            }

            var oldState = CurrentState;
            CurrentState = newState;

            StateChanged?.Invoke(oldState, newState);
        }

        #endregion

        #region Event Handlers (перенос из строк 148-177)

        private void OnUserSignedIn(FirebaseUser user)
        {
            // Если уже в состоянии UserFlowActive, не переходить обратно в SignedIn
            if (CurrentState == IdentityState.UserFlowActive)
            {
                return;
            }

            // Переходим в SignedIn из любого состояния, где это разрешено (включая Initializing)
            if (CanTransitionTo(IdentityState.SignedIn))
            {
                TransitionTo(IdentityState.SignedIn);
            }
        }

        private void OnUserSignedOut()
        {
            if (CanTransitionTo(IdentityState.SignedOut))
            {
                TransitionTo(IdentityState.SignedOut);
                // Здесь всегда открываем Identity (например, вызываем UI Coordinator или событие для показа окна)
            }
        }

        #endregion

        #region Public API

        #endregion

        #region IDisposable

        public void Dispose()
        {
            if (identityService != null)
            {
                identityService.OnSignedIn -= OnUserSignedIn;
                identityService.OnSignedOut -= OnUserSignedOut;
            }
        }

        #endregion
    }
}
