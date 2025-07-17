using System;
using UnityEngine;
using Energy8.ViewportManager.Components;
using Energy8.ViewportManager.Core;
using Energy8.ViewportManager.Configuration;

using VMScreenOrientation = Energy8.ViewportManager.Core.ScreenOrientation;

namespace Energy8.Identity.UI.Runtime.Controllers
{
    /// <summary>
    /// Менеджер Identity UI, который автоматически создает и управляет Canvas контроллерами
    /// в зависимости от ориентации экрана, пересоздавая их из соответствующих префабов.
    /// Работает с единственным IdentityUIController Instance.
    /// </summary>
    public class IdentityViewportManager : ViewportEventListener
    {
        [Header("Identity Prefabs")]
        [SerializeField] private GameObject portraitCanvasPrefab;
        [SerializeField] private GameObject landscapeCanvasPrefab;
        
        [Header("Controller Settings")]
        [SerializeField] private string canvasControllerName = "IdentityCanvas";
        [SerializeField] private bool autoCreateOnStart = true;
        
        [Header("State Management")]
        [SerializeField] private bool preserveState = true;
        
        [Header("Debug")]
        [SerializeField] private bool debugLogging = false;

        private IdentityCanvasController currentCanvasController;
        private IdentityCanvasState savedState;
        private bool isTransitioning = false;
        private VMScreenOrientation lastOrientation;

        /// <summary>
        /// Структура для сохранения состояния Identity Canvas контроллера
        /// </summary>
        [Serializable]
        private class IdentityCanvasState
        {
            public bool isOpen;
            public Vector2 lastScreenSize;

            public IdentityCanvasState()
            {
                isOpen = false;
                lastScreenSize = Vector2.zero;
            }
        }

        #region Unity Lifecycle

        protected override void Awake()
        {
            base.Awake();
            
            // Инициализируем состояние
            savedState = new IdentityCanvasState();
            
            // Проверяем префабы
            ValidatePrefabs();
            
            if (debugLogging)
                Debug.Log("IdentityViewportManager initialized");
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
                Debug.Log($"Initial setup with context: {initialContext}");
                
            lastOrientation = initialContext.orientation;
            EnsureCorrectCanvasController(initialContext.orientation);
        }

        protected override void OnManagerInitialized()
        {
            if (debugLogging)
                Debug.Log("ViewportManager initialized, creating initial canvas controller");
            
            if (autoCreateOnStart)
            {
                CreateInitialCanvasController();
            }
        }

        protected override void OnOrientationChanged(VMScreenOrientation fromOrientation, VMScreenOrientation toOrientation) 
        {
            if (debugLogging)
                Debug.Log($"Orientation changed: {fromOrientation} → {toOrientation}");
                
            if (isTransitioning) return;
            
            lastOrientation = toOrientation;
            EnsureCorrectCanvasController(toOrientation);
        }

        protected override void OnContextChanged(ViewportContext previousContext, ViewportContext newContext)
        {
            if (debugLogging)
                Debug.Log($"Context changed: {previousContext} → {newContext}");
                
            if (isTransitioning) return;
            
            lastOrientation = newContext.orientation;
            EnsureCorrectCanvasController(newContext.orientation);
        }

        protected override void OnConfigurationChanged(ViewportConfiguration previousConfiguration, ViewportConfiguration newConfiguration)
        {
            if (debugLogging)
                Debug.Log($"Configuration changed: {previousConfiguration?.ToString()} → {newConfiguration?.ToString()}");
                
            // Применяем новую конфигурацию к текущему контроллеру
            ApplyConfigurationToController(newConfiguration);
        }

        #endregion

        #region Canvas Controller Management

        /// <summary>
        /// Создает начальный Canvas контроллер на основе текущей ориентации
        /// </summary>
        private void CreateInitialCanvasController()
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
            CreateCanvasController(orientation);
        }

        /// <summary>
        /// Убеждается, что текущий Canvas контроллер соответствует ориентации
        /// </summary>
        private void EnsureCorrectCanvasController(VMScreenOrientation orientation)
        {
            bool needsPortrait = IsPortraitOrientation(orientation);
            bool hasPortrait = IsCurrentCanvasControllerPortrait();
            
            // Если контроллера нет или он не соответствует ориентации
            if (currentCanvasController == null || needsPortrait != hasPortrait)
            {
                RecreateCanvasController(orientation);
            }
        }

