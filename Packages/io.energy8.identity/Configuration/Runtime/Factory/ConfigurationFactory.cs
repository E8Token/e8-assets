using Energy8.Identity.Configuration.Core;
using Energy8.Identity.Configuration.Core.Interfaces;
using UnityEngine;

namespace Energy8.Identity.Configuration.Runtime.Factory
{
    /// <summary>
    /// Factory for creating configuration providers
    /// </summary>
    public static class ConfigurationFactory
    {
        /// <summary>
        /// Creates the default configuration provider (IdentityConfiguration)
        /// </summary>
        /// <returns>Configuration provider instance</returns>
        public static IConfigurationProvider CreateProvider()
        {
            var config = IdentityConfiguration.Instance;
            
            if (config == null)
            {
                Debug.LogError("IdentityConfiguration not found! Please create one via Assets > Create > Identity > Configuration");
                return new DefaultConfigurationProvider();
            }
            
            // Create adapter to wrap static IdentityConfiguration
            return new IdentityConfigurationAdapter(config);
        }
        
        /// <summary>
        /// Creates a configuration provider for testing
        /// </summary>
        /// <returns>Test configuration provider</returns>
        public static IConfigurationProvider CreateTestProvider()
        {
            return new DefaultConfigurationProvider();
        }
    }
    
    /// <summary>
    /// Adapter to wrap static IdentityConfiguration as IConfigurationProvider
    /// </summary>
    public class IdentityConfigurationAdapter : IConfigurationProvider
    {
        private readonly IdentityConfiguration config;
        
        public IdentityConfigurationAdapter(IdentityConfiguration config)
        {
            this.config = config;
        }
        
        public IPType SelectedIPType 
        { 
            get => IdentityConfiguration.SelectedIPType; 
            set => IdentityConfiguration.SelectedIPType = value; 
        }
        
        public string SelectedIP => IdentityConfiguration.SelectedIP;
        
        public AuthType SelectedAuthType 
        { 
            get => IdentityConfiguration.SelectedAuthType; 
            set => IdentityConfiguration.SelectedAuthType = value; 
        }
        
        public string AuthConfig => IdentityConfiguration.AuthConfig;
        public bool EnableAnalytics => IdentityConfiguration.EnableAnalytics;
        public bool EnableDebugLogging => IdentityConfiguration.EnableDebugLogging;
        public bool TrackUserActions => IdentityConfiguration.TrackUserActions;
        public bool TrackErrors => IdentityConfiguration.TrackErrors;
        public bool TrackPerformance => IdentityConfiguration.TrackPerformance;
        public bool IsValid => config.IsValid;
        public string[] ValidationErrors => config.ValidationErrors;
    }
    
    /// <summary>
    /// Default configuration provider for fallback scenarios
    /// </summary>
    public class DefaultConfigurationProvider : IConfigurationProvider
    {
        public IPType SelectedIPType { get; set; } = IPType.LocalPC;
        public string SelectedIP => "http://localhost";
        public AuthType SelectedAuthType { get; set; } = AuthType.Local;
        public string AuthConfig => "{}";
        public bool EnableAnalytics => false;
        public bool EnableDebugLogging => true;
        public bool TrackUserActions => false;
        public bool TrackErrors => false;
        public bool TrackPerformance => false;
        public bool IsValid => true;
        public string[] ValidationErrors => new string[0];
    }
}
