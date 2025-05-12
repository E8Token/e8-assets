using System;
using NUnit.Framework;
using UnityEngine;

namespace Energy8.JSPluginTools.Core.Tests.PlayMode
{
    public class BaseServiceBehaviourTests
    {
        private class TestServiceBehaviour : BaseServiceBehaviour
        {
            public bool WasDestroyed { get; private set; }
            
            // Override OnDestroy to make it public for testing
            public void PublicOnDestroy()
            {
                base.OnDestroy();
                WasDestroyed = true;
            }
            
            // Helper method to expose the protected CheckInitialized method
            public void TestCheckInitialized()
            {
                CheckInitialized();
            }
        }
        
        private GameObject _gameObject;
        private TestServiceBehaviour _serviceComponent;
        private TestPluginCore _mockPluginCore;

        [SetUp]
        public void Setup()
        {
            _gameObject = new GameObject("TestServiceGameObject");
            _serviceComponent = _gameObject.AddComponent<TestServiceBehaviour>();
            _mockPluginCore = new TestPluginCore();
        }

        [TearDown]
        public void TearDown()
        {
            if (_gameObject != null)
            {
                UnityEngine.Object.DestroyImmediate(_gameObject);
            }
        }

        // Тестовая реализация IPluginCore для замены NSubstitute
        private class TestPluginCore : IPluginCore
        {
            public IMemoryManager MemoryManager => null;
            public IMessageBus MessageBus => null;
            public bool IsInitialized => true;
            public bool IsWebGLContext => false;
            public event Action OnInitialized;
            public event Action OnShutdown;
            public void Initialize() { }
            public string RegisterGameObject(string gameObjectName) => "test_id";
            public void Shutdown() { }
            public void UnregisterGameObject(string objectId) { }
        }

        [Test]
        public void Initialize_WithValidPluginCore_SetsIsInitializedToTrue()
        {
            // Arrange
            Assert.IsFalse(_serviceComponent.IsInitialized, "Component should start uninitialized");
            
            // Act
            _serviceComponent.Initialize(_mockPluginCore);
            
            // Assert
            Assert.IsTrue(_serviceComponent.IsInitialized, "IsInitialized should be true after initialization");
        }
        
        [Test]
        public void Initialize_WithNullPluginCore_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _serviceComponent.Initialize(null));
        }
        
        [Test]
        public void CheckInitialized_WhenNotInitialized_ThrowsInvalidOperationException()
        {
            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => _serviceComponent.TestCheckInitialized());
        }
        
        [Test]
        public void CheckInitialized_WhenInitialized_DoesNotThrowException()
        {
            // Arrange
            _serviceComponent.Initialize(_mockPluginCore);
            
            // Act & Assert
            Assert.DoesNotThrow(() => _serviceComponent.TestCheckInitialized());
        }
        
        [Test]
        public void OnDestroy_SetsIsInitializedToFalse()
        {
            // Arrange
            _serviceComponent.Initialize(_mockPluginCore);
            Assert.IsTrue(_serviceComponent.IsInitialized, "Component should be initialized");
            
            // Act
            _serviceComponent.PublicOnDestroy();
            
            // Assert
            Assert.IsFalse(_serviceComponent.IsInitialized, "IsInitialized should be false after destruction");
            Assert.IsTrue(_serviceComponent.WasDestroyed, "WasDestroyed flag should be set");
        }
    }
}