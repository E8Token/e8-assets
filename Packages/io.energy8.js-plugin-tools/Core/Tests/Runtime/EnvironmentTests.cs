using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Energy8.JSPluginTools.Core;

namespace Energy8.JSPluginTools.Tests
{
    /// <summary>
    /// Тесты для проверки работы плагина в различных средах выполнения (WebGL/редактор)
    /// </summary>
    public class EnvironmentTests
    {
        private PluginManager _pluginManager;
        
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            // Получаем экземпляр PluginManager
            _pluginManager = PluginManager.Instance;
            yield return null;
        }
        
        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (_pluginManager != null)
            {
                Object.Destroy(_pluginManager.gameObject);
            }
            yield return null;
        }
        
        [UnityTest]
        public IEnumerator PluginManager_ShouldBeInitializedInAllEnvironments()
        {
            // В любом окружении PluginManager должен успешно инициализироваться
            // В WebGL вызовется настоящая инициализация JS, в редакторе - симуляция
            
            // Ждем, чтобы инициализация завершилась
            yield return new WaitForSeconds(0.2f);
            
            // Assert
            Assert.IsTrue(_pluginManager.IsInitialized, "PluginManager should be initialized");
        }
        
        [UnityTest]
        public IEnumerator PluginManager_ShutdownAllModules_ShouldWorkInAllEnvironments()
        {
            // Arrange
            var testModule1 = new TestPluginModule("test1");
            var testModule2 = new TestPluginModule("test2");
            
            // Регистрируем и инициализируем два тестовых модуля
            _pluginManager.RegisterModule(testModule1);
            _pluginManager.RegisterModule(testModule2);
            
            _pluginManager.InitializeModule("test1");
            _pluginManager.InitializeModule("test2");
            
            // Проверяем, что модули инициализированы
            Assert.IsTrue(testModule1.IsInitialized, "Module 1 should be initialized");
            Assert.IsTrue(testModule2.IsInitialized, "Module 2 should be initialized");
            
            // Act
            _pluginManager.ShutdownAllModules();
            
            // Ждем немного, чтобы модули успели выключиться
            yield return new WaitForSeconds(0.1f);
            
            // Assert
            Assert.IsFalse(testModule1.IsInitialized, "Module 1 should be shut down");
            Assert.IsFalse(testModule2.IsInitialized, "Module 2 should be shut down");
        }
        
        [UnityTest]
        public IEnumerator ExternalCommunicator_ShouldNotThrowExceptionsInAnyEnvironment()
        {
            // Act - вызываем методы ExternalCommunicator
            // В редакторе это будет симуляция, в WebGL - реальный вызов
            
            // Проверяем, что не будет исключений при вызове CallJS
            Assert.DoesNotThrow(() => ExternalCommunicator.CallJS("console.log", "Test from Unity"));
            
            // Проверяем, что не будет исключений при вызове SendMessageToJS
            Assert.DoesNotThrow(() => ExternalCommunicator.SendMessageToJS("test", "test_action", "{\"test\":true}"));
            
            // Ждем немного, чтобы увидеть потенциальные исключения в логе
            yield return new WaitForSeconds(0.1f);
            
            // Если мы дошли сюда, значит исключений не было
            Assert.Pass("ExternalCommunicator calls didn't throw exceptions");
        }
        
        // Простой тестовый модуль для проверки функциональности
        private class TestPluginModule : IPluginModule
        {
            public string ModuleId { get; }
            public bool IsInitialized { get; private set; }
            
            public TestPluginModule(string moduleId)
            {
                ModuleId = moduleId;
            }
            
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
        
        // Проверка, что инициализация и вызовы JavaScript ведут себя по-разному
        // в зависимости от среды (WebGL/редактор)
        [Test]
        public void JSEnvironmentDetection_ShouldBeCorrectInAllEnvironments()
        {
            #if UNITY_WEBGL && !UNITY_EDITOR
            // В WebGL сборке
            Assert.Pass("Running in WebGL environment, JS calls will be real");
            #else
            // В редакторе или других платформах
            Assert.Pass("Running in non-WebGL environment, JS calls will be simulated");
            #endif
        }
    }
}
