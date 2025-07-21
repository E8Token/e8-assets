using System;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Energy8.Identity.UI.Runtime.Services;
using Energy8.Identity.UI.Runtime.Canvas;
using Energy8.Identity.UI.Runtime.State;
using Energy8.Identity.UI.Runtime.Management.Flows;
using Energy8.Identity.UI.Runtime.DI;
using Energy8.Identity.UI.Runtime.Extensions;
using Energy8.Identity.UI.Core;
using Energy8.Identity.UI.Core.Management;



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

            // После DI: если StateManager в Uninitialized, переводим в Initializing
            if (stateManager != null && stateManager.CurrentState == IdentityState.Uninitialized)
            {
                stateManager.TransitionTo(IdentityState.Initializing);
            }

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

            stateManager.TransitionTo(IdentityState.PreAuthentication);
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
            switch (newState)
            {
                case IdentityState.PreAuthentication:
                    // 1. Проверка обновления
                    var updateService = serviceContainer.Resolve<IUpdateService>();
                    var updateFlowManager = serviceContainer.Resolve<IUpdateFlowManager>();
                    if (updateService != null && updateService.HasUpdate && updateFlowManager != null)
                    {
                        updateFlowManager.ShowUpdateFlowAsync(lifetimeCts.Token).ContinueWith(() =>
                        {
                            stateManager.TransitionTo(IdentityState.PreAuthentication);
                        }).Forget();
                        break;
                    }
                    // 2. Проверка аналитики
                    var analyticsPermissionService = serviceContainer.Resolve<IAnalyticsPermissionService>();
                    if (analyticsPermissionService != null && analyticsPermissionService.ShouldShowAnalyticsPermissionRequest)
                    {
                        analyticsPermissionService.RequestAnalyticsPermissionAsync(lifetimeCts.Token).ContinueWith(_ =>
                        {
                            stateManager.TransitionTo(IdentityState.PreAuthentication);
                        }).Forget();
                        break;
                    }
                    stateManager.TransitionTo(IdentityState.AuthCheck);
                    identityService.Initialize(lifetimeCts.Token).Forget();
                    break;
                case IdentityState.AuthCheck:
                    break;
                case IdentityState.SignedOut:
                    OnSignedOut?.Invoke();
                    if (authFlowManager != null)
                        authFlowManager.StartAuthFlowAsync(lifetimeCts.Token).Forget();
                    break;
                case IdentityState.AuthFlowActive:
                    if (authFlowManager != null)
                        authFlowManager.StartAuthFlowAsync(lifetimeCts.Token).Forget();
                    break;
                case IdentityState.SignedIn:
                    OnSignedIn?.Invoke();
                    if (userFlowManager != null)
                        userFlowManager.StartUserFlowAsync(lifetimeCts.Token).Forget();
                    break;
                case IdentityState.UserFlowActive:
                    // UserFlowManager сам управляет UI, здесь ничего не делаем
                    break;
                case IdentityState.Error:
                    Debug.LogError("[IdentityOrchestrator] State machine entered Error state!");
                    break;
            }
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
