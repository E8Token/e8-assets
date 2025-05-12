using System;
using NUnit.Framework;
using Energy8.JSPluginTools.Core.Implementation;

namespace Energy8.JSPluginTools.Core.Tests.EditMode
{
    public class PluginCoreTests
    {
        private TestMemoryManager _memoryManager;
        private TestMessageBus _messageBus;
        private PluginCore _pluginCore;

        [SetUp]
        public void Setup()
        {
            _memoryManager = new TestMemoryManager();
            _messageBus = new TestMessageBus(_memoryManager);
            _pluginCore = new PluginCore(_memoryManager, _messageBus);
        }

        // Простая реализация IMemoryManager для тестирования
        private class TestMemoryManager : IMemoryManager
        {
            public bool ReleaseAllCalled { get; private set; }

            public IntPtr Allocate(int size) => new IntPtr(1);
            public void Free(IntPtr ptr) { }
            public void CopyToUnmanaged(byte[] source, IntPtr destination, int length) { }
            public void CopyToManaged(IntPtr source, byte[] destination, int length) { }
            public IntPtr StringToPtr(string text) => new IntPtr(1);
            public string PtrToString(IntPtr ptr) => "test";
            
            public void ReleaseAll() 
            {
                ReleaseAllCalled = true;
            }
        }

        // Простая реализация IMessageBus для тестирования
        private class TestMessageBus : IMessageBus
        {
            private readonly IMemoryManager _memoryManager;

            public TestMessageBus(IMemoryManager memoryManager)
            {
                _memoryManager = memoryManager;
            }

            public void RegisterMessageHandler(string messageType, Action<string> handler) { }
            public void RegisterMessageHandler<T>(string messageType, Action<T> handler) { }
            public void SendMessage(string messageType, string payload) { }
            public void SendMessage<T>(string messageType, T payload) { }
            public void SendMessageWithResponse<TRequest, TResponse>(string messageType, TRequest payload, Action<TResponse> callback) { }
            public void UnregisterMessageHandler(string messageType) { }
        }

        [Test]
        public void Constructor_NullMemoryManager_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new PluginCore(null, _messageBus));
        }

        [Test]
        public void Constructor_NullMessageBus_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new PluginCore(_memoryManager, null));
        }

        [Test]
        public void Initialize_WhenNotInitialized_SetsIsInitializedToTrue()
        {
            // Arrange
            Assert.IsFalse(_pluginCore.IsInitialized, "Plugin should start uninitialized");

            // Act
            _pluginCore.Initialize();

            // Assert
            Assert.IsTrue(_pluginCore.IsInitialized, "Plugin should be initialized after Initialize call");
        }

        [Test]
        public void Initialize_WhenAlreadyInitialized_DoesNotThrowException()
        {
            // Arrange
            _pluginCore.Initialize();

            // Act & Assert
            Assert.DoesNotThrow(() => _pluginCore.Initialize(), "Initialize should not throw when called multiple times");
        }

        [Test]
        public void Initialize_WhenCalled_TriggersOnInitializedEvent()
        {
            // Arrange
            bool eventFired = false;
            _pluginCore.OnInitialized += () => eventFired = true;

            // Act
            _pluginCore.Initialize();

            // Assert
            Assert.IsTrue(eventFired, "OnInitialized event should be triggered");
        }

        [Test]
        public void Shutdown_WhenInitialized_SetsIsInitializedToFalse()
        {
            // Arrange
            _pluginCore.Initialize();
            Assert.IsTrue(_pluginCore.IsInitialized, "Plugin should be initialized");

            // Act
            _pluginCore.Shutdown();

            // Assert
            Assert.IsFalse(_pluginCore.IsInitialized, "Plugin should not be initialized after Shutdown call");
        }

        [Test]
        public void Shutdown_WhenNotInitialized_DoesNotThrowException()
        {
            // Act & Assert
            Assert.DoesNotThrow(() => _pluginCore.Shutdown(), "Shutdown should not throw when called on uninitialized plugin");
        }

        [Test]
        public void Shutdown_WhenCalled_TriggersOnShutdownEvent()
        {
            // Arrange
            _pluginCore.Initialize();
            bool eventFired = false;
            _pluginCore.OnShutdown += () => eventFired = true;

            // Act
            _pluginCore.Shutdown();

            // Assert
            Assert.IsTrue(eventFired, "OnShutdown event should be triggered");
        }

        [Test]
        public void Shutdown_WhenCalled_CallsMemoryManagerReleaseAll()
        {
            // Arrange
            _pluginCore.Initialize();

            // Act
            _pluginCore.Shutdown();

            // Assert
            Assert.IsTrue(_memoryManager.ReleaseAllCalled, "ReleaseAll should be called on memory manager");
        }

        [Test]
        public void RegisterGameObject_WithValidName_ReturnsNonEmptyId()
        {
            // Arrange
            string gameObjectName = "TestGameObject";

            // Act
            string objectId = _pluginCore.RegisterGameObject(gameObjectName);

            // Assert
            Assert.IsFalse(string.IsNullOrEmpty(objectId), "Object ID should not be null or empty");
            Assert.IsTrue(objectId.StartsWith("jsplugintoolsobj_"), "Object ID should have the correct prefix");
        }

        [Test]
        public void RegisterGameObject_WithNullOrEmptyName_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => _pluginCore.RegisterGameObject(null));
            Assert.Throws<ArgumentException>(() => _pluginCore.RegisterGameObject(""));
        }

        [Test]
        public void UnregisterGameObject_WithValidId_DoesNotThrowException()
        {
            // Arrange
            string gameObjectName = "TestGameObject";
            string objectId = _pluginCore.RegisterGameObject(gameObjectName);

            // Act & Assert
            Assert.DoesNotThrow(() => _pluginCore.UnregisterGameObject(objectId));
        }

        [Test]
        public void UnregisterGameObject_WithNullOrEmptyId_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => _pluginCore.UnregisterGameObject(null));
            Assert.Throws<ArgumentException>(() => _pluginCore.UnregisterGameObject(""));
        }

        [Test]
        public void UnregisterGameObject_WithInvalidId_DoesNotThrowException()
        {
            // Act & Assert
            Assert.DoesNotThrow(() => _pluginCore.UnregisterGameObject("invalid_id"));
        }

        [Test]
        public void IsWebGLContext_InUnityEditor_ReturnsFalse()
        {
            // Act & Assert
            Assert.IsFalse(_pluginCore.IsWebGLContext, "IsWebGLContext should return false in non-WebGL context");
        }
    }
}