using System;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Energy8.Identity.UI.Runtime.Services;
using Energy8.Identity.UI.Runtime.Canvas;
using Energy8.Identity.UI.Runtime.State;
using Energy8.Identity.UI.Runtime.Flows;
using Energy8.Identity.UI.Runtime.DI;
using Energy8.Identity.UI.Runtime.Extensions;

#if UNITY_WEBGL && !UNITY_EDITOR
using Energy8.Identity.Auth.Core.Models;
#else
using Firebase.Auth;
#endif

namespace Energy8.Identity.UI.Runtime.Controllers
{
    /// <summary>
    /// Главный координатор Identity системы. 
    /// Singleton, живет весь lifecycle приложения.
    /// Только координирует между компонентами - никакой бизнес-логики!
    /// Точный перенос координационной логики из IdentityUIController
    /// </summary>
    public class IdentityOrchestrator : MonoBehaviour
    {
        public static IdentityOrchestrator Instance { get; private set; }
        
        [Header("Configuration")]
        [SerializeField] private bool isLite = false;
        [SerializeField] private bool debugLogging = false;
        
        // Менеджеры (инжектируются через DI)
        private ICanvasManager canvasManager;
        private IAuthFlowManager authFlowManager;
        private IUserFlowManager userFlowManager;
        private IStateManager stateManager;
        private IIdentityService identityService;
        private IServiceContainer serviceContainer;
        
        // Lifecycle management
        private CancellationTokenSource lifetimeCts;
        
        // События (публичный API - точный перенос из строк 59-60)
        public event Action OnSignedOut;
        public event Action OnSignedIn;
        
        // Публичные свойства (точный перенос API)
        public bool IsOpen => canvasManager?.IsOpen ?? false;
        public bool IsLite => isLite;
        public IdentityCanvasController CurrentCanvasController { get; private set; }
        
        #region Unity Lifecycle (точный перенос из строк 179-362)
        
        protected virtual void Awake()
        {
            // Точный перенос Singleton setup (строки 179-202)
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            // Устанавливаем Instance СРАЗУ, чтобы предотвратить создание дубликатов
            Instance = this;
            DontDestroyOnLoad(gameObject);

            lifetimeCts = new CancellationTokenSource();
            
            // Dependency injection setup
            InitializeServiceContainer();
            InjectDependencies();
            SubscribeToEvents();

#if UNITY_EDITOR
            // Подписываемся на событие остановки воспроизведения в редакторе
            UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#endif

            if (debugLogging)
                Debug.Log("IdentityOrchestrator initialized as singleton");
        }
        
        void Start()
        {
            // Автоматически найти и подключить CanvasController если есть
            FindAndRegisterCanvasController();
            
            // Точный перенос из Start (строки 223-225)
            StartIdentityFlow().Forget();
        }
        
        private void OnDestroy()
        {
            if (debugLogging)
                Debug.Log("IdentityOrchestrator OnDestroy started");

            // СНАЧАЛА отменяем все асинхронные операции
            if (lifetimeCts != null && !lifetimeCts.IsCancellationRequested)
            {
                lifetimeCts.Cancel();
            }

#if UNITY_EDITOR
            // Отписываемся от событий редактора
            try
            {
                UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            }
            catch (System.Exception ex)
            {
                if (debugLogging)
                    Debug.LogWarning($"Error unsubscribing from editor events: {ex.Message}");
            }
#endif

            // Отписываемся от событий
            UnsubscribeFromEvents();
            
            // Очищаем Instance если это текущий экземпляр
            if (Instance == this)
            {
                Instance = null;
            }

            // Очищаем токен отмены
            try
            {
                lifetimeCts?.Dispose();
            }
            catch (System.Exception ex)
            {
                if (debugLogging)
                    Debug.LogWarning($"Error disposing cancellation token: {ex.Message}");
            }
            finally
            {
                lifetimeCts = null;
            }

            // Очищаем WithLoading
            try
            {
                WithLoadingExtensions.CleanupLoading();
            }
            catch (System.Exception ex)
            {
                if (debugLogging)
                    Debug.LogWarning($"Error cleaning up WithLoading: {ex.Message}");
            }
            
            if (debugLogging)
                Debug.Log("IdentityOrchestrator OnDestroy completed");
        }

#if UNITY_EDITOR
        private void OnPlayModeStateChanged(UnityEditor.PlayModeStateChange state)
        {
            if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode)
            {
                if (debugLogging)
                    Debug.Log("Editor exiting play mode, forcing cleanup");
                
                ForceCleanup();
            }
        }

