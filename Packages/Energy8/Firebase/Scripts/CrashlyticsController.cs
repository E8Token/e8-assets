using UnityEngine;
#if UNITY_WEBGL && !UNITY_EDITOR
#else
using Firebase.Crashlytics;
#endif

namespace Energy8.Firebase
{
    public class CrashlyticsController
    {
        static readonly Logger logger = new(null, "CrashlyticsController", new Color(0.3f, 0.25f, 0.85f));
#if UNITY_WEBGL && !UNITY_EDITOR
#else
        public static void Initialize()
        {
            Crashlytics.ReportUncaughtExceptionsAsFatal = true;
            logger.Log("Initialized");
        }
#endif
    }
}