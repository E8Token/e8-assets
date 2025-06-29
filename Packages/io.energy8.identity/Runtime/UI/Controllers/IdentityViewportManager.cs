using System;
using UnityEngine;
using Energy8.ViewportManager.Components;
using Energy8.ViewportManager.Core;
using Energy8.ViewportManager.Configuration;
using Energy8.Identity.Core.Logging;
using VMScreenOrientation = Energy8.ViewportManager.Core.ScreenOrientation;

namespace Energy8.Identity.Runtime.UI.Controllers
{
    /// <summary>
    /// Менеджер Identity UI, который автоматически создает и управляет единственным контроллером
    /// в зависимости от ориентации экрана, пересоздавая его из соответствующих префабов.
    /// </summary>
    public class IdentityViewportManager : ViewportEventListener
    {
        [Header("Identity Prefabs")]
        [SerializeField] private GameObject portraitControllerPrefab;
        [SerializeField] private GameObject landscapeControllerPrefab;
        
        [Header("Controller Settings")]
        [SerializeField] private string controllerName = "IdentityController";
        [SerializeField] private bool autoCreateOnStart = true;
        
        [Header("State Management")]
        [SerializeField] private bool preserveState = true;
        
        [Header("Debug")]
        [SerializeField] private bool debugLogging = false;

        private readonly ILogger<IdentityViewportManager> logger = new Logger<IdentityViewportManager>();
        private IdentityUIController currentController;
        private IdentityControllerState savedState;
        private bool isTransitioning = false;
        private VMScreenOrientation lastOrientation;

        /// <summary>
        /// Структура для сохранения состояния Identity контроллера
        /// </summary>
        [Serializable]
        private class IdentityControllerState
        {
            public bool isOpen;
            public bool isCurrentlyLite;
            public Vector2 lastScreenSize;
            public float animationDuration;

            public IdentityControllerState()
            {
                isOpen = false;
                isCurrentlyLite = false;
                lastScreenSize = Vector2.zero;
                animationDuration = 0.5f;
            }
        }

        #region Unity Lifecycle

        protected override void Awake()
        {
            base.Awake();
            
            // Инициализируем состояние
            savedState = new IdentityControllerState();
            
            // Проверяем префабы
            ValidatePrefabs();
            
            if (debugLogging)
                logger.LogInfo("IdentityViewportManager initialized");
        }

        protected override void Start()
        {
            base.Start();
            
            // Создание контроллера теперь происходит в OnManagerInitialized
        }

        #endregion

        #region ViewportEventListener Overrides

        protected override void OnInitialSetup(ViewportContext initialContext, ViewportConfiguration initialConfiguration)
        {
            if (debugLogging)
                logger.LogInfo($"Initial setup with context: {initialContext}");
                
            lastOrientation = initialContext.orientation;
            EnsureCorrectController(initialContext.orientation);
        }

        protected override void OnManagerInitialized()
        {
            if (debugLogging)
                logger.LogInfo("ViewportManager initialized, creating initial controller");
            
            if (autoCreateOnStart)
            {
                CreateInitialController();
            }
        }

        protected override void OnOrientationChanged(VMScreenOrientation fromOrientation, VMScreenOrientation toOrientation) 
        {
            if (debugLogging)
                logger.LogInfo($"Orientation changed: {fromOrientation} → {toOrientation}");
                
            if (isTransitioning) return;
            
            lastOrientation = toOrientation;
            EnsureCorrectController(toOrientation);
        }

        protected override void OnContextChanged(ViewportContext previousContext, ViewportContext newContext)
        {
            if (debugLogging)
                logger.LogInfo($"Context changed: {previousContext} → {newContext}");
                
            if (isTransitioning) return;
            
            lastOrientation = newContext.orientation;
            EnsureCorrectController(newContext.orientation);
        }

        protected override void OnConfigurationChanged(ViewportConfiguration previousConfiguration, ViewportConfiguration newConfiguration)
        {
            if (debugLogging)
                logger.LogInfo($"Configuration changed: {previousConfiguration?.ToString()} → {newConfiguration?.ToString()}");
                
            // Применяем новую конфигурацию к текущему контроллеру
            ApplyConfigurationToController(newConfiguration);
        }

        #endregion

        #region Controller Management

        /// <summary>
        /// Создает начальный контроллер на основе текущей ориентации
        /// </summary>
        private void CreateInitialController()
        {
            VMScreenOrientation orientation;
            
            if (Energy8.ViewportManager.ViewportManager.IsInitialized)
            {
                orientation = Energy8.ViewportManager.ViewportManager.CurrentContext.orientation;
            }
            else
            {
                // Fallback: определяем по размеру экрана
                orientation = Screen.width < Screen.height ? VMScreenOrientation.Portrait : VMScreenOrientation.Landscape;
            }
            
            lastOrientation = orientation;
            CreateController(orientation);
        }

