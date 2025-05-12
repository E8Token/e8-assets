using System;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Energy8.JSPluginTools.Core.Implementation;

namespace Energy8.JSPluginTools.Core.Tests.PlayMode
{
    public class PluginCoreBehaviourTests
    {
        [Test]
        public void Initialize_WithValidCore_SetsCoreProperly()
        {
            // Arrange
            var memoryManager = new MemoryManager();
            var messageBus = new MessageBus(memoryManager);
            var core = new PluginCore(memoryManager, messageBus);
            
            var gameObject = new GameObject("TestPluginCoreBehaviour");
            var behaviour = gameObject.AddComponent<PluginCoreBehaviour>();
            
            // Act
            behaviour.Initialize(core);
            
            // Assert
            Assert.AreEqual(core, behaviour.Core, "Core property should be set to the provided core instance");
            
            // Cleanup
            UnityEngine.Object.DestroyImmediate(gameObject);
        }
        
        [Test]
        public void Initialize_WithNullCore_ThrowsArgumentNullException()
        {
            // Arrange
            var gameObject = new GameObject("TestPluginCoreBehaviour");
            var behaviour = gameObject.AddComponent<PluginCoreBehaviour>();
            
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => behaviour.Initialize(null));
            
            // Cleanup
            UnityEngine.Object.DestroyImmediate(gameObject);
        }
        
        [Test]
        public void CreateInstance_WithNoParameters_CreatesNewInstanceWithNewCore()
        {
            // Arrange & Act
            var behaviour = PluginCoreBehaviour.CreateInstance();
            
            // Assert
            Assert.IsNotNull(behaviour, "Should create a new PluginCoreBehaviour instance");
            Assert.IsNotNull(behaviour.Core, "Core should be automatically created");
            Assert.AreEqual("JSPluginTools_PluginCore", behaviour.gameObject.name, "GameObject should have the default name");
            
            // Cleanup
            UnityEngine.Object.DestroyImmediate(behaviour.gameObject);
        }
        
        [Test]
        public void CreateInstance_WithCustomNameAndCore_CreatesInstanceWithProvidedParameters()
        {
            // Arrange
            var memoryManager = new MemoryManager();
            var messageBus = new MessageBus(memoryManager);
            var core = new PluginCore(memoryManager, messageBus);
            string gameObjectName = "CustomPluginCore";
            
            // Act
            var behaviour = PluginCoreBehaviour.CreateInstance(core, gameObjectName);
            
            // Assert
            Assert.IsNotNull(behaviour, "Should create a new PluginCoreBehaviour instance");
            Assert.AreEqual(core, behaviour.Core, "Core should be set to the provided core instance");
            Assert.AreEqual(gameObjectName, behaviour.gameObject.name, "GameObject should have the custom name");
            
            // Cleanup
            UnityEngine.Object.DestroyImmediate(behaviour.gameObject);
        }
        
        [Test]
        public void CreateInstance_WhenInstanceAlreadyExists_ReturnsExistingInstance()
        {
            // Arrange - Create first instance
            var firstInstance = PluginCoreBehaviour.CreateInstance();
            
            // Act - Create another instance
            var secondInstance = PluginCoreBehaviour.CreateInstance();
            
            // Assert
            Assert.AreEqual(firstInstance, secondInstance, "Should return the existing instance");
            
            // Cleanup
            UnityEngine.Object.DestroyImmediate(firstInstance.gameObject);
        }
        
        [Test]
        public void CreateInstance_WhenInstanceExistsAndCoreProvided_UpdatesExistingInstanceWithNewCore()
        {
            // Arrange - Create first instance
            var firstInstance = PluginCoreBehaviour.CreateInstance();
            var originalCore = firstInstance.Core;
            
            // Create new core
            var memoryManager = new MemoryManager();
            var messageBus = new MessageBus(memoryManager);
            var newCore = new PluginCore(memoryManager, messageBus);
            
            // Act - Create another instance with new core
            var secondInstance = PluginCoreBehaviour.CreateInstance(newCore);
            
            // Assert
            Assert.AreEqual(firstInstance, secondInstance, "Should return the existing instance");
            Assert.AreNotEqual(originalCore, secondInstance.Core, "Core should be updated");
            Assert.AreEqual(newCore, secondInstance.Core, "Core should be set to the new core instance");
            
            // Cleanup
            UnityEngine.Object.DestroyImmediate(firstInstance.gameObject);
        }
    }
}