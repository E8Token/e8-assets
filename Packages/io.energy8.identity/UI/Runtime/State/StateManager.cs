using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

#if UNITY_WEBGL && !UNITY_EDITOR
using Energy8.Identity.Auth.Core.Models;
#else
using Firebase.Auth;
#endif

using Energy8.Identity.UI.Runtime.Services;

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
        private readonly bool debugLogging;
        
        public IdentityState CurrentState { get; private set; } = IdentityState.Uninitialized;
        public event Action<IdentityState, IdentityState> StateChanged;
        
        // Allowed state transitions for validation
        private readonly Dictionary<IdentityState, List<IdentityState>> allowedTransitions = new()
        {
            [IdentityState.Uninitialized] = new() { IdentityState.Initializing },
            [IdentityState.Initializing] = new() { IdentityState.SignedOut, IdentityState.SignedIn, IdentityState.Error },
            [IdentityState.SignedOut] = new() { IdentityState.AuthenticationInProgress },
            [IdentityState.AuthenticationInProgress] = new() { 
                IdentityState.SignedIn, IdentityState.SignedOut, IdentityState.Error 
            },
            [IdentityState.SignedIn] = new() { IdentityState.UserFlowActive, IdentityState.SignedOut },
            [IdentityState.UserFlowActive] = new() { 
                IdentityState.SettingsOpen, IdentityState.SignedOut, IdentityState.SignedIn 
            },
            [IdentityState.SettingsOpen] = new() { 
                IdentityState.UserFlowActive, IdentityState.SignedOut 
            },
            [IdentityState.Error] = new() { 
                IdentityState.SignedOut, IdentityState.SignedIn, IdentityState.Initializing 
            }
        };
        
        public StateManager(IIdentityService identityService, bool debugLogging)
        {
            this.identityService = identityService;
            this.debugLogging = debugLogging;
            
            // Subscribe to identity service events (перенос из строк 148-177)
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
                
                // Инициализируем сервис аутентификации
                await identityService.Initialize(default);
                
                // Проверяем состояние аутентификации и переходим в соответствующее состояние
                if (identityService.IsSignedIn)
                {
                    TransitionTo(IdentityState.SignedIn);
                }
                else
                {
                    TransitionTo(IdentityState.SignedOut);
                }
            }
            catch (Exception ex)
            {
                if (debugLogging)
                    Debug.LogError($"Error during initial flow: {ex.Message}");
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
                if (debugLogging)
                    Debug.LogError(errorMsg);
                throw new InvalidOperationException(errorMsg);
            }
            
            var oldState = CurrentState;
            CurrentState = newState;
            
            if (debugLogging)
                Debug.Log($"State transition: {oldState} → {newState}");
            
            StateChanged?.Invoke(oldState, newState);
        }
        
        #endregion
        
        #region Event Handlers (перенос из строк 148-177)
        
#if UNITY_WEBGL && !UNITY_EDITOR
        private void OnUserSignedIn(string userId)
#else
        private void OnUserSignedIn(FirebaseUser user)
#endif
        {
            // Точный перенос из OnUserSignedIn (строки 148-162)
            if (debugLogging)
                Debug.Log("User signed in event received, transitioning to SignedIn state");
                
            if (CanTransitionTo(IdentityState.SignedIn))
            {
                TransitionTo(IdentityState.SignedIn);
            }
        }
        
        private void OnUserSignedOut()
        {
            // Точный перенос из OnUserSignedOut (строки 164-177)
            if (debugLogging)
                Debug.Log("User signed out event received, transitioning to SignedOut state");
                
            if (CanTransitionTo(IdentityState.SignedOut))
            {
                TransitionTo(IdentityState.SignedOut);
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