        /// <summary>
        /// Убеждается, что текущий контроллер соответствует ориентации
        /// </summary>
        private void EnsureCorrectController(VMScreenOrientation orientation)
        {
            bool needsPortrait = IsPortraitOrientation(orientation);
            bool hasPortrait = IsCurrentControllerPortrait();
            
            // Если контроллера нет или он не соответствует ориентации
            if (currentController == null || needsPortrait != hasPortrait)
            {
                RecreateController(orientation);
            }
        }

        /// <summary>
        /// Пересоздает контроллер для указанной ориентации
        /// </summary>
        private void RecreateController(VMScreenOrientation orientation)
        {
            if (isTransitioning) return;
            
            isTransitioning = true;
            
            try
            {
                // Сохраняем состояние текущего контроллера
                if (currentController != null && preserveState)
                {
                    SaveControllerState();
                }
                
                // Уничтожаем текущий контроллер
                DestroyCurrentController();
                
                // Создаем новый контроллер
                CreateController(orientation);
                
                // Восстанавливаем состояние
                if (preserveState && currentController != null)
                {
                    RestoreControllerState();
                }
                
                if (debugLogging)
                    logger.LogInfo($"Recreated controller for orientation: {orientation}");
            }
            finally
            {
                isTransitioning = false;
            }
        }

        /// <summary>
        /// Создает контроллер для указанной ориентации
        /// </summary>
        private void CreateController(VMScreenOrientation orientation)
        {
            GameObject prefab = IsPortraitOrientation(orientation) ? portraitControllerPrefab : landscapeControllerPrefab;
            
            if (prefab == null)
            {
                logger.LogError($"No prefab assigned for orientation: {orientation}");
                return;
            }
            
            // Проверяем, что префаб содержит IdentityUIController
            if (prefab.GetComponent<IdentityUIController>() == null)
            {
                logger.LogError($"Prefab {prefab.name} does not contain IdentityUIController component!");
                return;
            }
            
            // Создаем контроллер
            GameObject controllerGO = Instantiate(prefab);
            controllerGO.name = controllerName;
            
            // Получаем компонент контроллера
            currentController = controllerGO.GetComponent<IdentityUIController>();
                        
            if (debugLogging)
                logger.LogInfo($"Created controller: {controllerGO.name} from prefab: {prefab.name}");
        }

        /// <summary>
        /// Уничтожает текущий контроллер
        /// </summary>
        private void DestroyCurrentController()
        {
            if (currentController != null)
            {
                // Закрываем UI перед уничтожением
                if (currentController.IsOpen)
                {
                    currentController.SetOpenState(false);
                }
                
                string controllerName = currentController.name;
                
                // Уничтожаем GameObject
                if (Application.isPlaying)
                {
                    Destroy(currentController.gameObject);
                }
                else
                {
                    DestroyImmediate(currentController.gameObject);
                }
                
                currentController = null;
                
                if (debugLogging)
                    logger.LogInfo($"Destroyed controller: {controllerName}");
            }
        }

        #endregion

        #region State Management

