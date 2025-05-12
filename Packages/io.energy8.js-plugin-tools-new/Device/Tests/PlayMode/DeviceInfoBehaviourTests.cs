using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Energy8.JSPluginTools.Communication;

namespace Energy8.JSPluginTools.Device.Tests.PlayMode
{
    public class DeviceInfoBehaviourTests
    {
        private GameObject _gameObject;
        private DeviceInfoBehaviour _behaviour;
        private MockDeviceInfo _deviceInfo;

        // Тестовая реализация IDeviceInfo
        private class MockDeviceInfo : IDeviceInfo
        {
            public bool WasCalled { get; private set; }

            public System.Threading.Tasks.Task<string> GetUserAgent()
            {
                WasCalled = true;
                return System.Threading.Tasks.Task.FromResult("Test User Agent");
            }

            public System.Threading.Tasks.Task<BrowserInfo> GetBrowserInfo()
            {
                WasCalled = true;
                return System.Threading.Tasks.Task.FromResult(new BrowserInfo 
                { 
                    Name = "Test Browser",
                    Version = "1.0"
                });
            }

            public System.Threading.Tasks.Task<OSInfo> GetOSInfo()
            {
                WasCalled = true;
                return System.Threading.Tasks.Task.FromResult(new OSInfo 
                { 
                    Name = "Test OS",
                    Version = "1.0"
                });
            }

            public System.Threading.Tasks.Task<ScreenInfo> GetScreenInfo()
            {
                WasCalled = true;
                return System.Threading.Tasks.Task.FromResult(new ScreenInfo 
                { 
                    Width = 1920,
                    Height = 1080
                });
            }

            public System.Threading.Tasks.Task<bool> IsMobileDevice()
            {
                WasCalled = true;
                return System.Threading.Tasks.Task.FromResult(false);
            }

            public System.Threading.Tasks.Task<string> GetLanguage()
            {
                WasCalled = true;
                return System.Threading.Tasks.Task.FromResult("en-US");
            }

            public System.Threading.Tasks.Task<TimeZoneInfo> GetTimeZone()
            {
                WasCalled = true;
                return System.Threading.Tasks.Task.FromResult(new TimeZoneInfo 
                { 
                    Id = "Test/TimeZone",
                    OffsetMinutes = 0
                });
            }

            public System.Threading.Tasks.Task<HardwareInfo> GetHardwareInfo()
            {
                WasCalled = true;
                return System.Threading.Tasks.Task.FromResult(new HardwareInfo 
                { 
                    ProcessorCores = 4,
                    GpuRenderer = "Test GPU"
                });
            }
        }

        [SetUp]
        public void Setup()
        {
            _gameObject = new GameObject("TestDeviceInfoBehaviour");
            _behaviour = _gameObject.AddComponent<DeviceInfoBehaviour>();
            _deviceInfo = new MockDeviceInfo();
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
            _behaviour.Initialize(_deviceInfo);
            yield return null;

            // Assert
            Assert.AreEqual(_deviceInfo, _behaviour.Service);
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
            var instance = DeviceInfoBehaviour.CreateInstance(_deviceInfo);
            yield return null;

            // Assert
            Assert.IsNotNull(instance);
            Assert.AreEqual(_deviceInfo, instance.Service);

            // Cleanup
            UnityEngine.Object.Destroy(instance.gameObject);
        }

        [UnityTest]
        public IEnumerator CreateInstance_WhenInstanceExists_ReusesExistingInstance()
        {
            // Arrange
            _behaviour.Initialize(_deviceInfo);
            
            // Act
            var newDeviceInfo = new MockDeviceInfo();
            var newInstance = DeviceInfoBehaviour.CreateInstance(newDeviceInfo);
            yield return null;

            // Assert
            Assert.AreEqual(_behaviour, newInstance, "Should reuse existing instance");
            Assert.AreNotEqual(_deviceInfo, newInstance.Service, "Service should be updated");
            Assert.AreEqual(newDeviceInfo, newInstance.Service, "Service should be set to the new instance");
        }

        [UnityTest]
        public IEnumerator CreateInstance_AppliesDontDestroyOnLoad()
        {
            // Act
            var instance = DeviceInfoBehaviour.CreateInstance(_deviceInfo);
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
            string customName = "CustomDeviceInfoName";

            // Act
            var instance = DeviceInfoBehaviour.CreateInstance(_deviceInfo, customName);
            yield return null;

            // Assert
            Assert.AreEqual(customName, instance.gameObject.name);

            // Cleanup
            UnityEngine.Object.Destroy(instance.gameObject);
        }
    }
}