using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Energy8.JSPluginTools.Core;
using Energy8.JSPluginTools.Core.Implementation;

namespace Energy8.JSPluginTools.Communication.Tests.EditMode
{
    public class CommunicationManagerTests
    {
        private MockPluginCore _pluginCore;
        private CommunicationManager _communicationManager;
        private MockMessageBus _messageBus;

        [SetUp]
        public void Setup()
        {
            _messageBus = new MockMessageBus();
            var memoryManager = new MockMemoryManager();
            _pluginCore = new MockPluginCore(memoryManager, _messageBus);
            _communicationManager = new CommunicationManager();
        }

        // Mock implementation that inherits from PluginCore for testing
        private class MockPluginCore : PluginCore
        {
            public MockPluginCore(IMemoryManager memoryManager, IMessageBus messageBus) 
                : base(memoryManager, messageBus)
            {
                // Initialize with mocked dependencies
            }

            // Only override what's necessary
            public new bool IsInitialized => true;
            public new bool IsWebGLContext => false;

            // Override methods to do nothing
            public new void Initialize() { }
            public new string RegisterGameObject(string gameObjectName) => "mock_id";
            public new void Shutdown() { }
            public new void UnregisterGameObject(string objectId) { }
        }

        private class MockMemoryManager : IMemoryManager
        {
            public IntPtr Allocate(int size) => IntPtr.Zero;
            public void CopyToManaged(IntPtr source, byte[] destination, int length) { }
            public void CopyToUnmanaged(byte[] source, IntPtr destination, int length) { }
            public void Free(IntPtr ptr) { }
            public string PtrToString(IntPtr ptr) => "";
            public void ReleaseAll() { }
            public IntPtr StringToPtr(string text) => IntPtr.Zero;
        }

        private class MockMessageBus : IMessageBus
        {
            public string LastMessageType { get; private set; }
            public object LastPayload { get; private set; }
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
                LastPayload = payload;
            }

            public void SendMessage<T>(string messageType, T payload)
            {
                LastMessageType = messageType;
                LastPayload = payload;
            }

            public void SendMessageWithResponse<TRequest, TResponse>(string messageType, TRequest payload, Action<TResponse> callback)
            {
                LastMessageType = messageType;
                LastPayload = payload;
                // In a real implementation, we'd capture the callback and call it, but for testing we'll skip that
            }

            public void UnregisterMessageHandler(string messageType)
            {
                UnregisterHandlerCalled = true;
                LastMessageType = messageType;
            }
        }

