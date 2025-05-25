using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Energy8.WebGL.PluginPlatform;

namespace Energy8.WebGL.PluginPlatform.Tests
{
    /// <summary>
    /// Runtime tests for PluginManager core functionality
    /// </summary>
    public class PluginManagerTests
    {
        private GameObject testObject;
        private TestPlugin testPlugin;

        [SetUp]
        public void SetUp()
        {
            // Create test plugin
            testObject = new GameObject("TestPlugin");
            testPlugin = testObject.AddComponent<TestPlugin>();
        }
        [TearDown]
        public void TearDown()
        {
            // Unregister test plugin if it was registered
            if (testPlugin != null && PluginManager.Instance != null)
            {
                PluginManager.Instance.UnregisterPlugin(testPlugin);
            }

            if (testObject != null)
            {
                Object.DestroyImmediate(testObject);
            }
        }

        [Test]
        public void PluginManager_Instance_IsNotNull()
        {
            // Act & Assert
            Assert.IsNotNull(PluginManager.Instance);
        }

        [Test]
        public void PluginManager_IsSingleton()
        {
            // Arrange
            var instance1 = PluginManager.Instance;
            var instance2 = PluginManager.Instance;

            // Assert
            Assert.AreSame(instance1, instance2);
        }

        [Test]
        public void PluginManager_RegisterPlugin_AddsToRegisteredPlugins()
        {
            // Arrange
            var manager = PluginManager.Instance;

            // Ensure plugin is not already registered (in case auto-discovery found it)
            if (manager.RegisteredPlugins.ContainsKey(testPlugin.PluginName))
            {
                manager.UnregisterPlugin(testPlugin);
            }

            var initialCount = manager.RegisteredPlugins.Count;

            // Act
            manager.RegisterPlugin(testPlugin);

            // Assert
            Assert.AreEqual(initialCount + 1, manager.RegisteredPlugins.Count);
            Assert.IsTrue(manager.RegisteredPlugins.ContainsKey(testPlugin.PluginName));
        }

        [Test]
        public void PluginManager_UnregisterPlugin_RemovesFromRegisteredPlugins()
        {
            // Arrange
            var manager = PluginManager.Instance;

            // Ensure plugin is registered (handle both auto-discovery and manual registration)
            if (!manager.RegisteredPlugins.ContainsKey(testPlugin.PluginName))
            {
                manager.RegisterPlugin(testPlugin);
            }

            var countAfterRegistration = manager.RegisteredPlugins.Count;

            // Act
            manager.UnregisterPlugin(testPlugin);

            // Assert
            Assert.AreEqual(countAfterRegistration - 1, manager.RegisteredPlugins.Count);
            Assert.IsFalse(manager.RegisteredPlugins.ContainsKey(testPlugin.PluginName));
        }

        [Test]
        public void PluginManager_EnablePlugin_SetsEnabledState()
        {
            // Arrange
            var manager = PluginManager.Instance;

            // Ensure plugin is registered
            if (!manager.RegisteredPlugins.ContainsKey(testPlugin.PluginName))
            {
                manager.RegisterPlugin(testPlugin);
            }

            testPlugin.IsEnabled = false;

            // Act
            manager.EnablePlugin(testPlugin.PluginName);

            // Assert
            Assert.IsTrue(testPlugin.IsEnabled);
            Assert.IsTrue(testPlugin.WasEnabled);
        }

        [Test]
        public void PluginManager_DisablePlugin_SetsDisabledState()
        {
            // Arrange
            var manager = PluginManager.Instance;

            // Ensure plugin is registered
            if (!manager.RegisteredPlugins.ContainsKey(testPlugin.PluginName))
            {
                manager.RegisterPlugin(testPlugin);
            }

            testPlugin.IsEnabled = true;

            // Act
            manager.DisablePlugin(testPlugin.PluginName);

            // Assert
            Assert.IsFalse(testPlugin.IsEnabled);
            Assert.IsTrue(testPlugin.WasDisabled);
        }

