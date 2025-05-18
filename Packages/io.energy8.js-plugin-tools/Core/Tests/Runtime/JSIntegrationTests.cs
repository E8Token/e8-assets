using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Energy8.JSPluginTools.Core;

namespace Energy8.JSPluginTools.Tests
{
    /// <summary>
    /// Интеграционные тесты для проверки двустороннего взаимодействия между Unity и JavaScript
    /// Эти тесты требуют запуска в WebGL сборке для полной проверки функциональности.
    /// В редакторе они будут использовать симуляцию.
    /// </summary>
    public class JSIntegrationTests
    {
        // Тестовый модуль для проверки коммуникации с JavaScript
        private class CommunicationTestModule : PluginModuleBase, IJSMessageHandler
        {
            public List<JSMessage> ReceivedMessages { get; } = new List<JSMessage>();
            public Dictionary<string, string> CallbackResponses { get; } = new Dictionary<string, string>();
              public CommunicationTestModule() : base("communication_test") { }
              protected override bool OnInitialize()
            {
                Debug.Log($"[Test] CommunicationTestModule initialized");
                return true;
            }
            
            protected override void OnShutdown()
            {
                Debug.Log($"[Test] CommunicationTestModule shut down");
                ReceivedMessages.Clear();
                CallbackResponses.Clear();
            }
            
            public void HandleJSMessage(JSMessage message)
            {
                Debug.Log($"[Test] Received message: {JsonUtility.ToJson(message)}");
                ReceivedMessages.Add(message);
                
                // Обработка различных типов сообщений
                switch (message.action)
                {
                    case "ping":
                        // Отправляем pong в ответ
                        SendMessageToJS("pong", message.data, message.callbackId);
                        break;
                    case "echo":
                        // Отправляем то же сообщение обратно
                        SendMessageToJS("echo_response", message.data, message.callbackId);
                        break;
                    case "callback_response":
                        // Сохраняем ответ на вызов с callback
                        if (!string.IsNullOrEmpty(message.callbackId))
                        {
                            CallbackResponses[message.callbackId] = message.data;
                        }
                        break;
                }
            }
              // Отправка сообщения в JavaScript
            public new void SendMessageToJS(string action, string data = null, string callbackId = null)
            {
                ExternalCommunicator.SendMessageToJS(ModuleId, action, data, callbackId);
            }
            
            // Генерация уникального ID для callback
            public string GenerateCallbackId()
            {
                return System.Guid.NewGuid().ToString().Substring(0, 8);
            }
        }
        
        private CommunicationTestModule _testModule;
        private PluginManager _pluginManager;
        
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            // Получаем экземпляр PluginManager
            _pluginManager = PluginManager.Instance;
            
            // Создаем тестовый модуль
            _testModule = new CommunicationTestModule();
            
            // Регистрируем модуль
            Assert.IsTrue(_pluginManager.RegisterModule(_testModule), "Module should register successfully");
            
            // Инициализируем модуль
            Assert.IsTrue(_pluginManager.InitializeModule(_testModule.ModuleId), "Module should initialize successfully");
            
            yield return null;
        }
        
        [UnityTearDown]
        public IEnumerator TearDown()
        {
            // Отключаем модуль
            if (_pluginManager != null && _testModule != null)
            {
                _pluginManager.ShutdownModule(_testModule.ModuleId);
            }
            
            yield return null;
        }
        
        [UnityTest]
        public IEnumerator SendMessage_ShouldBeAbleToSendMessageToJS()
        {
            // Arrange
            string testData = "{\"message\":\"hello from unity\"}";
            
            // Act - отправляем сообщение в JavaScript
            _testModule.SendMessageToJS("test_message", testData);
            
            // Ждем немного, чтобы сообщение обработалось
            yield return new WaitForSeconds(0.1f);
            
            // Assert - в редакторе мы не можем проверить, что сообщение дошло до JavaScript,
            // но мы можем проверить, что не возникло исключений
            // В WebGL сборке это будет реальный тест взаимодействия
            Assert.Pass("Message was sent without exceptions");
        }
        
