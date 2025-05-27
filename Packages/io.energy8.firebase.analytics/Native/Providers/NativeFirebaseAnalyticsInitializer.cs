using UnityEngine;
using Energy8.Firebase.Analytics.Providers;

namespace Energy8.Firebase.Analytics.Providers
{
    /// <summary>
    /// Initializer for Native Firebase Analytics Provider
    /// This class automatically registers the native provider when the assembly loads
    /// </summary>
    public static class NativeFirebaseAnalyticsInitializer
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            Debug.Log("[FirebaseAnalytics] Initializing Native Firebase Analytics Provider");
            
            var provider = new NativeFirebaseAnalyticsProvider();
            FirebaseAnalytics.RegisterProvider(provider);
            
            Debug.Log("[FirebaseAnalytics] Native provider registered successfully");
        }
    }
}
