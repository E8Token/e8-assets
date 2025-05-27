using UnityEngine;
using Energy8.Firebase.Analytics.Providers;

namespace Energy8.Firebase.Analytics.Providers
{
    /// <summary>
    /// Initializer for WebGL Firebase Analytics Provider
    /// This class automatically registers the WebGL provider when the assembly loads
    /// </summary>
    public static class WebFirebaseAnalyticsInitializer
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            Debug.Log("[FirebaseAnalytics] Initializing WebGL Firebase Analytics Provider");
            
            var provider = new WebFirebaseAnalyticsProvider();
            FirebaseAnalytics.RegisterProvider(provider);
            
            Debug.Log("[FirebaseAnalytics] WebGL provider registered successfully");
        }
    }
}
