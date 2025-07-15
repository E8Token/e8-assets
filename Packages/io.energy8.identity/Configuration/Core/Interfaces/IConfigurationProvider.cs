using Energy8.Identity.Configuration.Core;

namespace Energy8.Identity.Configuration.Core.Interfaces
{
    /// <summary>
    /// Interface for configuration provider that manages Identity system settings
    /// </summary>
    public interface IConfigurationProvider
    {
        // IP Configuration
        IPType SelectedIPType { get; set; }
        string SelectedIP { get; }
        
        // Auth Configuration
        AuthType SelectedAuthType { get; set; }
        string AuthConfig { get; }
        
        // Analytics Configuration
        bool EnableAnalytics { get; }
        bool EnableDebugLogging { get; }
        bool TrackUserActions { get; }
        bool TrackErrors { get; }
        bool TrackPerformance { get; }
        
        // Validation
        bool IsValid { get; }
        string[] ValidationErrors { get; }
    }
}
