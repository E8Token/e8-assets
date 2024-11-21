#if UNITY_WEBGL && !UNITY_EDITOR
#else
using UnityEngine;
using Firebase.Analytics;
using Newtonsoft.Json;
#endif

namespace Energy8.Firebase
{
    public class AnalyticsController
    {
#if UNITY_WEBGL && !UNITY_EDITOR
#else
        static readonly Logger logger = new(null, "AnalyticsController", new Color(0.1f, 0.9f, 0.38f));
        public static void Initialize()
        {
            logger.Log("Initialized");
        }
        public static void LogEvent(string name, params (string, object)[] parameters)
        {
            logger.Log($"LogEvent({name}, {JsonConvert.SerializeObject(parameters)})");
            FirebaseAnalytics.LogEvent(name, GetParameters(parameters));
        }
        public static void LogEvent(string name)
        {
            logger.Log($"LogEvent({name})");
            FirebaseAnalytics.LogEvent(name);
        }
        public static void LogEvent(string name, string parameterName, int parameterValue)
        {
            logger.Log($"LogEvent({name}, {parameterName}, {parameterValue})");
            FirebaseAnalytics.LogEvent(name, parameterName, parameterValue);
        }
        public static void LogEvent(string name, string parameterName, long parameterValue)
        {
            logger.Log($"LogEvent({name}, {parameterName}, {parameterValue})");
            FirebaseAnalytics.LogEvent(name, parameterName, parameterValue);
        }
        public static void LogEvent(string name, string parameterName, double parameterValue)
        {
            logger.Log($"LogEvent({name}, {parameterName}, {parameterValue})");
            FirebaseAnalytics.LogEvent(name, parameterName, parameterValue);
        }
        public static void LogEvent(string name, string parameterName, string parameterValue)
        {
            logger.Log($"LogEvent({name}, {parameterName}, {parameterValue})");
            FirebaseAnalytics.LogEvent(name, parameterName, parameterValue);
        }

        public static void SetUserId(string userId)
        {
            logger.Log($"SetUserId({userId})");
            FirebaseAnalytics.SetUserId(userId);
        }
        public static void SetUserProperty(string name, string property)
        {
            logger.Log($"SetUserProperty({name}, {property})");
            FirebaseAnalytics.SetUserProperty(name, property);
        }

        static Parameter[] GetParameters((string, object)[] parameters)
        {
            Parameter[] convertedParams = new Parameter[parameters.Length];
            int i = 0;
            foreach (var p in parameters)
            {
                if (p.Item2.GetType() == typeof(long))
                    convertedParams[i] = new Parameter(p.Item1, (long)p.Item2);
                else if (p.Item2.GetType() == typeof(double))
                    convertedParams[i] = new Parameter(p.Item1, (double)p.Item2);
                else
                    convertedParams[i] = new Parameter(p.Item1, p.Item2.ToString());
                i++;
            }
            return convertedParams;
#endif
        }
    }
}