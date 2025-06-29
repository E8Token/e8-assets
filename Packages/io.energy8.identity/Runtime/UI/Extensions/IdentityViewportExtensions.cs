using System;
using UnityEngine;
using Energy8.Identity.Runtime.UI.Controllers;
using VMScreenOrientation = Energy8.ViewportManager.Core.ScreenOrientation;

namespace Energy8.Identity.Extensions
{
    /// <summary>
    /// Extension методы для работы с IdentityUIController в контексте ViewportManager
    /// </summary>
    public static class IdentityViewportExtensions
    {
        /// <summary>
        /// Получает или создает IdentityViewportManagerEventListener для данного контроллера
        /// </summary>
        public static IdentityViewportManagerEventListener GetOrCreateViewportListener(this IdentityUIController controller)
        {
            var listener = UnityEngine.Object.FindObjectOfType<IdentityViewportManagerEventListener>();
            
            if (listener == null)
            {
                var go = new GameObject("IdentityViewportManager");
                listener = go.AddComponent<IdentityViewportManagerEventListener>();
                UnityEngine.Object.DontDestroyOnLoad(go);
            }
            
            return listener;
        }

        /// <summary>
        /// Проверяет, является ли контроллер активным в ViewportListener
        /// </summary>
        public static bool IsActiveInViewportListener(this IdentityUIController controller)
        {
            var listener = UnityEngine.Object.FindObjectOfType<IdentityViewportManagerEventListener>();
            return listener?.GetCurrentActiveController() == controller;
        }

        /// <summary>
        /// Конфигурирует контроллер для определенной ориентации
        /// </summary>
        public static void ConfigureForOrientation(this IdentityUIController controller, VMScreenOrientation orientation, bool isLiteMode = false)
        {
            if (controller == null) return;

            // Применяем настройки в зависимости от ориентации
            switch (orientation)
            {
                case VMScreenOrientation.Portrait:
                    ConfigureForPortrait(controller, isLiteMode);
                    break;
                    
                case VMScreenOrientation.Landscape:
                case VMScreenOrientation.LandscapeLeft:
                case VMScreenOrientation.LandscapeRight:
                    ConfigureForLandscape(controller, isLiteMode);
                    break;
            }
        }

        /// <summary>
        /// Настраивает контроллер для портретной ориентации
        /// </summary>
        private static void ConfigureForPortrait(IdentityUIController controller, bool isLiteMode)
        {
            // Используем рефлексию для установки режима
            var controllerType = controller.GetType();
            
            try
            {
                var isLiteField = controllerType.GetField("isLite", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (isLiteField != null)
                {
                    isLiteField.SetValue(controller, isLiteMode);
                }

                var useViewportManagerField = controllerType.GetField("useViewportManager", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (useViewportManagerField != null)
                {
                    useViewportManagerField.SetValue(controller, true);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to configure controller for portrait: {ex.Message}");
            }
        }

        /// <summary>
        /// Настраивает контроллер для альбомной ориентации
        /// </summary>
        private static void ConfigureForLandscape(IdentityUIController controller, bool isLiteMode)
        {
            // Используем рефлексию для установки режима
            var controllerType = controller.GetType();
            
            try
            {
                var isLiteField = controllerType.GetField("isLite", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (isLiteField != null)
                {
                    isLiteField.SetValue(controller, isLiteMode);
                }

                var useViewportManagerField = controllerType.GetField("useViewportManager", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (useViewportManagerField != null)
                {
                    useViewportManagerField.SetValue(controller, true);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to configure controller for landscape: {ex.Message}");
            }
        }

        /// <summary>
        /// Получает информацию о конфигурации контроллера
        /// </summary>
        public static string GetConfigurationInfo(this IdentityUIController controller)
        {
            if (controller == null) return "Controller is null";

            var controllerType = controller.GetType();
            string info = $"Controller: {controller.name}\n";
            
            try
            {
                var isLiteField = controllerType.GetField("isLite", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (isLiteField != null)
                {
                    bool isLite = (bool)isLiteField.GetValue(controller);
                    info += $"Lite Mode: {isLite}\n";
                }

                var useViewportManagerField = controllerType.GetField("useViewportManager", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (useViewportManagerField != null)
                {
                    bool useViewportManager = (bool)useViewportManagerField.GetValue(controller);
                    info += $"Use Viewport Manager: {useViewportManager}\n";
                }

                var animationDurationField = controllerType.GetField("animationDuration", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (animationDurationField != null)
                {
                    float animationDuration = (float)animationDurationField.GetValue(controller);
                    info += $"Animation Duration: {animationDuration}\n";
                }
            }
            catch (Exception ex)
            {
                info += $"Error getting configuration: {ex.Message}\n";
            }
            
            info += $"Is Open: {controller.IsOpen}\n";
            info += $"Active: {controller.gameObject.activeInHierarchy}";
            
            return info;
        }

        /// <summary>
        /// Синхронизирует состояние между двумя контроллерами
        /// </summary>
        public static void SynchronizeWith(this IdentityUIController fromController, IdentityUIController toController)
        {
            if (fromController == null || toController == null) return;

            var fromType = fromController.GetType();
            var toType = toController.GetType();

            try
            {
                // Синхронизируем основные поля
                var fieldsToSync = new[] { "isLite", "useViewportManager", "forceMode", "animationDuration" };

                foreach (var fieldName in fieldsToSync)
                {
                    var fromField = fromType.GetField(fieldName, 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    var toField = toType.GetField(fieldName, 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                    if (fromField != null && toField != null)
                    {
                        var value = fromField.GetValue(fromController);
                        toField.SetValue(toController, value);
                    }
                }

                // Синхронизируем состояние открытости
                if (fromController.IsOpen != toController.IsOpen)
                {
                    toController.SetOpenState(fromController.IsOpen);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to synchronize controllers: {ex.Message}");
            }
        }

        /// <summary>
        /// Проверяет совместимость контроллера с ViewportManager
        /// </summary>
        public static bool IsViewportManagerCompatible(this IdentityUIController controller)
        {
            if (controller == null) return false;

            var controllerType = controller.GetType();
            
            try
            {
                var useViewportManagerField = controllerType.GetField("useViewportManager", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (useViewportManagerField != null)
                {
                    return (bool)useViewportManagerField.GetValue(controller);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to check ViewportManager compatibility: {ex.Message}");
            }

            return false;
        }

        /// <summary>
        /// Устанавливает оптимальные настройки для мобильных устройств
        /// </summary>
        public static void OptimizeForMobile(this IdentityUIController controller)
        {
            if (controller == null) return;

            controller.ConfigureForOrientation(
                Screen.width < Screen.height ? VMScreenOrientation.Portrait : VMScreenOrientation.Landscape, 
                true // Enable lite mode for mobile
            );

            // Дополнительные оптимизации для мобильных устройств
            var controllerType = controller.GetType();
            
            try
            {
                var animationDurationField = controllerType.GetField("animationDuration", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (animationDurationField != null)
                {
                    // Ускоряем анимации на мобильных устройствах
                    animationDurationField.SetValue(controller, 0.3f);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to optimize controller for mobile: {ex.Message}");
            }
        }
    }
}