        private void ForceCleanup()
        {
            try
            {
                if (lifetimeCts != null && !lifetimeCts.IsCancellationRequested)
                {
                    lifetimeCts.Cancel();
                }

                UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
                UnsubscribeFromEvents();

                if (Instance == this)
                {
                    Instance = null;
                }

                if (lifetimeCts != null)
                {
                    lifetimeCts.Dispose();
                    lifetimeCts = null;
                }

                WithLoadingExtensions.CleanupLoading();
            }
            catch (System.Exception ex)
            {
                if (debugLogging)
                    Debug.LogWarning($"Error during force cleanup: {ex.Message}");
            }
        }
#endif
        
        #endregion
        
        #region Service Management
        
        private void InitializeServiceContainer()
        {
            serviceContainer = new IdentityServiceContainer();
            serviceContainer.ConfigureServices(debugLogging, isLite);
        }
        
        private void InjectDependencies()
        {
            canvasManager = serviceContainer.Resolve<ICanvasManager>();
            authFlowManager = serviceContainer.Resolve<IAuthFlowManager>();
            userFlowManager = serviceContainer.Resolve<IUserFlowManager>();
            stateManager = serviceContainer.Resolve<IStateManager>();
            identityService = serviceContainer.Resolve<IIdentityService>();
        }
        
        private void SubscribeToEvents()
        {
            stateManager.StateChanged += OnStateChanged;
        }
        
        private void UnsubscribeFromEvents()
        {
            if (stateManager != null)
            {
                stateManager.StateChanged -= OnStateChanged;
                stateManager.Dispose();
            }
        }
        
        #endregion
        
        #region State Coordination (простая координация между менеджерами)
        
        private void OnStateChanged(IdentityState oldState, IdentityState newState)
        {
            // Простая координация между менеджерами - никакой сложной логики!
            switch (newState)
            {
                case IdentityState.AuthenticationInProgress:
                    SetOpenState(true);
                    break;
                case IdentityState.SignedOut:
                    OnSignedOut?.Invoke();
                    if (lifetimeCts != null && !lifetimeCts.IsCancellationRequested)
                    {
                        authFlowManager.StartAuthFlowAsync(lifetimeCts.Token).Forget();
                    }
                    break;
                case IdentityState.SignedIn:
                    OnSignedIn?.Invoke();
                    if (lifetimeCts != null && !lifetimeCts.IsCancellationRequested)
                    {
                        userFlowManager.StartUserFlowAsync(lifetimeCts.Token).Forget();
                    }
                    break;
            }
        }
        
        private async UniTask StartIdentityFlow()
        {
            // Делегируем StateManager
            await stateManager.StartInitialFlowAsync();
        }
        
        #endregion
        
        #region Canvas Management API (перенос публичного API)
        
        /// <summary>
        /// Автоматически находит CanvasController в сцене и подключается к нему
        /// </summary>
        private void FindAndRegisterCanvasController()
        {
            // Если уже есть подключенный CanvasController, не ищем
            if (CurrentCanvasController != null)
                return;
            
            // Ищем CanvasController в сцене
            var canvasController = FindFirstObjectByType<IdentityCanvasController>();
            if (canvasController != null)
            {
                SetCanvasController(canvasController);
            }
        }
        
        /// <summary>
        /// Устанавливает Canvas контроллер для управления UI
        /// Точный перенос публичного API
        /// </summary>
        public void SetCanvasController(IdentityCanvasController canvasController)
        {
            CurrentCanvasController = canvasController;
            canvasManager?.SetCanvasController(canvasController);
        }

        /// <summary>
        /// Переключает состояние открытия/закрытия UI
        /// Точный перенос публичного API
        /// </summary>
        public void ToggleOpenState()
        {
            canvasManager?.ToggleOpenState();
        }

        /// <summary>
        /// Устанавливает состояние открытия/закрытия UI
        /// Точный перенос публичного API
        /// </summary>
        public void SetOpenState(bool isOpen)
        {
            canvasManager?.SetOpenState(isOpen);
        }
        
        /// <summary>
        /// Включает/отключает логирование токенов доступа для отладки
        /// </summary>
        public void EnableTokenLogging(bool enabled)
        {
            identityService?.EnableTokenLogging(enabled);
        }
        
        #endregion
        
        #region Editor Support
        
#if UNITY_EDITOR
        [ContextMenu("SignOut")]
        private void SignOut()
        {
            var authProvider = serviceContainer?.Resolve<Auth.Core.Providers.IAuthProvider>();
            authProvider?.SignOut();
        }

        [ContextMenu("Debug State")]
        private void DebugState()
        {
            Debug.Log($"IdentityOrchestrator State:\n" +
                      $"IsOpen: {IsOpen}\n" +
                      $"IsLite: {isLite}\n" +
                      $"Canvas Controller: {(CurrentCanvasController != null ? CurrentCanvasController.name : "null")}\n" +
                      $"Current State: {stateManager?.CurrentState}\n" +
                      $"Is Signed In: {identityService?.IsSignedIn ?? false}");
        }
#endif
        
        #endregion
    }
}
