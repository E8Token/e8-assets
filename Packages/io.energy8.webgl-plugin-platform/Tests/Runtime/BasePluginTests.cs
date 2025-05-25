using NUnit.Framework;
using UnityEngine;
using Energy8.WebGL.PluginPlatform;

namespace Energy8.WebGL.PluginPlatform.Tests
{
    /// <summary>
    /// Tests for BasePlugin abstract class functionality
    /// </summary>
    public class BasePluginTests
    {
        private GameObject testObject;
        private TestPlugin testPlugin;

        [SetUp]
        public void SetUp()
        {
            testObject = new GameObject("TestPlugin");
            testPlugin = testObject.AddComponent<TestPlugin>();
        }

        [TearDown]
        public void TearDown()
        {
            if (testObject != null)
            {
                Object.DestroyImmediate(testObject);
            }
        }

        [Test]
        public void BasePlugin_PluginName_ReturnsCorrectName()
        {
            // Act
            var pluginName = testPlugin.PluginName;

            // Assert
            Assert.AreEqual("TestPlugin", pluginName);
        }

        [Test]
        public void BasePlugin_Priority_CanBeSet()
        {
            // Arrange
            var newPriority = 25;

            // Act
            testPlugin.Priority = newPriority;

            // Assert
            Assert.AreEqual(newPriority, testPlugin.Priority);
        }

        [Test]
        public void BasePlugin_Priority_ClampsBetween0And100()
        {
            // Test lower bound
            testPlugin.Priority = -10;
            Assert.AreEqual(0, testPlugin.Priority);

            // Test upper bound
            testPlugin.Priority = 150;
            Assert.AreEqual(100, testPlugin.Priority);
        }

        [Test]
        public void BasePlugin_Version_CanBeSetAndGet()
        {
            // Arrange
            var newVersion = "2.1.0";

            // Act
            testPlugin.Version = newVersion;

            // Assert
            Assert.AreEqual(newVersion, testPlugin.Version);
        }

        [Test]
        public void BasePlugin_IsEnabled_DefaultsToTrue()
        {
            // Assert
            Assert.IsTrue(testPlugin.IsEnabled);
        }

        [Test]
        public void BasePlugin_IsEnabled_CanBeToggled()
        {
            // Act
            testPlugin.IsEnabled = false;

            // Assert
            Assert.IsFalse(testPlugin.IsEnabled);

            // Act again
            testPlugin.IsEnabled = true;

            // Assert
            Assert.IsTrue(testPlugin.IsEnabled);
        }

        [Test]
        public void BasePlugin_Settings_ReturnsValidSettings()
        {
            // Act
            var settings = testPlugin.Settings;

            // Assert
            Assert.IsNotNull(settings);
            Assert.IsInstanceOf<IPluginSettings>(settings);
        }

        [Test]
        public void BasePlugin_InitializeCallsOverriddenMethod()
        {
            // Act
            testPlugin.Initialize();

            // Assert
            Assert.IsTrue(testPlugin.WasInitialized);
        }

        [Test]
        public void BasePlugin_EnableCallsOverriddenMethod()
        {
            // Act
            testPlugin.Enable();

            // Assert
            Assert.IsTrue(testPlugin.WasEnabled);
        }

        [Test]
        public void BasePlugin_DisableCallsOverriddenMethod()
        {
            // Act
            testPlugin.Disable();

            // Assert
            Assert.IsTrue(testPlugin.WasDisabled);
        }

        [Test]
        public void BasePlugin_DestroyCallsOverriddenMethod()
        {
            // Act
            testPlugin.Destroy();

            // Assert
            Assert.IsTrue(testPlugin.WasDestroyed);
        }
    }
}
