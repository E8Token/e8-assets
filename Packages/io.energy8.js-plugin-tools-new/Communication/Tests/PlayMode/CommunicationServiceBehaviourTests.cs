using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using Energy8.JSPluginTools.Core;
using UnityEngine.TestTools;

namespace Energy8.JSPluginTools.Communication.Tests.PlayMode
{
    public class CommunicationServiceBehaviourTests
    {
        private GameObject _gameObject;
        private CommunicationServiceBehaviour _behaviour;
        private MockCommunicationService _service;

        // Test implementation of ICommunicationService
        private class MockCommunicationService : ICommunicationService
        {
            public bool WasUsed { get; private set; }
            public IMessageBus MessageBus { get; } = new MockMessageBus();

            public void Send(string channel, string data)
            {
                WasUsed = true;
            }

            public void Send<T>(string channel, T data)
            {
                WasUsed = true;
            }

            public System.Threading.Tasks.Task<TResponse> SendWithResponseAsync<TResponse>(string channel, string data, int timeout = 5000)
            {
                WasUsed = true;
                return System.Threading.Tasks.Task.FromResult(default(TResponse));
            }

            public System.Threading.Tasks.Task<TResponse> SendWithResponseAsync<TRequest, TResponse>(string channel, TRequest data, int timeout = 5000)
            {
                WasUsed = true;
                return System.Threading.Tasks.Task.FromResult(default(TResponse));
            }

            public void Subscribe(string channel, Action<string> handler)
            {
                WasUsed = true;
            }

            public void Subscribe<T>(string channel, Action<T> handler)
            {
                WasUsed = true;
            }

            public void Unsubscribe(string channel)
            {
                WasUsed = true;
            }
        }

        private class MockMessageBus : IMessageBus
        {
            public void RegisterMessageHandler(string messageType, Action<string> handler) { }
            public void RegisterMessageHandler<T>(string messageType, Action<T> handler) { }
            public void SendMessage(string messageType, string payload) { }
            public void SendMessage<T>(string messageType, T payload) { }
            public void SendMessageWithResponse<TRequest, TResponse>(string messageType, TRequest payload, Action<TResponse> callback) { }
            public void UnregisterMessageHandler(string messageType) { }
        }

        [SetUp]
        public void Setup()
        {
            _gameObject = new GameObject("TestCommunicationServiceBehaviour");
            _behaviour = _gameObject.AddComponent<CommunicationServiceBehaviour>();
            _service = new MockCommunicationService();
        }

        [TearDown]
        public void TearDown()
        {
            if (_gameObject != null)
            {
                UnityEngine.Object.Destroy(_gameObject);
            }
        }

        [UnityTest]
        public IEnumerator Initialize_SetsServiceProperty()
        {
            // Act
            _behaviour.Initialize(_service);
            yield return null;

            // Assert
            Assert.AreEqual(_service, _behaviour.Service);
        }

        [UnityTest]
        public IEnumerator Initialize_WithNullService_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _behaviour.Initialize(null));
            yield return null;
        }

        [UnityTest]
        public IEnumerator CreateInstance_CreatesNewGameObjectWithBehaviour()
        {
            // Act
            var instance = CommunicationServiceBehaviour.CreateInstance(_service);
            yield return null;

            // Assert
            Assert.IsNotNull(instance);
            Assert.AreEqual(_service, instance.Service);

            // Cleanup
            UnityEngine.Object.Destroy(instance.gameObject);
        }

        [UnityTest]
        public IEnumerator CreateInstance_WhenInstanceExists_ReusesExistingInstance()
        {
            // Arrange
            _behaviour.Initialize(_service);
            
            // Act
            var newInstance = CommunicationServiceBehaviour.CreateInstance(new MockCommunicationService());
            yield return null;

            // Assert
            Assert.AreEqual(_behaviour, newInstance, "Should reuse existing instance");
            Assert.AreNotEqual(_service, newInstance.Service, "Service should be updated");
        }

        [UnityTest]
        public IEnumerator CreateInstance_AppliesDontDestroyOnLoad()
        {
            // Act
            var instance = CommunicationServiceBehaviour.CreateInstance(_service);
            yield return null;

            // Assert
            // Check if the instance's gameObject has the DontDestroyOnLoad flag
            // This is a bit tricky to test directly, so we'll check if it's in the DontDestroyOnLoad scene
            Assert.AreEqual(true, instance.gameObject.scene.buildIndex == -1, 
                "GameObject should be in DontDestroyOnLoad scene");

            // Cleanup
            UnityEngine.Object.Destroy(instance.gameObject);
        }

        [UnityTest]
        public IEnumerator CreateInstance_WithCustomNameParameter_SetsGameObjectName()
        {
            // Arrange
            string customName = "CustomCommunicationServiceName";

            // Act
            var instance = CommunicationServiceBehaviour.CreateInstance(_service, customName);
            yield return null;

            // Assert
            Assert.AreEqual(customName, instance.gameObject.name);

            // Cleanup
            UnityEngine.Object.Destroy(instance.gameObject);
        }
    }
}