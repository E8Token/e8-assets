using System;
using NUnit.Framework;
using Energy8.JSPluginTools.Core;

namespace Energy8.JSPluginTools.Communication.Tests.EditMode
{
    public class CommunicationServiceTests
    {
        private TestMessageBus _messageBus;
        private CommunicationService _communicationService;

        [SetUp]
        public void Setup()
        {
            _messageBus = new TestMessageBus();
            _communicationService = new CommunicationService(_messageBus);
        }

        // Simple IMessageBus implementation for testing
        private class TestMessageBus : IMessageBus
        {
            public string LastMessageType { get; private set; }
            public string LastStringPayload { get; private set; }
            public object LastObjectPayload { get; private set; }
            public bool RegisterHandlerCalled { get; private set; }
            public bool UnregisterHandlerCalled { get; private set; }

            public void RegisterMessageHandler(string messageType, Action<string> handler)
            {
                RegisterHandlerCalled = true;
                LastMessageType = messageType;
            }

            public void RegisterMessageHandler<T>(string messageType, Action<T> handler)
            {
                RegisterHandlerCalled = true;
                LastMessageType = messageType;
            }

            public void SendMessage(string messageType, string payload)
            {
                LastMessageType = messageType;
                LastStringPayload = payload;
            }

            public void SendMessage<T>(string messageType, T payload)
            {
                LastMessageType = messageType;
                LastObjectPayload = payload;
            }

            public void SendMessageWithResponse<TRequest, TResponse>(string messageType, TRequest payload, Action<TResponse> callback)
            {
                LastMessageType = messageType;
                LastObjectPayload = payload;
            }

            public void UnregisterMessageHandler(string messageType)
            {
                UnregisterHandlerCalled = true;
                LastMessageType = messageType;
            }
        }

        [Test]
        public void Constructor_NullMessageBus_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new CommunicationService(null));
        }

        [Test]
        public void Send_StringData_CallsMessageBusSendMessage()
        {
            // Arrange
            string channel = "testChannel";
            string data = "testData";

            // Act
            _communicationService.Send(channel, data);

            // Assert
            Assert.AreEqual(channel, _messageBus.LastMessageType);
            Assert.AreEqual(data, _messageBus.LastStringPayload);
        }

        [Test]
        public void Send_GenericData_CallsMessageBusSendMessage()
        {
            // Arrange
            string channel = "testChannel";
            var data = new { id = 1, name = "test" };

            // Act
            _communicationService.Send(channel, data);

            // Assert
            Assert.AreEqual(channel, _messageBus.LastMessageType);
            Assert.AreEqual(data, _messageBus.LastObjectPayload);
        }

        [Test]
        public void Send_NullOrEmptyChannel_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => _communicationService.Send(null, "data"));
            Assert.Throws<ArgumentException>(() => _communicationService.Send("", "data"));
            Assert.Throws<ArgumentException>(() => _communicationService.Send<object>(null, new object()));
            Assert.Throws<ArgumentException>(() => _communicationService.Send<object>("", new object()));
        }

        [Test]
        public void Subscribe_StringHandler_CallsMessageBusRegisterHandler()
        {
            // Arrange
            string channel = "testChannel";
            Action<string> handler = (data) => { };

            // Act
            _communicationService.Subscribe(channel, handler);

            // Assert
            Assert.IsTrue(_messageBus.RegisterHandlerCalled);
            Assert.AreEqual(channel, _messageBus.LastMessageType);
        }

        [Test]
        public void Subscribe_GenericHandler_CallsMessageBusRegisterHandler()
        {
            // Arrange
            string channel = "testChannel";
            Action<object> handler = (data) => { };

            // Act
            _communicationService.Subscribe<object>(channel, handler);

            // Assert
            Assert.IsTrue(_messageBus.RegisterHandlerCalled);
            Assert.AreEqual(channel, _messageBus.LastMessageType);
        }

        [Test]
        public void Subscribe_NullOrEmptyChannel_ThrowsArgumentException()
        {
            // Arrange
            Action<string> stringHandler = (data) => { };
            Action<object> genericHandler = (data) => { };

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _communicationService.Subscribe(null, stringHandler));
            Assert.Throws<ArgumentException>(() => _communicationService.Subscribe("", stringHandler));
            Assert.Throws<ArgumentException>(() => _communicationService.Subscribe<object>(null, genericHandler));
            Assert.Throws<ArgumentException>(() => _communicationService.Subscribe<object>("", genericHandler));
        }

        [Test]
        public void Subscribe_NullHandler_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _communicationService.Subscribe("channel", (Action<string>)null));
            Assert.Throws<ArgumentNullException>(() => _communicationService.Subscribe<object>("channel", null));
        }

        [Test]
        public void Unsubscribe_ValidChannel_CallsMessageBusUnregisterHandler()
        {
            // Arrange
            string channel = "testChannel";

            // Act
            _communicationService.Unsubscribe(channel);

            // Assert
            Assert.IsTrue(_messageBus.UnregisterHandlerCalled);
            Assert.AreEqual(channel, _messageBus.LastMessageType);
        }

        [Test]
        public void Unsubscribe_NullOrEmptyChannel_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => _communicationService.Unsubscribe(null));
            Assert.Throws<ArgumentException>(() => _communicationService.Unsubscribe(""));
        }

        // Additional tests for SendWithResponseAsync methods would need Tasks and async processing
        // We'll add simplified versions that just test the parameter validation
        
        [Test]
        public void SendWithResponseAsync_String_NullOrEmptyChannel_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(
                async () => await _communicationService.SendWithResponseAsync<object>(null, "data"));
            Assert.ThrowsAsync<ArgumentException>(
                async () => await _communicationService.SendWithResponseAsync<object>("", "data"));
        }
        
        [Test]
        public void SendWithResponseAsync_Generic_NullOrEmptyChannel_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(
                async () => await _communicationService.SendWithResponseAsync<object, object>(null, new object()));
            Assert.ThrowsAsync<ArgumentException>(
                async () => await _communicationService.SendWithResponseAsync<object, object>("", new object()));
        }
        
        [Test]
        public void SendWithResponseAsync_InvalidTimeout_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(
                async () => await _communicationService.SendWithResponseAsync<object>("channel", "data", 0));
            Assert.ThrowsAsync<ArgumentException>(
                async () => await _communicationService.SendWithResponseAsync<object>("channel", "data", -1));
            Assert.ThrowsAsync<ArgumentException>(
                async () => await _communicationService.SendWithResponseAsync<object, object>("channel", new object(), 0));
            Assert.ThrowsAsync<ArgumentException>(
                async () => await _communicationService.SendWithResponseAsync<object, object>("channel", new object(), -1));
        }
    }
}