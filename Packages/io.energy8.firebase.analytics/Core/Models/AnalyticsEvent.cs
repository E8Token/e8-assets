using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Energy8.Firebase.Analytics.Models
{
    [Serializable]
    public class AnalyticsEvent
    {
        // Firebase Analytics limits
        public const int MaxParametersPerEvent = 25;
        public const int MaxStringParameterLength = 100;
        public const int MaxEventNameLength = 40;
        public const int MaxParameterNameLength = 40;

        private static readonly Regex ValidNamePattern = new Regex(@"^[a-zA-Z_][a-zA-Z0-9_]*$");

        public string Name { get; set; }
        public Dictionary<string, object> Parameters { get; set; }
        public DateTime Timestamp { get; set; }

        public AnalyticsEvent()
        {
            Parameters = new Dictionary<string, object>();
            Timestamp = DateTime.UtcNow;
        }

        public AnalyticsEvent(string name) : this()
        {
            Name = name;
        }

        public AnalyticsEvent(string name, Dictionary<string, object> parameters) : this(name)
        {
            if (parameters != null)
            {
                // Limit parameters to maximum allowed
                var limitedParams = parameters.Take(MaxParametersPerEvent).ToDictionary(
                    kvp => kvp.Key,
                    kvp => TruncateStringValue(kvp.Value)
                );
                Parameters = limitedParams;
            }
        }

        /// <summary>
        /// Validate event name according to Firebase Analytics rules
        /// </summary>
        public static bool IsValidEventName(string eventName)
        {
            if (string.IsNullOrEmpty(eventName))
                return false;
            
            if (eventName.Length > MaxEventNameLength)
                return false;
            
            return ValidNamePattern.IsMatch(eventName);
        }

        /// <summary>
        /// Validate parameter name according to Firebase Analytics rules
        /// </summary>
        public static bool IsValidParameterName(string parameterName)
        {
            if (string.IsNullOrEmpty(parameterName))
                return false;
            
            if (parameterName.Length > MaxParameterNameLength)
                return false;
            
            return ValidNamePattern.IsMatch(parameterName);
        }

        /// <summary>
        /// Truncate string values to maximum allowed length
        /// </summary>
        private static object TruncateStringValue(object value)
        {
            if (value is string stringValue && stringValue.Length > MaxStringParameterLength)
            {
                return stringValue.Substring(0, MaxStringParameterLength);
            }
            return value;
        }
    }
}
