using System;
using NUnit.Framework;
using UnityEngine;
using Energy8.JSPluginTools.Core;
using UnityEngine.TestTools;
using System.Collections.Generic;

namespace Energy8.JSPluginTools.Tests
{
    /// <summary>
    /// Тесты для проверки функциональности обработчиков JS сообщений
    /// </summary>
    public class JSMessageHandlerTests
    {
        // Тестовый модуль, реализующий интерфейс IJSMessageHandler
        private class TestMessageHandlerModule : IPluginModule, IJSMessageHandler
        {
            public string ModuleId => "testModule";
            public bool IsInitialized { get; private set; }
            
            public List<JSMessage> ReceivedMessages { get; private set; } = new List<JSMessage>();

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
                if (message == null)
                {
                    throw new ArgumentNullException(nameof(message));
                }
                
                ReceivedMessages.Add(message);
                
                // Обработка различных типов действий
                switch (message.action)
                {
                    case "echo":
                        // Просто отправляем сообщение обратно в JS
                        ExternalCommunicator.SendMessageToJS(ModuleId, "echo_response", message.data, message.callbackId);
                        break;
                    case "error":
                        // Симулируем ошибку
                        throw new Exception("Test exception from HandleJSMessage");
                    default:
                        // По умолчанию ничего не делаем
                        break;
                }
            }
            
            public void Reset()
            {
                ReceivedMessages.Clear();
            }
        }
        
        // Тестовый обработчик JS вызовов для перехвата ответов
        private class TestJSCallHandler : IJSCallHandler
        {
            public string LastFunctionPath { get; private set; }
            public object[] LastArgs { get; private set; }
            public bool WasCalled { get; private set; }
            public List<JSMessage> SentMessages { get; private set; } = new List<JSMessage>();

            public void HandleCall(string functionPath, object[] args)
            {
                LastFunctionPath = functionPath;
                LastArgs = args;
                WasCalled = true;
                
                // Если это вызов для отправки сообщения, сохраняем его
                if (functionPath == "UnityWebPlugin.Core.receiveMessageFromUnity" && args.Length > 0 && args[0] is string)
                {
                    try
                    {
                        var message = JsonUtility.FromJson<JSMessage>((string)args[0]);
                        SentMessages.Add(message);
                    }
                    catch (Exception)
                    {
                        // Игнорируем ошибки при парсинге
                    }
                }
            }

            public void Reset()
            {
                LastFunctionPath = null;
                LastArgs = null;
                WasCalled = false;
                SentMessages.Clear();
            }
        }
        
        private TestMessageHandlerModule _testModule;
        private TestJSCallHandler _testCallHandler;
        
        [SetUp]
        public void SetUp()
        {
            _testModule = new TestMessageHandlerModule();
            _testCallHandler = new TestJSCallHandler();
            
            // Устанавливаем тестовый обработчик JS вызовов
            var methodInfo = typeof(ExternalCommunicator).GetMethod("SetCallHandler", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            methodInfo.Invoke(null, new object[] { _testCallHandler });
        }
        
        [TearDown]
        public void TearDown()
        {
            // Восстанавливаем стандартный обработчик JS вызовов
            var methodInfo = typeof(ExternalCommunicator).GetMethod("SetCallHandler", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            methodInfo.Invoke(null, new object[] { null });
        }
        
        [Test]
        public void HandleJSMessage_ValidMessage_ShouldAddToReceivedMessages()
        {
            // Arrange
            var message = new JSMessage
            {
                moduleId = "testModule",
                action = "test",
                data = "{\"key\":\"value\"}",
                callbackId = "callback123"
            };
            
            // Act
            _testModule.HandleJSMessage(message);
            
            // Assert
            Assert.AreEqual(1, _testModule.ReceivedMessages.Count, "Message should be added to received messages");
            Assert.AreEqual(message, _testModule.ReceivedMessages[0], "Received message should match sent message");
        }
        
        [Test]
        public void HandleJSMessage_NullMessage_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _testModule.HandleJSMessage(null));
        }
        
        [Test]
        public void HandleJSMessage_EchoAction_ShouldSendResponseBack()
        {
            // Arrange
            var message = new JSMessage
            {
                moduleId = "testModule",
                action = "echo",
                data = "{\"echo\":\"test\"}",
                callbackId = "echo123"
            };
            
            // Act
            _testModule.HandleJSMessage(message);
            
            // Assert
            Assert.IsTrue(_testCallHandler.WasCalled, "ExternalCommunicator should call JS");
            Assert.AreEqual(1, _testCallHandler.SentMessages.Count, "One message should be sent");
            
            var sentMessage = _testCallHandler.SentMessages[0];
            Assert.AreEqual("testModule", sentMessage.moduleId, "Module ID should match");
            Assert.AreEqual("echo_response", sentMessage.action, "Action should be echo_response");
            Assert.AreEqual("{\"echo\":\"test\"}", sentMessage.data, "Data should match original message");
            Assert.AreEqual("echo123", sentMessage.callbackId, "Callback ID should match original message");
        }
        
        [Test]
        public void HandleJSMessage_ErrorAction_ShouldThrowException()
        {
            // Arrange
            var message = new JSMessage
            {
                moduleId = "testModule",
                action = "error",
                data = null,
                callbackId = null
            };
            
            // Act & Assert
            Exception ex = Assert.Throws<Exception>(() => _testModule.HandleJSMessage(message));
            Assert.AreEqual("Test exception from HandleJSMessage", ex.Message);
        }
        
        [Test]
        public void HandleJSMessage_UnknownAction_ShouldAddToReceivedButNotRespond()
        {
            // Arrange
            var message = new JSMessage
            {
                moduleId = "testModule",
                action = "unknown",
                data = "{\"test\":\"data\"}",
                callbackId = "unknown123"
            };
            
            // Act
            _testModule.HandleJSMessage(message);
            
            // Assert
            Assert.AreEqual(1, _testModule.ReceivedMessages.Count, "Message should be added to received messages");
            Assert.AreEqual(0, _testCallHandler.SentMessages.Count, "No messages should be sent for unknown action");
        }
    }
}