        /// <summary>
        /// Пересоздает Canvas контроллер для указанной ориентации
        /// </summary>
        private void RecreateCanvasController(VMScreenOrientation orientation)
        {
            if (isTransitioning) return;
            
            isTransitioning = true;
            
            try
            {
                // Сохраняем состояние текущего контроллера
                if (currentCanvasController != null && preserveState)
                {
                    SaveCanvasControllerState();
                }
                
                // Уничтожаем текущий контроллер
                DestroyCurrentCanvasController();
                
                // Создаем новый контроллер
                CreateCanvasController(orientation);
                
                // Восстанавливаем состояние
                if (preserveState && currentCanvasController != null)
                {
                    RestoreCanvasControllerState();
                }
                
                if (debugLogging)
                    Debug.Log($"Recreated canvas controller for orientation: {orientation}");
            }
            finally
            {
                isTransitioning = false;
            }
        }

        /// <summary>
        /// Создает Canvas контроллер для указанной ориентации
        /// </summary>
        private void CreateCanvasController(VMScreenOrientation orientation)
        {
            GameObject prefab = IsPortraitOrientation(orientation) ? portraitCanvasPrefab : landscapeCanvasPrefab;
            
            if (prefab == null)
            {
                Debug.LogError($"No prefab assigned for orientation: {orientation}");
                return;
            }
            
            // Проверяем, что префаб содержит IdentityCanvasController
            if (prefab.GetComponent<IdentityCanvasController>() == null)
            {
                Debug.LogError($"Prefab {prefab.name} does not contain IdentityCanvasController component!");
                return;
            }
            
            // Создаем контроллер
            GameObject canvasControllerGO = Instantiate(prefab);
            canvasControllerGO.name = canvasControllerName;
            
            // Получаем компонент контроллера
            currentCanvasController = canvasControllerGO.GetComponent<IdentityCanvasController>();
            
            // Устанавливаем Canvas контроллер в IdentityUIController
            if (IdentityUIController.Instance != null)
            {
                IdentityUIController.Instance.SetCanvasController(currentCanvasController);
            }
            else
            {
                Debug.LogWarning("IdentityUIController.Instance is null, canvas controller will be set later");
            }
                        
            if (debugLogging)
                Debug.Log($"Created canvas controller: {canvasControllerGO.name} from prefab: {prefab.name}");
        }

        /// <summary>
        /// Уничтожает текущий Canvas контроллер
        /// </summary>
        private void DestroyCurrentCanvasController()
        {
            if (currentCanvasController != null)
            {
                // Закрываем UI перед уничтожением
                if (currentCanvasController.IsOpen)
                {
                    currentCanvasController.SetOpenState(false);
                }
                
                string controllerName = currentCanvasController.name;
                
                // Отключаем от IdentityUIController
                if (IdentityUIController.Instance != null)
                {
                    IdentityUIController.Instance.SetCanvasController(null);
                }
                
                // Уничтожаем GameObject
                if (Application.isPlaying)
                {
                    Destroy(currentCanvasController.gameObject);
                }
                else
                {
                    DestroyImmediate(currentCanvasController.gameObject);
                }
                
                currentCanvasController = null;
                
                if (debugLogging)
                    Debug.Log($"Destroyed canvas controller: {controllerName}");
            }
        }

        #endregion

        #region State Management

        /// <summary>
        /// Сохраняет состояние текущего Canvas контроллера
        /// </summary>
        private void SaveCanvasControllerState()
        {
            if (currentCanvasController == null) return;
            
            savedState.isOpen = currentCanvasController.IsOpen;
            savedState.lastScreenSize = new Vector2(Screen.width, Screen.height);
            
            if (debugLogging)
                Debug.Log($"Saved canvas controller state: IsOpen={savedState.isOpen}");
        }