        /// <summary>
        /// Сохраняет состояние текущего контроллера
        /// </summary>
        private void SaveControllerState()
        {
            if (currentController == null) return;
            
            savedState.isOpen = currentController.IsOpen;
            savedState.lastScreenSize = new Vector2(Screen.width, Screen.height);
            
            // Используем рефлексию для доступа к приватным полям
            var controllerType = currentController.GetType();
            
            try
            {
                var animationDurationField = controllerType.GetField("animationDuration", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (animationDurationField != null)
                {
                    savedState.animationDuration = (float)animationDurationField.GetValue(currentController);
                }
                
                var isCurrentlyLiteField = controllerType.GetField("isCurrentlyLite", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (isCurrentlyLiteField != null)
                {
                    savedState.isCurrentlyLite = (bool)isCurrentlyLiteField.GetValue(currentController);
                }
            }
            catch (Exception ex)
            {
                if (debugLogging)
                    logger.LogWarning($"Failed to save some controller state via reflection: {ex.Message}");
            }
            
            if (debugLogging)
                logger.LogInfo($"Saved controller state: IsOpen={savedState.isOpen}, IsLite={savedState.isCurrentlyLite}");
        }

        /// <summary>
        /// Восстанавливает состояние контроллера
        /// </summary>
        private void RestoreControllerState()
        {
            if (currentController == null || savedState == null) return;
            
            // Восстанавливаем состояние открытия/закрытия
            if (currentController.IsOpen != savedState.isOpen)
            {
                currentController.SetOpenState(savedState.isOpen);
            }
            
            // Используем рефлексию для восстановления приватных полей
            var controllerType = currentController.GetType();
            
            try
            {
                var animationDurationField = controllerType.GetField("animationDuration", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (animationDurationField != null)
                {
                    animationDurationField.SetValue(currentController, savedState.animationDuration);
                }
                
                var isCurrentlyLiteField = controllerType.GetField("isCurrentlyLite", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (isCurrentlyLiteField != null)
                {
                    isCurrentlyLiteField.SetValue(currentController, savedState.isCurrentlyLite);
                }
                
                var lastScreenSizeField = controllerType.GetField("lastScreenSize", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (lastScreenSizeField != null)
                {
                    lastScreenSizeField.SetValue(currentController, savedState.lastScreenSize);
                }
            }
            catch (Exception ex)
            {
                if (debugLogging)
                    logger.LogWarning($"Failed to restore some controller state via reflection: {ex.Message}");
            }
            
            if (debugLogging)
                logger.LogInfo($"Restored controller state: IsOpen={savedState.isOpen}, IsLite={savedState.isCurrentlyLite}");
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Проверяет, является ли ориентация портретной
        /// </summary>
        private bool IsPortraitOrientation(VMScreenOrientation orientation)
        {
            return orientation == VMScreenOrientation.Portrait;
        }

        /// <summary>
        /// Проверяет, является ли текущий контроллер портретным
        /// </summary>
        private bool IsCurrentControllerPortrait()
        {
            if (currentController == null) return false;
            
            if (portraitControllerPrefab != null && currentController.name.Contains(portraitControllerPrefab.name))
                return true;
            if (landscapeControllerPrefab != null && currentController.name.Contains(landscapeControllerPrefab.name))
                return false;
                
            // Fallback: проверяем по имени
            return currentController.name.ToLower().Contains("portrait");
        }

        /// <summary>
        /// Применяет конфигурацию к текущему контроллеру
        /// </summary>
        private void ApplyConfigurationToController(ViewportConfiguration configuration)
        {
            if (currentController == null || configuration == null) return;
            
            // Здесь можно применить специфичные для конфигурации настройки
            // Например, изменить качество, анимации и т.д.
            
            if (debugLogging)
                logger.LogInfo($"Applied configuration {configuration.ToString()} to controller");
        }

        /// <summary>
        /// Проверяет корректность назначенных префабов
        /// </summary>
        private void ValidatePrefabs()
        {
            if (portraitControllerPrefab == null)
            {
                logger.LogError("Portrait controller prefab is not assigned!");
            }
            else if (portraitControllerPrefab.GetComponent<IdentityUIController>() == null)
            {
                logger.LogError("Portrait controller prefab does not contain IdentityUIController component!");
            }
            
            if (landscapeControllerPrefab == null)
            {
                logger.LogError("Landscape controller prefab is not assigned!");
            }
            else if (landscapeControllerPrefab.GetComponent<IdentityUIController>() == null)
            {
                logger.LogError("Landscape controller prefab does not contain IdentityUIController component!");
            }
            
            if (portraitControllerPrefab == landscapeControllerPrefab)
            {
                logger.LogWarning("Portrait and landscape controller prefabs are the same!");
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Получает текущий контроллер
        /// </summary>
        public IdentityUIController GetCurrentController()
        {
            return currentController;
        }

        /// <summary>
        /// Принудительно пересоздает контроллер для портретной ориентации
        /// </summary>
        public void ForcePortraitMode()
        {
            RecreateController(VMScreenOrientation.Portrait);
        }

        /// <summary>
        /// Принудительно пересоздает контроллер для горизонтальной ориентации
        /// </summary>
        public void ForceLandscapeMode()
        {
            RecreateController(VMScreenOrientation.Landscape);
        }

        /// <summary>
        /// Принудительно пересоздает контроллер для текущей ориентации
        /// </summary>
        public void RecreateCurrentController()
        {
            RecreateController(lastOrientation);
        }

        /// <summary>
        /// Переключает режим сохранения состояния
        /// </summary>
        public void SetPreserveState(bool preserve)
        {
            preserveState = preserve;
        }

        /// <summary>
        /// Получает информацию о текущем состоянии
        /// </summary>
        public string GetStateInfo()
        {
            return $"Controller: {currentController?.name ?? "None"}, " +
                   $"Orientation: {lastOrientation}, " +
                   $"Transitioning: {isTransitioning}, " +
                   $"PreserveState: {preserveState}, " +
                   $"SavedState: IsOpen={savedState?.isOpen}, IsLite={savedState?.isCurrentlyLite}";
        }

        #endregion

        #region Editor Support

#if UNITY_EDITOR
        [ContextMenu("Debug State Info")]
        private void DebugStateInfo()
        {
            Debug.Log(GetStateInfo());
        }
        
        [ContextMenu("Force Portrait")]
        private void EditorForcePortrait()
        {
            ForcePortraitMode();
        }
        
        [ContextMenu("Force Landscape")]
        private void EditorForceLandscape()
        {
            ForceLandscapeMode();
        }
        
        [ContextMenu("Recreate Controller")]
        private void EditorRecreateController()
        {
            RecreateCurrentController();
        }
        
        [ContextMenu("Validate Setup")]
        private void EditorValidateSetup()
        {
            ValidatePrefabs();
            
            if (currentController != null)
            {
                Debug.Log($"Current controller: {currentController.name}, Active: {currentController.gameObject.activeInHierarchy}");
                Debug.Log($"IdentityUIController.Instance: {IdentityUIController.Instance?.name ?? "null"}");
            }
            else
            {
                Debug.Log("No current controller");
            }
        }
#endif

        #endregion
    }
}
