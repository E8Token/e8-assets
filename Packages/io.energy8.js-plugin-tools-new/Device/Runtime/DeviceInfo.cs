using System;
using System.Threading.Tasks;
using Energy8.JSPluginTools.Communication;
using Energy8.JSPluginTools.Core;
using UnityEngine;

namespace Energy8.JSPluginTools.Device
{
    /// <summary>
    /// Implementation of the IDeviceInfo interface
    /// </summary>
    public class DeviceInfo : IDeviceInfo
    {
        private const string CHANNEL_PREFIX = "device.";

        private readonly ICommunicationService _communicationService;

        /// <summary>
        /// Creates a new DeviceInfo instance
        /// </summary>
        /// <param name="communicationService">Communication service for interacting with JavaScript</param>
        public DeviceInfo(ICommunicationService communicationService)
        {
            _communicationService = communicationService ?? throw new ArgumentNullException(nameof(communicationService));
        }

        /// <inheritdoc/>
        public Task<string> GetUserAgent()
        {
            return _communicationService.SendWithResponseAsync<string>(CHANNEL_PREFIX + "getUserAgent", string.Empty);
        }

        /// <inheritdoc/>
        public Task<BrowserInfo> GetBrowserInfo()
        {
            return _communicationService.SendWithResponseAsync<BrowserInfo>(CHANNEL_PREFIX + "getBrowserInfo", string.Empty);
        }

        /// <inheritdoc/>
        public Task<OSInfo> GetOSInfo()
        {
            return _communicationService.SendWithResponseAsync<OSInfo>(CHANNEL_PREFIX + "getOSInfo", string.Empty);
        }

        /// <inheritdoc/>
        public Task<ScreenInfo> GetScreenInfo()
        {
            return _communicationService.SendWithResponseAsync<ScreenInfo>(CHANNEL_PREFIX + "getScreenInfo", string.Empty);
        }

        /// <inheritdoc/>
        public Task<bool> IsMobileDevice()
        {
            return _communicationService.SendWithResponseAsync<bool>(CHANNEL_PREFIX + "isMobileDevice", string.Empty);
        }

        /// <inheritdoc/>
        public Task<string> GetLanguage()
        {
            return _communicationService.SendWithResponseAsync<string>(CHANNEL_PREFIX + "getLanguage", string.Empty);
        }

        /// <inheritdoc/>
        public Task<TimeZoneInfo> GetTimeZone()
        {
            return _communicationService.SendWithResponseAsync<TimeZoneInfo>(CHANNEL_PREFIX + "getTimeZone", string.Empty);
        }

        /// <inheritdoc/>
        public Task<HardwareInfo> GetHardwareInfo()
        {
            return _communicationService.SendWithResponseAsync<HardwareInfo>(CHANNEL_PREFIX + "getHardwareInfo", string.Empty);
        }
    }
}