        [Test]
        public void Initialize_WithNullCore_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _communicationManager.Initialize(null));
        }

        [Test]
        public void Initialize_WhenNotInitialized_SetsIsInitializedToTrue()
        {
            // Arrange
            Assert.IsFalse(_communicationManager.IsInitialized, "CommunicationManager should start uninitialized");

            // Act
            _communicationManager.Initialize(_pluginCore);

            // Assert
            Assert.IsTrue(_communicationManager.IsInitialized, "CommunicationManager should be initialized after Initialize call");
        }

        [Test]
        public void Initialize_WhenAlreadyInitialized_DoesNotReinitialize()
        {
            // Arrange
            _communicationManager.Initialize(_pluginCore);
            bool eventFired = false;
            _communicationManager.OnInitialized += () => eventFired = true;

            // Act
            _communicationManager.Initialize(_pluginCore);

            // Assert
            Assert.IsFalse(eventFired, "OnInitialized should not fire when already initialized");
        }

        [Test]
        public void Initialize_WhenCalled_TriggersOnInitializedEvent()
        {
            // Arrange
            bool eventFired = false;
            _communicationManager.OnInitialized += () => eventFired = true;

            // Act
            _communicationManager.Initialize(_pluginCore);

            // Assert
            Assert.IsTrue(eventFired, "OnInitialized event should be triggered");
        }

        [Test]
        public void Send_WhenNotInitialized_ThrowsInvalidOperationException()
        {
            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => _communicationManager.Send("channel", "data"));
        }

        [Test]
        public void Send_WithNullOrEmptyChannel_ThrowsArgumentException()
        {
            // Arrange
            _communicationManager.Initialize(_pluginCore);

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _communicationManager.Send<string>(null, "data"));
            Assert.Throws<ArgumentException>(() => _communicationManager.Send<string>("", "data"));
        }

        [Test]
        public void Send_WithValidChannel_SendsMessageWithPrefix()
        {
            // Arrange
            _communicationManager.Initialize(_pluginCore);
            string channel = "testChannel";
            string data = "testData";
            string expectedMessageType = "comm.testChannel"; // Should add the prefix

            // Act
            _communicationManager.Send(channel, data);

            // Assert
            var messageBus = (MockMessageBus)_pluginCore.MessageBus;
            Assert.AreEqual(expectedMessageType, messageBus.LastMessageType);
            Assert.AreEqual(data, messageBus.LastPayload);
        }

        [Test]
        public void SendAsync_WhenNotInitialized_ThrowsInvalidOperationException()
        {
            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _communicationManager.SendAsync<string, string>("channel", "data"));
        }

        [Test]
        public void SendAsync_WithNullOrEmptyChannel_ThrowsArgumentException()
        {
            // Arrange
            _communicationManager.Initialize(_pluginCore);

            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(
                async () => await _communicationManager.SendAsync<string, string>(null, "data"));
            Assert.ThrowsAsync<ArgumentException>(
                async () => await _communicationManager.SendAsync<string, string>("", "data"));
        }

        [Test]
        public void RegisterHandler_WhenNotInitialized_ThrowsInvalidOperationException()
        {
            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => _communicationManager.RegisterHandler<string>("channel", data => { }));
        }

        [Test]
        public void RegisterHandler_WithNullOrEmptyChannel_ThrowsArgumentException()
        {
            // Arrange
            _communicationManager.Initialize(_pluginCore);
            Action<string> handler = data => { };

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _communicationManager.RegisterHandler<string>(null, handler));
            Assert.Throws<ArgumentException>(() => _communicationManager.RegisterHandler<string>("", handler));
        }

        [Test]
        public void RegisterHandler_WithNullHandler_ThrowsArgumentNullException()
        {
            // Arrange
            _communicationManager.Initialize(_pluginCore);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(
                () => _communicationManager.RegisterHandler<string>("channel", null));
        }

        [Test]
        public void RegisterHandler_WithValidParameters_RegistersMessageHandler()
        {
            // Arrange
            _communicationManager.Initialize(_pluginCore);
            string channel = "testChannel";
            Action<string> handler = data => { };
            string expectedMessageType = "comm.testChannel"; // Should add the prefix

            // Act
            _communicationManager.RegisterHandler(channel, handler);

            // Assert
            var messageBus = (MockMessageBus)_pluginCore.MessageBus;
            Assert.IsTrue(messageBus.RegisterHandlerCalled);
            Assert.AreEqual(expectedMessageType, messageBus.LastMessageType);
        }

        [Test]
        public void UnregisterHandler_WhenNotInitialized_ThrowsInvalidOperationException()
        {
            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => _communicationManager.UnregisterHandler("channel"));
        }

        [Test]
        public void UnregisterHandler_WithNullOrEmptyChannel_ThrowsArgumentException()
        {
            // Arrange
            _communicationManager.Initialize(_pluginCore);

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _communicationManager.UnregisterHandler(null));
            Assert.Throws<ArgumentException>(() => _communicationManager.UnregisterHandler(""));
        }

        [Test]
        public void UnregisterHandler_WithValidChannel_UnregistersMessageHandler()
        {
            // Arrange
            _communicationManager.Initialize(_pluginCore);
            string channel = "testChannel";
            string expectedMessageType = "comm.testChannel"; // Should add the prefix

            // Act
            _communicationManager.UnregisterHandler(channel);

            // Assert
            var messageBus = (MockMessageBus)_pluginCore.MessageBus;
            Assert.IsTrue(messageBus.UnregisterHandlerCalled);
            Assert.AreEqual(expectedMessageType, messageBus.LastMessageType);
        }
    }
}