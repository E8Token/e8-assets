using System;
using NUnit.Framework;
using UnityEngine;
using Energy8.JSPluginTools.Core;

namespace Energy8.JSPluginTools.Tests
{
    /// <summary>
    /// Тесты для проверки поведения при ошибках и граничных условиях
    /// </summary>
    public class ErrorHandlingTests
    {
        // Тестовый модуль, бросающий исключение
        private class ExceptionThrowingModule : IPluginModule, IJSMessageHandler
        {
            public string ModuleId => "exception_module";
            public bool IsInitialized { get; private set; }
            
            public bool Initialize()
            {
                // При инициализации бросаем исключение
                throw new InvalidOperationException("Test initialization exception");
            }
            
            public void Shutdown()
            {
                IsInitialized = false;
            }
            
            public void HandleJSMessage(JSMessage message)
            {
                // При обработке сообщения бросаем исключение
                throw new ArgumentException("Test message handling exception");
            }
        }
        
        // Модуль с некорректными данными
        private class InvalidDataModule : IPluginModule, IJSMessageHandler
        {
            public string ModuleId => "invalid_data_module";
            public bool IsInitialized { get; private set; }
            
            public bool Initialize()
            {
                IsInitialized = true;
                return true;
            }
            
            public void Shutdown()
            {
                IsInitialized = false;
            }
            
            public void HandleJSMessage(JSMessage message)
            {
                // Ничего не делаем, просто не падаем на невалидных данных
            }
        }
        
        [Test]
        public void PluginManager_RegisterNullModule_ShouldReturnFalse()
        {
            // Arrange
            var pluginManager = PluginManager.Instance;
            
            // Act
            bool result = pluginManager.RegisterModule(null);
            
            // Assert
            Assert.IsFalse(result, "Registering null module should return false");
        }
        
        [Test]
        public void PluginManager_RegisterModuleWithNullId_ShouldReturnFalse()
        {
            // Arrange
            var pluginManager = PluginManager.Instance;
            var testModule = new NullIdModule();
            
            // Act
            bool result = pluginManager.RegisterModule(testModule);
            
            // Assert
            Assert.IsFalse(result, "Registering module with null ID should return false");
        }
        
        [Test]
        public void PluginManager_GetNonExistingModule_ShouldReturnNull()
        {
            // Arrange
            var pluginManager = PluginManager.Instance;
            
            // Act
            var module = pluginManager.GetModule<IPluginModule>("non_existing_module");
            
            // Assert
            Assert.IsNull(module, "Getting non-existing module should return null");
        }
        
        [Test]
        public void PluginManager_GetModuleWithWrongType_ShouldReturnNull()
        {
            // Arrange
            var pluginManager = PluginManager.Instance;
            var testModule = new TestPluginModule();
            pluginManager.RegisterModule(testModule);
            
            try
            {
                // Act - пытаемся получить модуль с неверным типом
                var wrongTypeModule = pluginManager.GetModule<InvalidDataModule>(testModule.ModuleId);
                
                // Assert
                Assert.IsNull(wrongTypeModule, "Getting module with wrong type should return null");
            }
            finally
            {
                // Очистка
                pluginManager.ShutdownModule(testModule.ModuleId);
            }
        }
        
        [Test]
        public void PluginManager_InitializeModuleWithException_ShouldHandleException()
        {
            // Arrange
            var pluginManager = PluginManager.Instance;
            var exceptionModule = new ExceptionThrowingModule();
            pluginManager.RegisterModule(exceptionModule);
            
            // Act
            bool result = pluginManager.InitializeModule(exceptionModule.ModuleId);
            
            // Assert
            Assert.IsFalse(result, "Initializing module that throws exception should return false");
            Assert.IsFalse(exceptionModule.IsInitialized, "Module should not be marked as initialized after exception");
            
            // Очистка
            pluginManager.ShutdownModule(exceptionModule.ModuleId);
        }
        
        [Test]
        public void PluginManager_OnMessageFromJS_InvalidJSON_ShouldHandleError()
        {
            // Arrange
            var pluginManager = PluginManager.Instance;
            
            // Act & Assert - не должно быть исключения
            Assert.DoesNotThrow(() => pluginManager.OnMessageFromJS("This is not valid JSON"));
        }
        
        [Test]
        public void PluginManager_OnMessageFromJS_NullModuleId_ShouldHandleError()
        {
            // Arrange
            var pluginManager = PluginManager.Instance;
            var message = new JSMessage
            {
                moduleId = null,
                action = "test",
                data = "{}",
                callbackId = null
            };
            
            // Act & Assert - не должно быть исключения
            Assert.DoesNotThrow(() => pluginManager.OnMessageFromJS(JsonUtility.ToJson(message)));
        }
        
        [Test]
        public void PluginManager_OnMessageFromJS_ModuleWithException_ShouldHandleException()
        {
            // Arrange
            var pluginManager = PluginManager.Instance;
            var exceptionModule = new ExceptionThrowingModule();
            pluginManager.RegisterModule(exceptionModule);
            
            var message = new JSMessage
            {
                moduleId = exceptionModule.ModuleId,
                action = "test",
                data = "{}",
                callbackId = null
            };
            
            // Act & Assert - не должно быть исключения, даже если модуль бросает исключение
            Assert.DoesNotThrow(() => pluginManager.OnMessageFromJS(JsonUtility.ToJson(message)));
            
            // Очистка
            pluginManager.ShutdownModule(exceptionModule.ModuleId);
        }
        
        [Test]
        public void PluginModule_HandleInvalidMessage_ShouldHandleGracefully()
        {
            // Arrange
            var pluginManager = PluginManager.Instance;
            var invalidDataModule = new InvalidDataModule();
            pluginManager.RegisterModule(invalidDataModule);
            pluginManager.InitializeModule(invalidDataModule.ModuleId);
            
            // Создаем сообщение с некорректными данными
            var message = new JSMessage
            {
                moduleId = invalidDataModule.ModuleId,
                action = "test",
                data = "{invalid_json}", // Некорректный JSON
                callbackId = null
            };
            
            // Act & Assert - не должно быть исключения
            Assert.DoesNotThrow(() => pluginManager.OnMessageFromJS(JsonUtility.ToJson(message)));
            
            // Очистка
            pluginManager.ShutdownModule(invalidDataModule.ModuleId);
        }
        
        // Вспомогательные классы
        
        private class NullIdModule : IPluginModule
        {
            public string ModuleId => null;
            public bool IsInitialized { get; private set; }
            
            public bool Initialize()
            {
                IsInitialized = true;
                return true;
            }
            
            public void Shutdown()
            {
                IsInitialized = false;
            }
        }
        
        private class TestPluginModule : IPluginModule
        {
            public string ModuleId => "test_module";
            public bool IsInitialized { get; private set; }
            
            public bool Initialize()
            {
                IsInitialized = true;
                return true;
            }
            
            public void Shutdown()
            {
                IsInitialized = false;
            }
        }
    }
}