        [Test]
        public void PluginManager_CallPluginMethod_ReturnsCorrectResult()
        {
            // Arrange
            var manager = PluginManager.Instance;

            // Ensure plugin is registered
            if (!manager.RegisteredPlugins.ContainsKey(testPlugin.PluginName))
            {
                manager.RegisterPlugin(testPlugin);
            }

            testPlugin.IsEnabled = true;

            // Act
            var result = manager.CallPluginMethod("TestPlugin", "GetTestMessage", "{\"name\":\"Unity\"}");

            // Assert
            Assert.IsNotNull(result);
            StringAssert.Contains("success", result);
            StringAssert.Contains("Hello, Unity", result);
        }
        [Test]
        public void PluginManager_CallPluginMethod_WithDisabledPlugin_ReturnsError()
        {
            // Arrange
            var manager = PluginManager.Instance;

            // Ensure plugin is registered
            if (!manager.RegisteredPlugins.ContainsKey(testPlugin.PluginName))
            {
                manager.RegisterPlugin(testPlugin);
            }

            testPlugin.IsEnabled = false;

            // Act
            var result = manager.CallPluginMethod("TestPlugin", "GetTestMessage", "{}");

            // Assert
            Assert.IsNotNull(result);
            StringAssert.Contains("Plugin disabled", result);
        }

        [Test]
        public void PluginManager_CallPluginMethod_WithNonExistentPlugin_ReturnsError()
        {
            // Arrange
            var manager = PluginManager.Instance;

            // Act
            var result = manager.CallPluginMethod("NonExistentPlugin", "SomeMethod", "{}");

            // Assert
            Assert.IsNotNull(result);
            StringAssert.Contains("Plugin not found", result);
        }
        [UnityTest]
        public IEnumerator PluginManager_CallPluginMethodAsync_ReturnsCorrectResult()
        {
            // Arrange
            var manager = PluginManager.Instance;

            // Ensure plugin is registered
            if (!manager.RegisteredPlugins.ContainsKey(testPlugin.PluginName))
            {
                manager.RegisterPlugin(testPlugin);
            }

            testPlugin.IsEnabled = true;
            string callbackResult = null;

            // Mock callback
            var callbackId = manager.RegisterCallback((result) =>
            {
                callbackResult = result;
            });

            // Act
            var result = manager.CallPluginMethodAsync("TestPlugin", "GetAsyncTestMessage", "{\"name\":\"Unity\"}", callbackId);

            // Wait for async result
            yield return new WaitForSeconds(0.1f);

            // Assert
            Assert.IsNotNull(result);
            StringAssert.Contains("Async call initiated", result);
            // Note: Callback result testing would require full WebGL build
        }
    }

    /// <summary>
    /// Test plugin for testing purposes
    /// </summary>
    public class TestPlugin : BasePlugin
    {
        private TestPluginSettings settings = new TestPluginSettings();

        public bool WasInitialized { get; private set; }
        public bool WasEnabled { get; private set; }
        public bool WasDisabled { get; private set; }
        public bool WasDestroyed { get; private set; }

        public override IPluginSettings Settings => settings;

        public override void Initialize()
        {
            WasInitialized = true;
            Debug.Log($"[{PluginName}] Test plugin initialized");
        }

        public override void Enable()
        {
            WasEnabled = true;
            Debug.Log($"[{PluginName}] Test plugin enabled");
        }

        public override void Disable()
        {
            WasDisabled = true;
            Debug.Log($"[{PluginName}] Test plugin disabled");
        }

        public override void Destroy()
        {
            WasDestroyed = true;
            Debug.Log($"[{PluginName}] Test plugin destroyed");
        }

        [JSCallable]
        public string GetTestMessage(string name)
        {
            return $"Hello, {name}! Test plugin is working.";
        }

        [JSCallable]
        public async System.Threading.Tasks.Task<string> GetAsyncTestMessage(string name)
        {
            await System.Threading.Tasks.Task.Delay(50);
            return $"Async Hello, {name}! Test plugin completed.";
        }

        [JSCallable]
        public bool SetTestMode(bool enabled)
        {
            settings.TestMode = enabled;
            return true;
        }
    }

    /// <summary>
    /// Test settings for test plugin
    /// </summary>
    [System.Serializable]
    public class TestPluginSettings : IPluginSettings
    {
        public bool TestMode { get; set; } = false;

        public string ToJson()
        {
            return JsonUtility.ToJson(this);
        }

        public void FromJson(string json)
        {
            JsonUtility.FromJsonOverwrite(json, this);
        }

        public void ResetToDefaults()
        {
            TestMode = false;
        }
    }
}
