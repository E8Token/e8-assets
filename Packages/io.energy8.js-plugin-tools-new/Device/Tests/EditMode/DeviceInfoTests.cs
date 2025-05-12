using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Energy8.JSPluginTools.Core;
using Energy8.JSPluginTools.Communication;

namespace Energy8.JSPluginTools.Device.Tests.EditMode
{
    public class DeviceInfoTests
    {
        private MockCommunicationService _communicationService;
        private DeviceInfo _deviceInfo;

        [SetUp]
        public void Setup()
        {
            _communicationService = new MockCommunicationService();
            _deviceInfo = new DeviceInfo(_communicationService);
        }

        // Тестовая реализация ICommunicationService
        private class MockCommunicationService : ICommunicationService
        {
            public string LastChannel { get; private set; }
            public object LastData { get; private set; }
            public bool WasCalled { get; private set; }
            public object ResponseToReturn { get; set; }

            public IMessageBus MessageBus => null; // Не используется в тестах

            public void Send(string channel, string data)
            {
                LastChannel = channel;
                LastData = data;
                WasCalled = true;
            }

            public void Send<T>(string channel, T data)
            {
                LastChannel = channel;
                LastData = data;
                WasCalled = true;
            }

            public Task<TResponse> SendWithResponseAsync<TResponse>(string channel, string data, int timeout = 5000)
            {
                LastChannel = channel;
                LastData = data;
                WasCalled = true;
                return Task.FromResult((TResponse)ResponseToReturn);
            }

            public Task<TResponse> SendWithResponseAsync<TRequest, TResponse>(string channel, TRequest data, int timeout = 5000)
            {
                LastChannel = channel;
                LastData = data;
                WasCalled = true;
                return Task.FromResult((TResponse)ResponseToReturn);
            }

            public void Subscribe(string channel, Action<string> handler) { }
            public void Subscribe<T>(string channel, Action<T> handler) { }
            public void Unsubscribe(string channel) { }
        }

