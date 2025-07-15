using System.Collections.Generic;

namespace Energy8.Identity.Analytics.Core.Models
{
    /// <summary>
    /// Represents a structured analytics event
    /// </summary>
    public class AnalyticsEvent
    {
        public string Name { get; }
        public Dictionary<string, object> Parameters { get; }

        public AnalyticsEvent(string name, Dictionary<string, object> parameters = null)
        {
            Name = name;
            Parameters = parameters ?? new Dictionary<string, object>();
        }

        public AnalyticsEvent AddParameter(string key, object value)
        {
            Parameters[key] = value;
            return this;
        }
    }

    /// <summary>
    /// Predefined analytics events for Identity system
    /// </summary>
    public static class IdentityEvents
    {
        public static AnalyticsEvent SignIn(string method) => 
            new AnalyticsEvent("sign_in", new Dictionary<string, object> { { "method", method } });

        public static AnalyticsEvent SignOut() => 
            new AnalyticsEvent("sign_out");

        public static AnalyticsEvent SignUp(string method) => 
            new AnalyticsEvent("sign_up", new Dictionary<string, object> { { "method", method } });

        public static AnalyticsEvent ProviderLinked(string provider) => 
            new AnalyticsEvent("provider_linked", new Dictionary<string, object> { { "provider", provider } });

        public static AnalyticsEvent ProviderUnlinked(string provider) => 
            new AnalyticsEvent("provider_unlinked", new Dictionary<string, object> { { "provider", provider } });

        public static AnalyticsEvent EmailChanged() => 
            new AnalyticsEvent("email_changed");

        public static AnalyticsEvent NameChanged() => 
            new AnalyticsEvent("name_changed");

        public static AnalyticsEvent AccountDeleted() => 
            new AnalyticsEvent("account_deleted");

        public static AnalyticsEvent ErrorOccurred(string errorType, string errorMessage) =>
            new AnalyticsEvent("error_occurred", new Dictionary<string, object> 
            { 
                { "error_type", errorType },
                { "error_message", errorMessage }
            });
    }

    /// <summary>
    /// User properties for analytics
    /// </summary>
    public static class UserProperties
    {
        public const string DisplayName = "display_name";
        public const string Email = "email";
        public const string ProviderId = "provider_id";
        public const string AuthMethod = "auth_method";
        public const string AccountAge = "account_age";
        public const string LinkedProviders = "linked_providers";
    }
}
