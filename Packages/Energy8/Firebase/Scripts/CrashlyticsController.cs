#if UNITY_ANDROID || UNITY_IOS
using Firebase;
using Firebase.Crashlytics;
#endif
using UnityEngine;

namespace Energy8.Firebase
{
    public class CrashlyticsController
    {
        static readonly Logger logger = new(null, "CrashlyticsController", new Color(0.3f, 0.25f, 0.85f));
#if UNITY_ANDROID || UNITY_IOS
        public static void Initialize()
        {
            Crashlytics.ReportUncaughtExceptionsAsFatal = true;
            logger.Log("Initialized");
        }
#endif
    }
}