using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Energy8.JSPluginTools.Core;

namespace Energy8.JSPluginTools.Tests
{
    /// <summary>
    /// Play Mode тесты для проверки интеграции Communication API с PluginManager
    /// </summary>
    public class CommunicationPluginManagerTests : MonoBehaviour
    {
        // Тестовый модуль для проверки интеграции с PluginManager
        private class TestPluginModule : PluginModuleBase, IJSMessageHandler
        {
            public List<JSMessage> ReceivedMessages { get; } = new List<JSMessage>();
            public TestPluginModule(string moduleId) : base(moduleId) { }
            protected override bool OnInitialize()
            {
                // Специфичная для модуля инициализация
                return true;
            }

            protected override void OnShutdown()
            {
                // Специфичная для модуля очистка
                ReceivedMessages.Clear();
            }

            public void HandleJSMessage(JSMessage message)
            {
                ReceivedMessages.Add(message);

                // Если сообщение требует ответа, отправляем его
                if (message.action == "echo")
                {
                    ExternalCommunicator.SendMessageToJS(ModuleId, "echo_response", message.data, message.callbackId);
                }
            }            // Метод для отправки тестового сообщения в JavaScript
            public void SendTestMessage(string action, string data)
            {
                ExternalCommunicator.SendMessageToJS(ModuleId, action, data);
            }
        }

        // Тестовый обработчик JS вызовов для перехвата вызовов в JavaScript
        private class TestJSCallHandler : IJSCallHandler
        {
            public List<(string FunctionPath, object[] Args)> Calls { get; } = new List<(string, object[])>();

            public void HandleCall(string functionPath, object[] args)
            {
                Calls.Add((functionPath, args));

                // Если это вызов JavaScript, симулируем ответ от JS (только для тестирования)
                if (functionPath == "UnityWebPlugin.Core.receiveMessageFromUnity" && args.Length > 0)
                {
                    // Получаем сообщение
                    var message = JsonUtility.FromJson<JSMessage>((string)args[0]);

                    // Симулируем, что JS отправляет ответное сообщение обратно в Unity
                    if (message.action == "testAction")
                    {
                        // Создаем сообщение-ответ от JavaScript
                        var responseMessage = new JSMessage
                        {
                            moduleId = message.moduleId,
                            action = "jsResponse",
                            data = "{\"response\":\"from JS\"}",
                            callbackId = message.callbackId
                        };

                        // Отправляем сообщение в PluginManager
                        string jsonResponse = JsonUtility.ToJson(responseMessage);
                        PluginManager.Instance.OnMessageFromJS(jsonResponse);
                    }
                }
            }
        }

        private TestPluginModule _testModule;
        private TestJSCallHandler _testCallHandler;
        private PluginManager _pluginManager;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            // Создаем тестовый модуль
            _testModule = new TestPluginModule("testModule");

