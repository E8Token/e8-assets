using System;
using System.Collections.Generic;
using NUnit.Framework;
using Energy8.JSPluginTools.Core.Implementation;

namespace Energy8.JSPluginTools.Core.Tests.EditMode
{
    public class MessageBusTests
    {
        private TestMemoryManager _memoryManager;
        private MessageBus _messageBus;

        [SetUp]
        public void Setup()
        {
            _memoryManager = new TestMemoryManager();
            _messageBus = new MessageBus(_memoryManager);
        }

        // Простая реализация IMemoryManager для тестирования
        private class TestMemoryManager : IMemoryManager
        {
            public IntPtr Allocate(int size) => new IntPtr(1);
            public void Free(IntPtr ptr) { }
            public void CopyToUnmanaged(byte[] source, IntPtr destination, int length) { }
            public void CopyToManaged(IntPtr source, byte[] destination, int length) { }
            public IntPtr StringToPtr(string text) => new IntPtr(1);
            public string PtrToString(IntPtr ptr) => "test";
            public void ReleaseAll() { }
        }

        [Test]
        public void Constructor_NullMemoryManager_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new MessageBus(null));
        }

        [Test]
        public void SendMessage_NullOrEmptyMessageType_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => _messageBus.SendMessage(null, "payload"));
            Assert.Throws<ArgumentException>(() => _messageBus.SendMessage("", "payload"));
        }

        [Test]
        public void SendMessage_ValidParams_LogsMessage()
        {
            // Arrange
            string messageType = "testMessage";
            string payload = "testPayload";

            // Act - No exception should be thrown in non-WebGL environment
            Assert.DoesNotThrow(() => _messageBus.SendMessage(messageType, payload));
        }

        [Test]
        public void SendMessageGeneric_ValidParams_SerializesAndSendsMessage()
        {
            // Arrange
            string messageType = "testObjectMessage";
            var testObject = new { name = "Test", value = 42 };

            // Act - No exception should be thrown in non-WebGL environment
            Assert.DoesNotThrow(() => _messageBus.SendMessage(messageType, testObject));
        }

        [Test]
        public void SendMessageWithResponse_NullOrEmptyMessageType_ThrowsArgumentException()
        {
            // Arrange
            Action<string> callback = s => { };

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _messageBus.SendMessageWithResponse<object, string>(null, new object(), callback));
            Assert.Throws<ArgumentException>(() => _messageBus.SendMessageWithResponse<object, string>("", new object(), callback));
        }

        [Test]
        public void SendMessageWithResponse_NullCallback_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                _messageBus.SendMessageWithResponse<object, string>("testMessage", new object(), null));
        }

        [Test]
        public void SendMessageWithResponse_ValidParams_InvokesCallbackInNonWebGL()
        {
            // Arrange
            string messageType = "testResponseMessage";
            var testObject = new { name = "Test", value = 42 };
            bool callbackInvoked = false;
            
            Action<string> callback = response => {
                callbackInvoked = true;
                Assert.IsNull(response); // Default value for string is null
            };

            // Act
            _messageBus.SendMessageWithResponse<object, string>(messageType, testObject, callback);

            // Assert
            Assert.IsTrue(callbackInvoked, "Callback should be invoked in non-WebGL environment");
        }

        [Test]
        public void RegisterMessageHandler_NullOrEmptyMessageType_ThrowsArgumentException()
        {
            // Arrange
            Action<string> handler = s => { };

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _messageBus.RegisterMessageHandler(null, handler));
            Assert.Throws<ArgumentException>(() => _messageBus.RegisterMessageHandler("", handler));
        }

        [Test]
        public void RegisterMessageHandler_NullHandler_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _messageBus.RegisterMessageHandler("testMessage", null));
        }

        [Test]
        public void RegisterMessageHandler_ValidParams_RegistersHandler()
        {
            // Arrange
            string messageType = "testHandlerMessage";
            string receivedPayload = null;
            Action<string> handler = payload => receivedPayload = payload;

            // Act
            _messageBus.RegisterMessageHandler(messageType, handler);
            _messageBus.HandleMessageFromJS(messageType, "testPayload");

            // Assert
            Assert.AreEqual("testPayload", receivedPayload);
        }

        [Test]
        public void RegisterGenericMessageHandler_ValidParams_DeserializesAndHandles()
        {
            // Arrange
            string messageType = "testGenericHandlerMessage";
            TestData receivedData = null;
            Action<TestData> handler = data => receivedData = data;

            // Act
            _messageBus.RegisterMessageHandler(messageType, handler);
            _messageBus.HandleMessageFromJS(messageType, "{\"Name\":\"Test\",\"Value\":42}");

            // Assert
            Assert.IsNotNull(receivedData);
            Assert.AreEqual("Test", receivedData.Name);
            Assert.AreEqual(42, receivedData.Value);
        }

        [Test]
        public void UnregisterMessageHandler_NullOrEmptyMessageType_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => _messageBus.UnregisterMessageHandler(null));
            Assert.Throws<ArgumentException>(() => _messageBus.UnregisterMessageHandler(""));
        }

        [Test]
        public void UnregisterMessageHandler_ValidMessageType_UnregistersHandler()
        {
            // Arrange
            string messageType = "testUnregisterMessage";
            bool handlerInvoked = false;
            Action<string> handler = payload => handlerInvoked = true;
            
            _messageBus.RegisterMessageHandler(messageType, handler);
            
            // Act
            _messageBus.UnregisterMessageHandler(messageType);
            _messageBus.HandleMessageFromJS(messageType, "testPayload");
            
            // Assert
            Assert.IsFalse(handlerInvoked, "Handler should not be invoked after unregistering");
        }

        [Test]
        public void HandleMessageFromJS_MessageTypeWithNoHandlers_LogsWarning()
        {
            // Act - Should not throw exception
            Assert.DoesNotThrow(() => _messageBus.HandleMessageFromJS("unknownMessageType", "payload"));
        }

        [Test]
        public void HandleMessageFromJS_NullOrEmptyMessageType_LogsWarning()
        {
            // Act - Should not throw exception
            Assert.DoesNotThrow(() => _messageBus.HandleMessageFromJS(null, "payload"));
            Assert.DoesNotThrow(() => _messageBus.HandleMessageFromJS("", "payload"));
        }

        [Test]
        public void HandleResponseFromJS_InvalidCallbackId_LogsWarning()
        {
            // Act - Should not throw exception
            Assert.DoesNotThrow(() => _messageBus.HandleResponseFromJS("invalidCallbackId", "response"));
            Assert.DoesNotThrow(() => _messageBus.HandleResponseFromJS(null, "response"));
            Assert.DoesNotThrow(() => _messageBus.HandleResponseFromJS("", "response"));
        }

        // Тестовый класс для сериализации/десериализации
        private class TestData
        {
            public string Name { get; set; }
            public int Value { get; set; }
        }
    }
}