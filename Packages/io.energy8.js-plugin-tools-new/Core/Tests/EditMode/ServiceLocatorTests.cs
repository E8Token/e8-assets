using System;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Energy8.JSPluginTools.Core;

namespace Energy8.JSPluginTools.Core.Tests.EditMode
{
    public class ServiceLocatorTests
    {
        [Test]
        public void FindOrCreateServiceBehaviour_WhenBehaviourDoesntExist_CreatesNewGameObject()
        {
            // Arrange
            string expectedGameObjectName = "TestServiceObject";
            
            // Ensure game object doesn't exist from previous tests
            var existingObject = GameObject.Find(expectedGameObjectName);
            if (existingObject != null)
            {
                UnityEngine.Object.DestroyImmediate(existingObject);
            }

            // Act
            var mockService = new MockService();
            var behaviour = ServiceLocator.FindOrCreateServiceBehaviour<MockServiceBehaviour, MockService>(
                mockService, expectedGameObjectName);

            // Assert
            Assert.IsNotNull(behaviour, "Service behaviour should be created");
            Assert.IsNotNull(GameObject.Find(expectedGameObjectName), "Game object should be created");
            Assert.AreEqual(expectedGameObjectName, behaviour.gameObject.name, "Game object should have the expected name");
            Assert.IsTrue(behaviour.IsInitialized, "Service behaviour should be initialized");
            Assert.AreEqual(mockService, behaviour.Service, "Service behaviour should be initialized with the correct service");

            // Cleanup
            UnityEngine.Object.DestroyImmediate(behaviour.gameObject);
        }

        [Test]
        public void FindOrCreateServiceBehaviour_WhenBehaviourExists_ReturnsBehaviour()
        {
            // Arrange
            string gameObjectName = "TestExistingServiceObject";
            var gameObject = new GameObject(gameObjectName);
            var existingBehaviour = gameObject.AddComponent<MockServiceBehaviour>();
            
            // Act
            var mockService = new MockService();
            var returnedBehaviour = ServiceLocator.FindOrCreateServiceBehaviour<MockServiceBehaviour, MockService>(
                mockService, gameObjectName);

            // Assert
            Assert.IsNotNull(returnedBehaviour, "Service behaviour should be found");
            Assert.AreEqual(existingBehaviour, returnedBehaviour, "Existing behaviour should be returned");
            Assert.IsTrue(returnedBehaviour.IsInitialized, "Service behaviour should be initialized");
            Assert.AreEqual(mockService, returnedBehaviour.Service, "Service behaviour should be initialized with the correct service");

            // Cleanup
            UnityEngine.Object.DestroyImmediate(gameObject);
        }

        [Test]
        public void FindOrCreateServiceBehaviour_WithDefaultGameObjectName_CreatesJSPluginToolsGameObject()
        {
            // Arrange
            string expectedDefaultName = "JSPluginTools";
            
            // Ensure game object doesn't exist from previous tests
            var existingObject = GameObject.Find(expectedDefaultName);
            if (existingObject != null)
            {
                UnityEngine.Object.DestroyImmediate(existingObject);
            }

            // Act
            var mockService = new MockService();
            var behaviour = ServiceLocator.FindOrCreateServiceBehaviour<MockServiceBehaviour, MockService>(mockService);

            // Assert
            Assert.IsNotNull(behaviour, "Service behaviour should be created");
            Assert.IsNotNull(GameObject.Find(expectedDefaultName), "Default game object should be created");
            Assert.AreEqual(expectedDefaultName, behaviour.gameObject.name, "Game object should have the default name");

            // Cleanup
            UnityEngine.Object.DestroyImmediate(behaviour.gameObject);
        }

        // Mock classes for testing
        private class MockService {}

        private class MockServiceBehaviour : MonoBehaviour
        {
            public bool IsInitialized { get; private set; }
            public MockService Service { get; private set; }

            public void Initialize(MockService service)
            {
                Service = service;
                IsInitialized = true;
            }
        }
    }
}