            // Устанавливаем тестовый обработчик JS вызовов
            _testCallHandler = new TestJSCallHandler();
            var methodInfo = typeof(ExternalCommunicator).GetMethod("SetCallHandler",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            methodInfo.Invoke(null, new object[] { _testCallHandler });

            // Получаем экземпляр PluginManager
            _pluginManager = PluginManager.Instance;

            // Регистрируем тестовый модуль
            _pluginManager.RegisterModule(_testModule);

            // Даем время для инициализации
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            // Отключаем тестовый модуль
            if (_pluginManager != null && _testModule != null)
            {
                _pluginManager.ShutdownModule(_testModule.ModuleId);
            }

            // Восстанавливаем стандартный обработчик JS вызовов
            var methodInfo = typeof(ExternalCommunicator).GetMethod("SetCallHandler",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            methodInfo.Invoke(null, new object[] { null });

            // Уничтожаем PluginManager
            if (_pluginManager != null)
            {
                Object.Destroy(_pluginManager.gameObject);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator PluginManager_InitializeModule_ShouldInitializeCorrectly()
        {
            // Act
            bool result = _pluginManager.InitializeModule(_testModule.ModuleId);

            // Ждем один кадр
            yield return null;

            // Assert
            Assert.IsTrue(result, "Module should initialize successfully");
            Assert.IsTrue(_testModule.IsInitialized, "Module should be marked as initialized");
        }

        [UnityTest]
        public IEnumerator PluginManager_OnMessageFromJS_ShouldRouteMessageToCorrectModule()
        {
            // Arrange - инициализируем модуль
            _pluginManager.InitializeModule(_testModule.ModuleId);
            yield return null;

            // Создаем тестовое сообщение от JavaScript
            var message = new JSMessage
            {
                moduleId = _testModule.ModuleId,
                action = "testMessageFromJS",
                data = "{\"test\":\"data\"}",
                callbackId = "jsCallback123"
            };

            // Act - отправляем сообщение через PluginManager
            string jsonMessage = JsonUtility.ToJson(message);
            _pluginManager.OnMessageFromJS(jsonMessage);

            // Ждем один кадр
            yield return null;

            // Assert
            Assert.AreEqual(1, _testModule.ReceivedMessages.Count, "Message should be received by the module");

            var receivedMessage = _testModule.ReceivedMessages[0];
            Assert.AreEqual(message.moduleId, receivedMessage.moduleId, "Module ID should match");
            Assert.AreEqual(message.action, receivedMessage.action, "Action should match");
            Assert.AreEqual(message.data, receivedMessage.data, "Data should match");
            Assert.AreEqual(message.callbackId, receivedMessage.callbackId, "Callback ID should match");
        }

        [UnityTest]
        public IEnumerator Module_SendMessageToJS_ShouldCallExternalCommunicator()
        {
            // Arrange - инициализируем модуль
            _pluginManager.InitializeModule(_testModule.ModuleId);
            yield return null;

            // Очищаем список вызовов
            _testCallHandler.Calls.Clear();

            // Act - отправляем сообщение из модуля в JavaScript
            _testModule.SendTestMessage("testAction", "{\"from\":\"unity\"}");

            // Ждем один кадр
            yield return null;

            // Assert
            Assert.IsTrue(_testCallHandler.Calls.Count > 0, "ExternalCommunicator should be called");

            var lastCall = _testCallHandler.Calls[_testCallHandler.Calls.Count - 1];
            Assert.AreEqual("UnityWebPlugin.Core.receiveMessageFromUnity", lastCall.FunctionPath,
                "Function path should match");

            // Проверяем содержимое сообщения
            Assert.IsTrue(lastCall.Args.Length > 0, "Arguments should not be empty");
            var jsonMessage = lastCall.Args[0] as string;
            var sentMessage = JsonUtility.FromJson<JSMessage>(jsonMessage);

            Assert.AreEqual(_testModule.ModuleId, sentMessage.moduleId, "Module ID should match");
            Assert.AreEqual("testAction", sentMessage.action, "Action should match");
            Assert.AreEqual("{\"from\":\"unity\"}", sentMessage.data, "Data should match");
        }

        [UnityTest]
        public IEnumerator Module_SendMessageToJS_ShouldReceiveResponse()
        {
            // Arrange - инициализируем модуль
            _pluginManager.InitializeModule(_testModule.ModuleId);
            yield return null;

            // Очищаем список полученных сообщений
            _testModule.ReceivedMessages.Clear();

            // Act - отправляем сообщение, которое вызовет ответ в тестовом обработчике
            _testModule.SendTestMessage("testAction", "{\"from\":\"unity\"}");

            // Ждем один кадр для обработки
            yield return null;

            // Assert - проверяем, что получили ответное сообщение
            Assert.AreEqual(1, _testModule.ReceivedMessages.Count, "Module should receive response message");
            var responseMessage = _testModule.ReceivedMessages[0];

            Assert.AreEqual(_testModule.ModuleId, responseMessage.moduleId, "Module ID should match");
            Assert.AreEqual("jsResponse", responseMessage.action, "Action should be jsResponse");
            Assert.AreEqual("{\"response\":\"from JS\"}", responseMessage.data, "Data should match expected response");
        }

        [UnityTest]
        public IEnumerator PluginManager_ShutdownModule_ShouldShutdownCorrectly()
        {
            // Arrange - инициализируем модуль
            _pluginManager.InitializeModule(_testModule.ModuleId);
            yield return null;

            Assert.IsTrue(_testModule.IsInitialized, "Module should be initialized");

            // Act - отключаем модуль
            _pluginManager.ShutdownModule(_testModule.ModuleId);
            yield return null;

            // Assert
            Assert.IsFalse(_testModule.IsInitialized, "Module should not be initialized after shutdown");
            Assert.AreEqual(0, _testModule.ReceivedMessages.Count, "Messages should be cleared on shutdown");
        }
    }
}