        /// <summary>
        /// Восстанавливает состояние Canvas контроллера
        /// </summary>
        private void RestoreCanvasControllerState()
        {
            if (currentCanvasController == null || savedState == null) return;
            
            // Восстанавливаем состояние открытия/закрытия
            if (currentCanvasController.IsOpen != savedState.isOpen)
            {
                currentCanvasController.SetOpenState(savedState.isOpen);
            }
            
            if (debugLogging)
                Debug.Log($"Restored canvas controller state: IsOpen={savedState.isOpen}");
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
        /// Проверяет, является ли текущий Canvas контроллер портретным
        /// </summary>
        private bool IsCurrentCanvasControllerPortrait()
        {
            if (currentCanvasController == null) return false;
            
            if (portraitCanvasPrefab != null && currentCanvasController.name.Contains(portraitCanvasPrefab.name))
                return true;
            if (landscapeCanvasPrefab != null && currentCanvasController.name.Contains(landscapeCanvasPrefab.name))
                return false;
                
            // Fallback: проверяем по имени
            return currentCanvasController.name.ToLower().Contains("portrait");
        }

        /// <summary>
        /// Применяет конфигурацию к текущему Canvas контроллеру
        /// </summary>
        private void ApplyConfigurationToController(ViewportConfiguration configuration)
        {
            if (currentCanvasController == null || configuration == null) return;
            
            // Здесь можно применить специфичные для конфигурации настройки
            // Например, изменить качество, анимации и т.д.
            
            if (debugLogging)
                Debug.Log($"Applied configuration {configuration.ToString()} to canvas controller");
        }

        /// <summary>
        /// Проверяет корректность назначенных префабов
        /// </summary>
        private void ValidatePrefabs()
        {
            if (portraitCanvasPrefab == null)
            {
                Debug.LogError("Portrait canvas prefab is not assigned!");
            }
            else if (portraitCanvasPrefab.GetComponent<IdentityCanvasController>() == null)
            {
                Debug.LogError("Portrait canvas prefab does not contain IdentityCanvasController component!");
            }
            
            if (landscapeCanvasPrefab == null)
            {
                Debug.LogError("Landscape canvas prefab is not assigned!");
            }
            else if (landscapeCanvasPrefab.GetComponent<IdentityCanvasController>() == null)
            {
                Debug.LogError("Landscape canvas prefab does not contain IdentityCanvasController component!");
            }
            
            if (portraitCanvasPrefab == landscapeCanvasPrefab)
            {
                Debug.LogWarning("Portrait and landscape canvas prefabs are the same!");
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Получает текущий Canvas контроллер
        /// </summary>
        public IdentityCanvasController GetCurrentCanvasController()
        {
            return currentCanvasController;
        }

        /// <summary>
        /// Принудительно пересоздает Canvas контроллер для портретной ориентации
        /// </summary>
        public void ForcePortraitMode()
        {
            RecreateCanvasController(VMScreenOrientation.Portrait);
        }

        /// <summary>
        /// Принудительно пересоздает Canvas контроллер для горизонтальной ориентации
        /// </summary>
        public void ForceLandscapeMode()
        {
            RecreateCanvasController(VMScreenOrientation.Landscape);
        }

        /// <summary>
        /// Принудительно пересоздает Canvas контроллер для текущей ориентации
        /// </summary>
        public void RecreateCurrentCanvasController()
        {
            RecreateCanvasController(lastOrientation);
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
            return $"Canvas Controller: {currentCanvasController?.name ?? "None"}, " +
                   $"Orientation: {lastOrientation}, " +
                   $"Transitioning: {isTransitioning}, " +
                   $"PreserveState: {preserveState}, " +
                   $"SavedState: IsOpen={savedState?.isOpen}, " +
                   $"IdentityUIController: {(IdentityUIController.Instance != null ? "Present" : "Null")}";
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
        
        [ContextMenu("Recreate Canvas Controller")]
        private void EditorRecreateCanvasController()
        {
            RecreateCurrentCanvasController();
        }
        
        [ContextMenu("Validate Setup")]
        private void EditorValidateSetup()
        {
            ValidatePrefabs();
            
            if (currentCanvasController != null)
            {
                Debug.Log($"Current canvas controller: {currentCanvasController.name}, Active: {currentCanvasController.gameObject.activeInHierarchy}");
                Debug.Log($"IdentityUIController.Instance: {IdentityUIController.Instance?.name ?? "null"}");
            }
            else
            {
                Debug.Log("No current canvas controller");
            }
        }
#endif

        #endregion
    }
}
