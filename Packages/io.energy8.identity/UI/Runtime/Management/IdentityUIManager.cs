using UnityEngine;
using Energy8.Identity.UI.Runtime.Controllers;

namespace Energy8.Identity.UI.Runtime.Management
{
    /// <summary>
    /// Менеджер для инициализации IdentityOrchestrator как синглтона.
    /// Должен быть размещен на сцене и будет создавать единственный экземпляр IdentityOrchestrator.
    /// </summary>
    public class IdentityUIManager : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private bool isLite = false;
        [SerializeField] private bool debugLogging = false;
        [SerializeField] private bool autoInitialize = true;

        private void Awake()
        {
            if (autoInitialize)
            {
                InitializeIdentityOrchestrator();
            }
        }

        /// <summary>
        /// Инициализирует IdentityOrchestrator как синглтон
        /// </summary>
        public void InitializeIdentityOrchestrator()
        {
            // Проверяем, есть ли уже экземпляр
            if (IdentityOrchestrator.Instance != null)
            {
                if (debugLogging)
                    Debug.Log("IdentityOrchestrator already exists, skipping initialization");
                return;
            }

            // Создаем GameObject для IdentityOrchestrator
            GameObject identityControllerGO = new GameObject("IdentityOrchestrator");
            
            // Добавляем компонент IdentityOrchestrator
            var identityController = identityControllerGO.AddComponent<IdentityOrchestrator>();
            
            // Настраиваем параметры через рефлексию (поскольку поля SerializeField)
            var identityType = typeof(IdentityOrchestrator);
            
            var isLiteField = identityType.GetField("isLite", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (isLiteField != null)
            {
                isLiteField.SetValue(identityController, isLite);
            }
            
            var debugLoggingField = identityType.GetField("debugLogging", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (debugLoggingField != null)
            {
                debugLoggingField.SetValue(identityController, debugLogging);
            }

            if (debugLogging)
                Debug.Log($"IdentityOrchestrator initialized: IsLite={isLite}, DebugLogging={debugLogging}");
        }

        /// <summary>
        /// Уничтожает текущий экземпляр IdentityOrchestrator
        /// </summary>
        public void DestroyIdentityOrchestrator()
        {
            if (IdentityOrchestrator.Instance != null)
            {
                if (debugLogging)
                    Debug.Log("Destroying IdentityOrchestrator instance");
                    
                Destroy(IdentityOrchestrator.Instance.gameObject);
            }
        }

        /// <summary>
        /// Получает информацию о состоянии IdentityOrchestrator
        /// </summary>
        public string GetIdentityOrchestratorInfo()
        {
            if (IdentityOrchestrator.Instance == null)
                return "IdentityOrchestrator: Not initialized";

            var instance = IdentityOrchestrator.Instance;
            return $"IdentityOrchestrator: {instance.name}\n" +
                   $"IsOpen: {instance.IsOpen}\n" +
                   $"IsLite: {instance.IsLite}\n" +
                   $"Canvas Controller: {(instance.CurrentCanvasController != null ? instance.CurrentCanvasController.name : "null")}";
        }

#if UNITY_EDITOR
        [ContextMenu("Initialize IdentityOrchestrator")]
        private void EditorInitialize()
        {
            InitializeIdentityOrchestrator();
        }

        [ContextMenu("Destroy IdentityOrchestrator")]
        private void EditorDestroy()
        {
            DestroyIdentityOrchestrator();
        }

        [ContextMenu("Debug Info")]
        private void EditorDebugInfo()
        {
            Debug.Log(GetIdentityOrchestratorInfo());
        }
#endif
    }
}
