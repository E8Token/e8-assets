using UnityEngine;
using Energy8.Identity.UI.Runtime.Controllers;

namespace Energy8.Identity.UI.Runtime.Management
{
    /// <summary>
    /// Менеджер для инициализации IdentityUIController как синглтона.
    /// Должен быть размещен на сцене и будет создавать единственный экземпляр IdentityUIController.
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
                InitializeIdentityUIController();
            }
        }

        /// <summary>
        /// Инициализирует IdentityUIController как синглтон
        /// </summary>
        public void InitializeIdentityUIController()
        {
            // Проверяем, есть ли уже экземпляр
            if (IdentityUIController.Instance != null)
            {
                if (debugLogging)
                    Debug.Log("IdentityUIController already exists, skipping initialization");
                return;
            }

            // Создаем GameObject для IdentityUIController
            GameObject identityControllerGO = new GameObject("IdentityUIController");
            
            // Добавляем компонент IdentityUIController
            var identityController = identityControllerGO.AddComponent<IdentityUIController>();
            
            // Настраиваем параметры через рефлексию (поскольку поля SerializeField)
            var identityType = typeof(IdentityUIController);
            
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
                Debug.Log($"IdentityUIController initialized: IsLite={isLite}, DebugLogging={debugLogging}");
        }

        /// <summary>
        /// Уничтожает текущий экземпляр IdentityUIController
        /// </summary>
        public void DestroyIdentityUIController()
        {
            if (IdentityUIController.Instance != null)
            {
                if (debugLogging)
                    Debug.Log("Destroying IdentityUIController instance");
                    
                Destroy(IdentityUIController.Instance.gameObject);
            }
        }

        /// <summary>
        /// Получает информацию о состоянии IdentityUIController
        /// </summary>
        public string GetIdentityUIControllerInfo()
        {
            if (IdentityUIController.Instance == null)
                return "IdentityUIController: Not initialized";

            var instance = IdentityUIController.Instance;
            return $"IdentityUIController: {instance.name}\n" +
                   $"IsOpen: {instance.IsOpen}\n" +
                   $"IsLite: {instance.IsLite}\n" +
                   $"Canvas Controller: {(instance.CurrentCanvasController != null ? instance.CurrentCanvasController.name : "null")}";
        }

#if UNITY_EDITOR
        [ContextMenu("Initialize IdentityUIController")]
        private void EditorInitialize()
        {
            InitializeIdentityUIController();
        }

        [ContextMenu("Destroy IdentityUIController")]
        private void EditorDestroy()
        {
            DestroyIdentityUIController();
        }

        [ContextMenu("Debug Info")]
        private void EditorDebugInfo()
        {
            Debug.Log(GetIdentityUIControllerInfo());
        }
#endif
    }
}