        [Test]
        public void Constructor_NullCommunicationService_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new DeviceInfo(null));
        }

        [Test]
        public async Task GetUserAgent_CallsCommunicationService()
        {
            // Arrange
            string expectedResponse = "Mozilla/5.0 (Windows NT 10.0; Win64; x64)";
            _communicationService.ResponseToReturn = expectedResponse;

            // Act
            var result = await _deviceInfo.GetUserAgent();

            // Assert
            Assert.IsTrue(_communicationService.WasCalled, "Communication service should be called");
            Assert.AreEqual("device.getUserAgent", _communicationService.LastChannel);
            Assert.AreEqual(expectedResponse, result);
        }

        [Test]
        public async Task GetBrowserInfo_CallsCommunicationService()
        {
            // Arrange
            var expectedResponse = new BrowserInfo
            {
                Name = "Chrome",
                Version = "100.0.0",
                Engine = "Blink",
                CookiesEnabled = true,
                JavaScriptEnabled = true
            };
            _communicationService.ResponseToReturn = expectedResponse;

            // Act
            var result = await _deviceInfo.GetBrowserInfo();

            // Assert
            Assert.IsTrue(_communicationService.WasCalled, "Communication service should be called");
            Assert.AreEqual("device.getBrowserInfo", _communicationService.LastChannel);
            Assert.AreEqual(expectedResponse.Name, result.Name);
            Assert.AreEqual(expectedResponse.Version, result.Version);
            Assert.AreEqual(expectedResponse.Engine, result.Engine);
            Assert.AreEqual(expectedResponse.CookiesEnabled, result.CookiesEnabled);
            Assert.AreEqual(expectedResponse.JavaScriptEnabled, result.JavaScriptEnabled);
        }

        [Test]
        public async Task GetOSInfo_CallsCommunicationService()
        {
            // Arrange
            var expectedResponse = new OSInfo
            {
                Name = "Windows",
                Version = "10",
                Architecture = "x64",
                IsMobile = false
            };
            _communicationService.ResponseToReturn = expectedResponse;

            // Act
            var result = await _deviceInfo.GetOSInfo();

            // Assert
            Assert.IsTrue(_communicationService.WasCalled, "Communication service should be called");
            Assert.AreEqual("device.getOSInfo", _communicationService.LastChannel);
            Assert.AreEqual(expectedResponse.Name, result.Name);
            Assert.AreEqual(expectedResponse.Version, result.Version);
            Assert.AreEqual(expectedResponse.Architecture, result.Architecture);
            Assert.AreEqual(expectedResponse.IsMobile, result.IsMobile);
        }

        [Test]
        public async Task GetScreenInfo_CallsCommunicationService()
        {
            // Arrange
            var expectedResponse = new ScreenInfo
            {
                Width = 1920,
                Height = 1080,
                PixelRatio = 1.0f,
                ColorDepth = 24,
                TouchScreen = false,
                RefreshRate = 60
            };
            _communicationService.ResponseToReturn = expectedResponse;

            // Act
            var result = await _deviceInfo.GetScreenInfo();

            // Assert
            Assert.IsTrue(_communicationService.WasCalled, "Communication service should be called");
            Assert.AreEqual("device.getScreenInfo", _communicationService.LastChannel);
            Assert.AreEqual(expectedResponse.Width, result.Width);
            Assert.AreEqual(expectedResponse.Height, result.Height);
            Assert.AreEqual(expectedResponse.PixelRatio, result.PixelRatio);
            Assert.AreEqual(expectedResponse.ColorDepth, result.ColorDepth);
            Assert.AreEqual(expectedResponse.TouchScreen, result.TouchScreen);
            Assert.AreEqual(expectedResponse.RefreshRate, result.RefreshRate);
        }

        [Test]
        public async Task IsMobileDevice_CallsCommunicationService()
        {
            // Arrange
            bool expectedResponse = false;
            _communicationService.ResponseToReturn = expectedResponse;

            // Act
            var result = await _deviceInfo.IsMobileDevice();

            // Assert
            Assert.IsTrue(_communicationService.WasCalled, "Communication service should be called");
            Assert.AreEqual("device.isMobileDevice", _communicationService.LastChannel);
            Assert.AreEqual(expectedResponse, result);
        }

        [Test]
        public async Task GetLanguage_CallsCommunicationService()
        {
            // Arrange
            string expectedResponse = "en-US";
            _communicationService.ResponseToReturn = expectedResponse;

            // Act
            var result = await _deviceInfo.GetLanguage();

            // Assert
            Assert.IsTrue(_communicationService.WasCalled, "Communication service should be called");
            Assert.AreEqual("device.getLanguage", _communicationService.LastChannel);
            Assert.AreEqual(expectedResponse, result);
        }

        [Test]
        public async Task GetTimeZone_CallsCommunicationService()
        {
            // Arrange
            var expectedResponse = new TimeZoneInfo
            {
                Id = "Europe/London",
                DisplayName = "GMT+0",
                OffsetMinutes = 0,
                DaylightSavingTime = false
            };
            _communicationService.ResponseToReturn = expectedResponse;

            // Act
            var result = await _deviceInfo.GetTimeZone();

            // Assert
            Assert.IsTrue(_communicationService.WasCalled, "Communication service should be called");
            Assert.AreEqual("device.getTimeZone", _communicationService.LastChannel);
            Assert.AreEqual(expectedResponse.Id, result.Id);
            Assert.AreEqual(expectedResponse.DisplayName, result.DisplayName);
            Assert.AreEqual(expectedResponse.OffsetMinutes, result.OffsetMinutes);
            Assert.AreEqual(expectedResponse.DaylightSavingTime, result.DaylightSavingTime);
        }

        [Test]
        public async Task GetHardwareInfo_CallsCommunicationService()
        {
            // Arrange
            var expectedResponse = new HardwareInfo
            {
                ProcessorCores = 8,
                DeviceMemory = 16,
                GpuRenderer = "NVIDIA GeForce RTX 3080",
                GpuVendor = "NVIDIA Corporation",
                HasBattery = false,
                BatteryLevel = -1
            };
            _communicationService.ResponseToReturn = expectedResponse;

            // Act
            var result = await _deviceInfo.GetHardwareInfo();

            // Assert
            Assert.IsTrue(_communicationService.WasCalled, "Communication service should be called");
            Assert.AreEqual("device.getHardwareInfo", _communicationService.LastChannel);
            Assert.AreEqual(expectedResponse.ProcessorCores, result.ProcessorCores);
            Assert.AreEqual(expectedResponse.DeviceMemory, result.DeviceMemory);
            Assert.AreEqual(expectedResponse.GpuRenderer, result.GpuRenderer);
            Assert.AreEqual(expectedResponse.GpuVendor, result.GpuVendor);
            Assert.AreEqual(expectedResponse.HasBattery, result.HasBattery);
            Assert.AreEqual(expectedResponse.BatteryLevel, result.BatteryLevel);
        }
    }
}