        [UnityTest]
        public IEnumerator SimulateReceiveMessageFromJS_ShouldHandleMessage()
        {
            // Arrange - создаем тестовое сообщение от JavaScript
            var message = new JSMessage
            {
                moduleId = _testModule.ModuleId,
                action = "ping",
                data = "{\"timestamp\":" + System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + "}",
                callbackId = "ping_" + System.Guid.NewGuid().ToString().Substring(0, 8)
            };
            
            // Act - симулируем получение сообщения от JavaScript
            string jsonMessage = JsonUtility.ToJson(message);
            _pluginManager.OnMessageFromJS(jsonMessage);
            
            // Ждем обработки сообщения
            yield return new WaitForSeconds(0.1f);
            
            // Assert
            Assert.IsTrue(_testModule.ReceivedMessages.Count > 0, "Module should receive the message");
            
            // Проверяем содержимое последнего полученного сообщения
            var lastMessage = _testModule.ReceivedMessages[_testModule.ReceivedMessages.Count - 1];
            Assert.AreEqual(message.moduleId, lastMessage.moduleId, "Module ID should match");
            Assert.AreEqual(message.action, lastMessage.action, "Action should match");
            Assert.AreEqual(message.data, lastMessage.data, "Data should match");
            Assert.AreEqual(message.callbackId, lastMessage.callbackId, "Callback ID should match");
        }
        
        [UnityTest]
        public IEnumerator SimulateBidirectionalCommunication_ShouldWorkCorrectly()
        {
            // В этом тесте мы симулируем двустороннюю коммуникацию:
            // 1. Unity отправляет сообщение в JS
            // 2. JS отвечает (мы симулируем это)
            // 3. Unity обрабатывает ответ
            
            // Arrange
            string callbackId = _testModule.GenerateCallbackId();
            string testData = "{\"request\":\"test_request\"}";
            
            // Act 1 - отправляем сообщение, которое должно вызвать ответ
            _testModule.SendMessageToJS("request_with_callback", testData, callbackId);
            
            // Ждем немного
            yield return new WaitForSeconds(0.1f);
            
            // Act 2 - симулируем ответ от JavaScript
            var responseMessage = new JSMessage
            {
                moduleId = _testModule.ModuleId,
                action = "callback_response",
                data = "{\"response\":\"test_response\",\"success\":true}",
                callbackId = callbackId
            };
            
            string jsonResponse = JsonUtility.ToJson(responseMessage);
            _pluginManager.OnMessageFromJS(jsonResponse);
            
            // Ждем обработки ответа
            yield return new WaitForSeconds(0.1f);
            
            // Assert
            Assert.IsTrue(_testModule.CallbackResponses.ContainsKey(callbackId), 
                "Module should receive callback response with correct ID");
            
            // Проверяем содержимое ответа
            string responseData = _testModule.CallbackResponses[callbackId];
            Assert.AreEqual(responseMessage.data, responseData, "Response data should match");
        }
        
        [UnityTest]
        [UnityPlatform(RuntimePlatform.WebGLPlayer)] // Этот тест запустится только в WebGL сборке
        public IEnumerator RealJSCommunication_ShouldWorkInWebGL()
        {
            // Этот тест выполнится только в реальной WebGL сборке
            
            // Arrange
            string callbackId = _testModule.GenerateCallbackId();
            string testData = "{\"webgl_test\":true}";
            
            // Act - отправляем эхо-запрос в JavaScript
            _testModule.SendMessageToJS("echo", testData, callbackId);
            
            // Ждем ответа (в WebGL это будет реальный ответ от JavaScript)
            // В редакторе этот тест будет пропущен
            yield return new WaitForSeconds(0.5f);
            
            // Assert
            Assert.IsTrue(_testModule.ReceivedMessages.Exists(m => 
                m.action == "echo_response" && m.callbackId == callbackId), 
                "Should receive echo response from JavaScript");
        }
    }
}
